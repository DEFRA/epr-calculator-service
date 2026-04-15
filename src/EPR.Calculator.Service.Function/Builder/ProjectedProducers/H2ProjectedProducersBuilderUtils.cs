namespace EPR.Calculator.Service.Function.Builder.ProjectedProducers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Mappers;
    using EPR.Calculator.Service.Function.Misc;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Services;
    using EPR.Calculator.Service.Function.Builder.Summary.Common;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;


    public static class H2ProjectedProducersBuilderUtils
    {
        public static async Task<List<CalcResultH2ProjectedProducer>> GetProjectedProducers(List<ProducerReportedMaterialsForSubmissionPeriod> reportedMaterials, List<MaterialDetail> materials)
        {
            return reportedMaterials.Select(rm => new CalcResultH2ProjectedProducer
            {
                ProducerId = rm.ProducerId,
                SubsidiaryId = rm.SubsidiaryId,
                Level = string.Empty,  // Level will be set later when subtotals are added
                SubmissionPeriodCode = rm.SubmissionPeriod,
                ProjectedTonnageByMaterial = GetProjectedTonnages(materials, rm.ReportedMaterials)
            }).ToList();
        }

        private static Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage> GetProjectedTonnages(List<MaterialDetail> materials, List<ProducerReportedMaterial> reportedMaterials)
        {
            return materials.ToDictionary(m => m.Code, m => GetProjectedTonnage(m, reportedMaterials.Where(rm => rm.MaterialId == m.Id).ToList()));
        }

        private static CalcResultH2ProjectedProducerMaterialTonnage GetProjectedTonnage(MaterialDetail material, List<ProducerReportedMaterial> reportedMaterials)
        {
            var householdRAMTonnage = CalcResultProjectedProducersBuilder.GetRAMTonnage(PackagingTypes.Household, reportedMaterials);
            var publicBinRAMTonnage = CalcResultProjectedProducersBuilder.GetRAMTonnage(PackagingTypes.PublicBin, reportedMaterials);
            var hdcRAMTonnage = (material.Code == MaterialCodes.Glass) ? CalcResultProjectedProducersBuilder.GetRAMTonnage(PackagingTypes.HouseholdDrinksContainers, reportedMaterials) : null;

            return new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdRAMTonnage = householdRAMTonnage,
                PublicBinRAMTonnage = publicBinRAMTonnage,
                HouseholdDrinksContainerRAMTonnage = hdcRAMTonnage,
                HouseholdTonnageDefaultedRed = CalcResultProjectedProducersBuilder.TonnageWithoutRAM(householdRAMTonnage),
                PublicBinTonnageDefaultedRed = CalcResultProjectedProducersBuilder.TonnageWithoutRAM(publicBinRAMTonnage),
                HouseholdDrinksContainerDefaultedRed = (hdcRAMTonnage != null) ? CalcResultProjectedProducersBuilder.TonnageWithoutRAM(hdcRAMTonnage) : null,
                TotalTonnage = householdRAMTonnage.Tonnage + publicBinRAMTonnage.Tonnage + (hdcRAMTonnage?.Tonnage ?? 0)
            };
        }
        public static CalcResultH2ProjectedProducer CreateParentProducer(CalcResultH2ProjectedProducer p)
        {
            return new CalcResultH2ProjectedProducer()
            {
                ProducerId = p.ProducerId,
                SubsidiaryId = null,
                Level = CommonConstants.LevelOne.ToString(),
                SubmissionPeriodCode = p.SubmissionPeriodCode,
                IsSubtotal = true,
                ProjectedTonnageByMaterial = p.ProjectedTonnageByMaterial
            };
        }

        public static CalcResultH2ProjectedProducer SumProducerGroupTonnages(IEnumerable<CalcResultH2ProjectedProducer> prodGroup)
        {
            var producer = prodGroup.First();
            var sumRam = (string matKey, Func<CalcResultH2ProjectedProducerMaterialTonnage, RAMTonnage?> tonnageFunc) => 
                CalcResultProjectedProducersBuilder.SumRAMTonnages<CalcResultH2ProjectedProducer, CalcResultH2ProjectedProducerMaterialTonnage>(prodGroup.ToList(), matKey, tonnageFunc);
            
            return new CalcResultH2ProjectedProducer
            {
                ProducerId = producer.ProducerId,
                SubsidiaryId = null,
                Level = CommonConstants.LevelOne.ToString(),
                SubmissionPeriodCode = producer.SubmissionPeriodCode,
                IsSubtotal = true,
                ProjectedTonnageByMaterial = producer.ProjectedTonnageByMaterial.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => new CalcResultH2ProjectedProducerMaterialTonnage {
                        HouseholdRAMTonnage = sumRam(kvp.Key, p => p.HouseholdRAMTonnage),
                        PublicBinRAMTonnage = sumRam(kvp.Key, p => p.PublicBinRAMTonnage),
                        HouseholdDrinksContainerRAMTonnage = kvp.Key == MaterialCodes.Glass ? sumRam(kvp.Key, p => p.HouseholdDrinksContainerRAMTonnage) : null,
                        HouseholdTonnageDefaultedRed = prodGroup.Sum(p => p.ProjectedTonnageByMaterial[kvp.Key].HouseholdTonnageDefaultedRed),
                        PublicBinTonnageDefaultedRed = prodGroup.Sum(p => p.ProjectedTonnageByMaterial[kvp.Key].PublicBinTonnageDefaultedRed),
                        HouseholdDrinksContainerDefaultedRed = kvp.Key == MaterialCodes.Glass ? prodGroup.Sum(p => p.ProjectedTonnageByMaterial[kvp.Key].HouseholdDrinksContainerDefaultedRed ?? 0) : null,
                        TotalTonnage = prodGroup.Sum(p => p.ProjectedTonnageByMaterial[kvp.Key].TotalTonnage) 
                    })
            };
        }


        public static ProjectedProducersHeaders GetProjectedProducerHeaders(IEnumerable<MaterialDetail> materials)
        {
            return new ProjectedProducersHeaders {
                TitleHeader = new ProjectedProducersHeader
                {
                    Name = CalcResultProjectedProducersHeaders.H2ProjectedProducers,
                    ColumnIndex = 1,
                },
                MaterialBreakdownHeaders = GetMaterialsBreakdownHeader(materials),
                ColumnHeaders = GetColumnHeaders(materials)
            };
        }

        private static List<ProjectedProducersHeader> GetMaterialsBreakdownHeader(IEnumerable<MaterialDetail> materials)
        {
            var materialsBreakdownHeaders = new List<ProjectedProducersHeader>();
            var columnIndex = GetInitialColumnHeaders().Count + 1;

            foreach (var material in materials)
            {
                materialsBreakdownHeaders.Add(new ProjectedProducersHeader
                {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = columnIndex,
                });

                var materialHeaderCount = GetMaterialColumnHeaders().Count + GetPostFixColumnHeaders().Count;

                columnIndex = material.Code == MaterialCodes.Glass
                    ? columnIndex + materialHeaderCount + GetGlassColumnHeaders().Count
                    : columnIndex + materialHeaderCount;
            }

            return materialsBreakdownHeaders;
        }

        private static List<ProjectedProducersHeader> GetColumnHeaders(IEnumerable<MaterialDetail> materials)
        {
            var columnHeaders = new List<ProjectedProducersHeader>();

            columnHeaders.AddRange(GetInitialColumnHeaders());

            foreach (var material in materials)
            {
                columnHeaders.AddRange(GetMaterialColumnHeaders());

                if (material.Code == MaterialCodes.Glass)
                {
                    columnHeaders.AddRange(GetGlassColumnHeaders());
                }

                columnHeaders.AddRange(GetPostFixColumnHeaders());
            }

            return columnHeaders;
        }

        private static List<ProjectedProducersHeader> GetInitialColumnHeaders()
        {
            return new List<ProjectedProducersHeader>
            {
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.ProducerId },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.SubsidiaryId },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.Level },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.SubmissionPeriodCode }
            };
        }

        private static List<ProjectedProducersHeader> GetPostFixColumnHeaders()
        {
            return new List<ProjectedProducersHeader>
            {
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.TotalTonnage }
            };
        }


        private static List<ProjectedProducersHeader> GetMaterialColumnHeaders()
        {
            return new List<ProjectedProducersHeader>
            {
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdPackagingTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdRedTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdAmberTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdGreenTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdRedMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdAmberMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdGreenMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdTonnageWithoutRAMDefaultedToRed },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinPackagingTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinRedTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinAmberTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinGreenTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinRedMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinAmberMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinGreenMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinTonnageWithoutRAMDefaultedToRed }
            };
        }

        private static List<ProjectedProducersHeader> GetGlassColumnHeaders()
        {
            return new List<ProjectedProducersHeader>
            {
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersPackagingTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersTonnageWithoutRAMDefaultedToRed }
            };
        }
    }
}