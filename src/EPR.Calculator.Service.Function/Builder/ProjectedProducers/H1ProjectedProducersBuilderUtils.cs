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


    public static class H1ProjectedProducersBuilderUtils
    {
        public static List<CalcResultH1ProjectedProducer> GetProjectedProducers(List<ProducerReportedMaterialsForSubmissionPeriod> reportedMaterials, List<CalcResultH2ProjectedProducer> h2ProjectedProducers, List<MaterialDetail> materials)
        {
            return reportedMaterials.Select(rm => new CalcResultH1ProjectedProducer
            {
                ProducerId = rm.ProducerId,
                SubsidiaryId = rm.SubsidiaryId,
                Level = string.Empty, // Level will be set later when subtotals are added
                SubmissionPeriodCode = rm.SubmissionPeriod,
                ProjectedTonnageByMaterial = GetProjectedTonnages(
                    materials,
                    rm.ReportedMaterials,
                    h2ProjectedProducers.FirstOrDefault(p => p.ProducerId == rm.ProducerId && p.SubsidiaryId == rm.SubsidiaryId)
                )
            }).ToList();
        }

        private static Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage> GetProjectedTonnages(List<MaterialDetail> materials, List<ProducerReportedMaterial> reportedMaterials, CalcResultH2ProjectedProducer? h2ProjectedProducer)
        {
            return materials.ToDictionary(m => m.Code, m => GetProjectedTonnage(m, reportedMaterials.Where(rm => rm.MaterialId == m.Id).ToList(), h2ProjectedProducer));
        }

        private static CalcResultH1ProjectedProducerMaterialTonnage GetProjectedTonnage(MaterialDetail material, List<ProducerReportedMaterial> reportedMaterials, CalcResultH2ProjectedProducer? h2ProjectedProducer)
        {
            if(!reportedMaterials.Any())
            {
                return GetEmptyH1MaterialTonnage(material.Code);
            }

            var householdRAMTonnage = CalcResultProjectedProducersBuilder.GetRAMTonnage(PackagingTypes.Household, reportedMaterials);
            var publicBinRAMTonnage = CalcResultProjectedProducersBuilder.GetRAMTonnage(PackagingTypes.PublicBin, reportedMaterials);
            var hdcRAMTonnage = (material.Code == MaterialCodes.Glass) ? CalcResultProjectedProducersBuilder.GetRAMTonnage(PackagingTypes.HouseholdDrinksContainers, reportedMaterials) : null;

            var householdTonnageWithoutRAM = CalcResultProjectedProducersBuilder.TonnageWithoutRAM(householdRAMTonnage);
            var publicBinTonnageWithoutRAM = CalcResultProjectedProducersBuilder.TonnageWithoutRAM(publicBinRAMTonnage);
            decimal? householdDrinksContainerTonnageWithoutRAM = (hdcRAMTonnage != null) ? CalcResultProjectedProducersBuilder.TonnageWithoutRAM(hdcRAMTonnage) : null;
            
            var h2ProjectedTonnage = h2ProjectedProducer != null ? h2ProjectedProducer.ProjectedTonnageByMaterial[material.Code] : GetEmptyH2MaterialTonnage(material.Code);
            var h2RamProportions = new RAMProportions{
                Red = GetH2RAMProportion(h2ProjectedTonnage, t => t.GetTotalRedTonnage()),
                Amber = GetH2RAMProportion(h2ProjectedTonnage, t => t.GetTotalAmberTonnage()),
                Green = GetH2RAMProportion(h2ProjectedTonnage, t => t.GetTotalGreenTonnage()),
                RedMedical = GetH2RAMProportion(h2ProjectedTonnage, t => t.GetTotalRedMedicalTonnage()),
                AmberMedical = GetH2RAMProportion(h2ProjectedTonnage, t => t.GetTotalAmberMedicalTonnage()),
                GreenMedical = GetH2RAMProportion(h2ProjectedTonnage, t => t.GetTotalGreenMedicalTonnage())
            };
            var h1ProportionateRAMTonnage = (RAMTonnage ramTonnage, decimal tonnageWithoutRAM, decimal h2TotalTonnage) => GetProjectedTonnage(ramTonnage, tonnageWithoutRAM, h2RamProportions, h2TotalTonnage);

            return new CalcResultH1ProjectedProducerMaterialTonnage
            {
                HouseholdRAMTonnage = householdRAMTonnage,
                PublicBinRAMTonnage = publicBinRAMTonnage,
                HouseholdDrinksContainerRAMTonnage = hdcRAMTonnage,
                HouseholdTonnageWithoutRAM = householdTonnageWithoutRAM,
                PublicBinTonnageWithoutRAM = publicBinTonnageWithoutRAM,
                HouseholdDrinksContainerTonnageWithoutRAM = householdDrinksContainerTonnageWithoutRAM,
                H2RamProportions = h2RamProportions,
                TotalTonnage = householdRAMTonnage.Tonnage + publicBinRAMTonnage.Tonnage + (hdcRAMTonnage?.Tonnage ?? 0),
                H2TotalTonnage = h2ProjectedTonnage.TotalTonnage,
                ProjectedHouseholdRAMTonnage = h1ProportionateRAMTonnage(householdRAMTonnage, householdTonnageWithoutRAM, h2ProjectedTonnage.TotalTonnage),
                ProjectedPublicBinRAMTonnage = h1ProportionateRAMTonnage(publicBinRAMTonnage, publicBinTonnageWithoutRAM, h2ProjectedTonnage.TotalTonnage),
                ProjectedHouseholdDrinksContainerRAMTonnage = hdcRAMTonnage != null ? h1ProportionateRAMTonnage(hdcRAMTonnage, householdDrinksContainerTonnageWithoutRAM!.Value, h2ProjectedTonnage.TotalTonnage) : null
            };
        }

        private static decimal GetH2RAMProportion(CalcResultH2ProjectedProducerMaterialTonnage h2ProjectedTonnage, Func<CalcResultH2ProjectedProducerMaterialTonnage, decimal> getTotalTonnage)
        {
            if(h2ProjectedTonnage.TotalTonnage <= 0) return 0;
            
            return Math.Round(getTotalTonnage(h2ProjectedTonnage) / h2ProjectedTonnage.TotalTonnage, 6);
        }

        private static RAMTonnage GetProjectedTonnage(RAMTonnage h1RAMTonnage, decimal tonnageWithoutRAM, RAMProportions h2RamProportions, decimal h2TotalTonnage)
        {
            if (h2TotalTonnage > 0) {
                return new RAMTonnage
                {
                    Tonnage = h1RAMTonnage.Tonnage,
                    RedTonnage = h1RAMTonnage.RedTonnage + (tonnageWithoutRAM * h2RamProportions.Red),
                    AmberTonnage = h1RAMTonnage.AmberTonnage + (tonnageWithoutRAM * h2RamProportions.Amber),
                    GreenTonnage = h1RAMTonnage.GreenTonnage + (tonnageWithoutRAM * h2RamProportions.Green),
                    RedMedicalTonnage = h1RAMTonnage.RedMedicalTonnage + (tonnageWithoutRAM * h2RamProportions.RedMedical),
                    AmberMedicalTonnage = h1RAMTonnage.AmberMedicalTonnage + (tonnageWithoutRAM * h2RamProportions.AmberMedical),
                    GreenMedicalTonnage = h1RAMTonnage.GreenMedicalTonnage + (tonnageWithoutRAM * h2RamProportions.GreenMedical)
                };
            } else
            {
                return new RAMTonnage
                {
                    Tonnage = h1RAMTonnage.Tonnage,
                    RedTonnage = h1RAMTonnage.RedTonnage + tonnageWithoutRAM,
                    AmberTonnage = h1RAMTonnage.AmberTonnage,
                    GreenTonnage = h1RAMTonnage.GreenTonnage,
                    RedMedicalTonnage = h1RAMTonnage.RedMedicalTonnage,
                    AmberMedicalTonnage = h1RAMTonnage.AmberMedicalTonnage,
                    GreenMedicalTonnage = h1RAMTonnage.GreenMedicalTonnage
                };
            }
        }

        public static CalcResultH1ProjectedProducer CreateParentProducer(CalcResultH1ProjectedProducer p)
        {
            return new CalcResultH1ProjectedProducer
                {
                    ProducerId = p.ProducerId,
                    SubsidiaryId = null,
                    Level = CommonConstants.LevelOne.ToString(),
                    SubmissionPeriodCode = p.SubmissionPeriodCode,
                    IsSubtotal = true,
                    ProjectedTonnageByMaterial = p.ProjectedTonnageByMaterial
                };
        }

        public static CalcResultH1ProjectedProducer SumProducerGroupTonnages(IEnumerable<CalcResultH1ProjectedProducer> prodGroup)
        {
            decimal GetSummedH2Proportion(string matKey, Func<CalcResultH1ProjectedProducerMaterialTonnage, decimal> proportionFunc) {
               var totalH2Tonnage = prodGroup.Sum(p => p.ProjectedTonnageByMaterial[matKey].H2TotalTonnage); 
               return totalH2Tonnage > 0 ? prodGroup.Sum(p => proportionFunc(p.ProjectedTonnageByMaterial[matKey]) * p.ProjectedTonnageByMaterial[matKey].H2TotalTonnage) / totalH2Tonnage : 0;
            }

            var producer = prodGroup.First();
            var sumRam = (string matKey, Func<CalcResultH1ProjectedProducerMaterialTonnage, RAMTonnage?> tonnageFunc) => 
                CalcResultProjectedProducersBuilder.SumRAMTonnages<CalcResultH1ProjectedProducer, CalcResultH1ProjectedProducerMaterialTonnage>(prodGroup.ToList(), matKey, tonnageFunc);
            
            return new CalcResultH1ProjectedProducer
            {
                ProducerId = producer.ProducerId,
                SubsidiaryId = null,
                Level = CommonConstants.LevelOne.ToString(),
                SubmissionPeriodCode = producer.SubmissionPeriodCode,
                IsSubtotal = true,
                ProjectedTonnageByMaterial = producer.ProjectedTonnageByMaterial.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => new CalcResultH1ProjectedProducerMaterialTonnage {
                        HouseholdRAMTonnage = sumRam(kvp.Key, p => p.HouseholdRAMTonnage),
                        PublicBinRAMTonnage = sumRam(kvp.Key, p => p.PublicBinRAMTonnage),
                        HouseholdDrinksContainerRAMTonnage = kvp.Key == MaterialCodes.Glass ? sumRam(kvp.Key, p => p.HouseholdDrinksContainerRAMTonnage) : null,
                        HouseholdTonnageWithoutRAM = prodGroup.Sum(p => p.ProjectedTonnageByMaterial[kvp.Key].HouseholdTonnageWithoutRAM),
                        PublicBinTonnageWithoutRAM = prodGroup.Sum(p => p.ProjectedTonnageByMaterial[kvp.Key].PublicBinTonnageWithoutRAM),
                        HouseholdDrinksContainerTonnageWithoutRAM = kvp.Key == MaterialCodes.Glass ? prodGroup.Sum(p => p.ProjectedTonnageByMaterial[kvp.Key].HouseholdDrinksContainerTonnageWithoutRAM ?? 0) : null,
                        H2RamProportions = new RAMProportions {
                            Red = GetSummedH2Proportion(kvp.Key, t => t.H2RamProportions.Red),
                            Amber = GetSummedH2Proportion(kvp.Key, t => t.H2RamProportions.Amber),
                            Green = GetSummedH2Proportion(kvp.Key, t => t.H2RamProportions.Green),
                            RedMedical = GetSummedH2Proportion(kvp.Key, t => t.H2RamProportions.RedMedical),
                            AmberMedical = GetSummedH2Proportion(kvp.Key, t => t.H2RamProportions.AmberMedical),
                            GreenMedical = GetSummedH2Proportion(kvp.Key, t => t.H2RamProportions.GreenMedical),
                        },
                        TotalTonnage = prodGroup.Sum(p => p.ProjectedTonnageByMaterial[kvp.Key].TotalTonnage),
                        H2TotalTonnage = prodGroup.Sum(p => p.ProjectedTonnageByMaterial[kvp.Key].H2TotalTonnage),
                        ProjectedHouseholdRAMTonnage = sumRam(kvp.Key, p => p.ProjectedHouseholdRAMTonnage),
                        ProjectedPublicBinRAMTonnage = sumRam(kvp.Key, p => p.ProjectedPublicBinRAMTonnage),
                        ProjectedHouseholdDrinksContainerRAMTonnage = kvp.Key == MaterialCodes.Glass ? sumRam(kvp.Key, p => p.ProjectedHouseholdDrinksContainerRAMTonnage) : null
                    })
            };
        }

        private static CalcResultH1ProjectedProducerMaterialTonnage GetEmptyH1MaterialTonnage(string materialCode) {
            return new CalcResultH1ProjectedProducerMaterialTonnage
            {
                HouseholdRAMTonnage = new RAMTonnage { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                PublicBinRAMTonnage = new RAMTonnage { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                HouseholdDrinksContainerRAMTonnage = materialCode == MaterialCodes.Glass ? 
                    new RAMTonnage { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 } : null,
                HouseholdTonnageWithoutRAM = 0,
                PublicBinTonnageWithoutRAM = 0,
                HouseholdDrinksContainerTonnageWithoutRAM = materialCode == MaterialCodes.Glass ? 0 : null,
                H2RamProportions = new RAMProportions {
                    Red = 0, Amber = 0, Green = 0,
                    RedMedical = 0, AmberMedical = 0, GreenMedical = 0
                },   
                TotalTonnage = 0,
                H2TotalTonnage = 0,
                ProjectedHouseholdRAMTonnage = new RAMTonnage { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                ProjectedPublicBinRAMTonnage = new RAMTonnage { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                ProjectedHouseholdDrinksContainerRAMTonnage = materialCode == MaterialCodes.Glass ? 
                    new RAMTonnage { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 } : null
            };
        }

        private static CalcResultH2ProjectedProducerMaterialTonnage GetEmptyH2MaterialTonnage(string materialCode) {
            return new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdRAMTonnage = new RAMTonnage { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                PublicBinRAMTonnage = new RAMTonnage { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                HouseholdDrinksContainerRAMTonnage = materialCode == MaterialCodes.Glass ? 
                    new RAMTonnage { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 } : null,
                HouseholdTonnageDefaultedRed = 0,
                PublicBinTonnageDefaultedRed = 0,
                HouseholdDrinksContainerDefaultedRed = materialCode == MaterialCodes.Glass ? 0 : null,
                TotalTonnage = 0
            };
        }

        public static ProjectedProducersHeaders GetProjectedProducerHeaders(IEnumerable<MaterialDetail> materials)
        {
            return new ProjectedProducersHeaders {
                TitleHeader = new ProjectedProducersHeader
                {
                    Name = CalcResultProjectedProducersHeaders.H1ProjectedProducers,
                    ColumnIndex = 1,
                },
                MaterialBreakdownHeaders = GetMaterialsBreakdownHeader(materials),
                ColumnHeaders = GetColumnHeaders(materials)
            };
        }

        private static List<ProjectedProducersHeader> GetMaterialsBreakdownHeader(IEnumerable<MaterialDetail> materials)
        {
            var materialsBreakdownHeaders = new List<ProjectedProducersHeader>();
            var breakdownColumnIndex = GetInitialColumnHeaders().Count + 1;
            var materialHeaderCount = GetMaterialColumnHeaders().Count + GetH2ProportionHeaders().Count;
            var glassHeaderCount = GetGlassColumnHeaders().Count;
            var projectedMaterialHeaderCount = GetProjectedMaterialColumnHeaders().Count;
            var projectedGlassHeaderCount = GetProjectedGlassColumnHeaders().Count;

            foreach (var material in materials)
            { 
                var header = $"{material.Name} Breakdown";
                materialsBreakdownHeaders.Add(new ProjectedProducersHeader
                {
                    Name = header,
                    ColumnIndex = breakdownColumnIndex
                });

                var projectedColumnIndex = 0;
                
                if(material.Code == MaterialCodes.Glass)
                {
                    var breakdownHeaderCount = materialHeaderCount + glassHeaderCount; 
                    projectedColumnIndex = breakdownColumnIndex + breakdownHeaderCount;
                    breakdownColumnIndex += breakdownHeaderCount + projectedMaterialHeaderCount + projectedGlassHeaderCount;
                } else {
                    var breakdownHeaderCount = materialHeaderCount;
                    projectedColumnIndex = breakdownColumnIndex + breakdownHeaderCount;
                    breakdownColumnIndex += breakdownHeaderCount + projectedMaterialHeaderCount;
                }

                materialsBreakdownHeaders.Add(new ProjectedProducersHeader
                {
                    Name = $"Projected {header}",
                    ColumnIndex = projectedColumnIndex
                });
            }

            return materialsBreakdownHeaders;
        }

        private static List<ProjectedProducersHeader> GetColumnHeaders(IEnumerable<MaterialDetail> materials)
        {
            var columnHeaders = new List<ProjectedProducersHeader>();

            columnHeaders.AddRange(GetInitialColumnHeaders());

            foreach (var material in materials.Select(m => m.Code))
            {
                columnHeaders.AddRange(GetMaterialColumnHeaders());

                if (material == MaterialCodes.Glass)
                {
                    columnHeaders.AddRange(GetGlassColumnHeaders());
                }

                columnHeaders.AddRange(GetH2ProportionHeaders().Concat(GetProjectedMaterialColumnHeaders()));

                if (material == MaterialCodes.Glass)
                {
                    columnHeaders.AddRange(GetProjectedGlassColumnHeaders());
                }
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
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdTonnageWithoutRAM },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinPackagingTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinRedTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinAmberTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinGreenTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinRedMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinAmberMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinGreenMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinTonnageWithoutRAM },
                
            };
        }

        private static List<ProjectedProducersHeader> GetH2ProportionHeaders()
        {
            return new List<ProjectedProducersHeader>
            {
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.RedH2MaterialTonnageProportion },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.AmberH2MaterialTonnageProportion },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.GreenH2MaterialTonnageProportion },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.RedMedicalH2MaterialTonnageProportion },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.AmberMedicalH2MaterialTonnageProportion },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.GreenMedicalH2MaterialTonnageProportion }
            };
        }

        private static List<ProjectedProducersHeader> GetProjectedMaterialColumnHeaders()
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
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinPackagingTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinRedTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinAmberTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinGreenTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinRedMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinAmberMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinGreenMedicalTonnage },
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
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersTonnageWithoutRAM }
            };
        }

        private static List<ProjectedProducersHeader> GetProjectedGlassColumnHeaders()
        {
            return new List<ProjectedProducersHeader>
            {
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersPackagingTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberMedicalTonnage },
                new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenMedicalTonnage }
            };
        }
    }
}