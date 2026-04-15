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
        public static async Task<List<CalcResultH1ProjectedProducer>> GetProjectedProducers(List<ProducerReportedMaterialsForSubmissionPeriod> reportedMaterials, List<CalcResultH2ProjectedProducer> h2ProjectedProducers, List<MaterialDetail> materials)
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
                    h2ProjectedProducers.First(p => p.ProducerId == rm.ProducerId && p.SubsidiaryId == rm.SubsidiaryId) //TODO: safe to assume?
                )
            }).ToList();
        }

        private static Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage> GetProjectedTonnages(List<MaterialDetail> materials, List<ProducerReportedMaterial> reportedMaterials, CalcResultH2ProjectedProducer h2ProjectedProducer)
        {
            return materials.ToDictionary(m => m.Code, m => GetProjectedTonnage(m, reportedMaterials.Where(rm => rm.MaterialId == m.Id).ToList(), h2ProjectedProducer));
        }

        private static CalcResultH1ProjectedProducerMaterialTonnage GetProjectedTonnage(MaterialDetail material, List<ProducerReportedMaterial> reportedMaterials, CalcResultH2ProjectedProducer h2ProjectedProducer)
        {
            if(!reportedMaterials.Any())
            {
                var emptyRam = new RAMTonnage { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 };
                return new CalcResultH1ProjectedProducerMaterialTonnage
                {
                    HouseholdRAMTonnage = emptyRam,
                    PublicBinRAMTonnage = emptyRam,
                    HouseholdDrinksContainerRAMTonnage = material.Code == MaterialCodes.Glass ? emptyRam : null,
                    HouseholdTonnageWithoutRAM = 0,
                    PublicBinTonnageWithoutRAM = 0,
                    HouseholdDrinksContainerTonnageWithoutRAM = 0,
                    RedH2Proportion = 0,
                    AmberH2Proportion = 0,
                    GreenH2Proportion = 0,
                    RedMedicalH2Proportion = 0,
                    AmberMedicalH2Proportion = 0,
                    GreenMedicalH2Proportion = 0,
                    TotalTonnage = 0,
                    H2TotalTonnage = 0,
                    ProjectedHouseholdRAMTonnage = emptyRam,
                    ProjectedPublicBinRAMTonnage = emptyRam,
                    ProjectedHouseholdDrinksContainerRAMTonnage = material.Code == MaterialCodes.Glass ? emptyRam : null
                };
            }

            var householdRAMTonnage = CalcResultProjectedProducersBuilder.GetRAMTonnage(PackagingTypes.Household, reportedMaterials);
            var publicBinRAMTonnage = CalcResultProjectedProducersBuilder.GetRAMTonnage(PackagingTypes.PublicBin, reportedMaterials);
            var hdcRAMTonnage = (material.Code == MaterialCodes.Glass) ? CalcResultProjectedProducersBuilder.GetRAMTonnage(PackagingTypes.HouseholdDrinksContainers, reportedMaterials) : null;

            var householdTonnageWithoutRAM = CalcResultProjectedProducersBuilder.TonnageWithoutRAM(householdRAMTonnage);
            var publicBinTonnageWithoutRAM = CalcResultProjectedProducersBuilder.TonnageWithoutRAM(publicBinRAMTonnage);
            decimal? householdDrinksContainerTonnageWithoutRAM = (hdcRAMTonnage != null) ? CalcResultProjectedProducersBuilder.TonnageWithoutRAM(hdcRAMTonnage) : null;
            
            var h2ProjectedTonnage = h2ProjectedProducer.ProjectedTonnageByMaterial[material.Code];
            var redH2Proportion = GetH2RedRAMProportion(h2ProjectedTonnage);
            var amberH2Proportion = GetH2RAMProportion(h2ProjectedTonnage, t => t.AmberTonnage);
            var greenH2Proportion = GetH2RAMProportion(h2ProjectedTonnage, t => t.GreenTonnage);
            var redMedicalH2Proportion = GetH2RAMProportion(h2ProjectedTonnage, t => t.RedMedicalTonnage);
            var amberMedicalH2Proportion = GetH2RAMProportion(h2ProjectedTonnage, t => t.AmberMedicalTonnage);
            var greenMedicalH2Proportion = GetH2RAMProportion(h2ProjectedTonnage, t => t.GreenMedicalTonnage);

            var h1ProportionateRAMTonnage = (RAMTonnage ramTonnage, decimal tonnageWithoutRAM) => GetH1RAMTonnageProportionateToH2(ramTonnage, tonnageWithoutRAM, redH2Proportion, amberH2Proportion, greenH2Proportion, redMedicalH2Proportion, amberMedicalH2Proportion, greenMedicalH2Proportion);

            return new CalcResultH1ProjectedProducerMaterialTonnage
            {
                HouseholdRAMTonnage = householdRAMTonnage,
                PublicBinRAMTonnage = publicBinRAMTonnage,
                HouseholdDrinksContainerRAMTonnage = hdcRAMTonnage,
                HouseholdTonnageWithoutRAM = householdTonnageWithoutRAM,
                PublicBinTonnageWithoutRAM = publicBinTonnageWithoutRAM,
                HouseholdDrinksContainerTonnageWithoutRAM = householdDrinksContainerTonnageWithoutRAM,
                RedH2Proportion = redH2Proportion,
                AmberH2Proportion = amberH2Proportion,
                GreenH2Proportion = greenH2Proportion,
                RedMedicalH2Proportion = redMedicalH2Proportion,
                AmberMedicalH2Proportion = amberMedicalH2Proportion,
                GreenMedicalH2Proportion = greenMedicalH2Proportion,
                TotalTonnage = householdRAMTonnage.Tonnage + publicBinRAMTonnage.Tonnage + (hdcRAMTonnage?.Tonnage ?? 0),
                H2TotalTonnage = h2ProjectedTonnage.TotalTonnage,
                ProjectedHouseholdRAMTonnage = h1ProportionateRAMTonnage(householdRAMTonnage, householdTonnageWithoutRAM),
                ProjectedPublicBinRAMTonnage = h1ProportionateRAMTonnage(publicBinRAMTonnage, publicBinTonnageWithoutRAM),
                ProjectedHouseholdDrinksContainerRAMTonnage = hdcRAMTonnage != null ? h1ProportionateRAMTonnage(hdcRAMTonnage, householdDrinksContainerTonnageWithoutRAM ?? 0) : null
            };
        }

        private static decimal GetH2RedRAMProportion(CalcResultH2ProjectedProducerMaterialTonnage h2ProjectedTonnage)
        {
            var totalTonnage = h2ProjectedTonnage.TotalTonnage;
            var proportion = totalTonnage > 0 ? (h2ProjectedTonnage.HouseholdRAMTonnage.RedTonnage + h2ProjectedTonnage.HouseholdTonnageDefaultedRed + 
                h2ProjectedTonnage.PublicBinRAMTonnage.RedTonnage + h2ProjectedTonnage.PublicBinTonnageDefaultedRed + 
                (h2ProjectedTonnage.HouseholdDrinksContainerRAMTonnage?.RedTonnage ?? 0) +
                (h2ProjectedTonnage.HouseholdDrinksContainerDefaultedRed ?? 0)) 
                / totalTonnage : 0;
            
            return Math.Round(proportion, 6);
        }

        private static decimal GetH2RAMProportion(CalcResultH2ProjectedProducerMaterialTonnage h2ProjectedTonnage, Func<RAMTonnage, decimal> getTonnage)
        {
            var totalTonnage = (h2ProjectedTonnage.TotalTonnage);
            var proportion = totalTonnage > 0 ? (getTonnage(h2ProjectedTonnage.HouseholdRAMTonnage) + getTonnage(h2ProjectedTonnage.PublicBinRAMTonnage) + 
                (h2ProjectedTonnage.HouseholdDrinksContainerRAMTonnage != null ? getTonnage(h2ProjectedTonnage.HouseholdDrinksContainerRAMTonnage) : 0m)
            ) / totalTonnage : 0;

            return Math.Round(proportion, 6);
        }

        private static RAMTonnage GetH1RAMTonnageProportionateToH2(RAMTonnage h1RAMTonnage, decimal tonnageWithoutRAM, decimal redH2Proportion, decimal amberH2Proportion, decimal greenH2Proportion, decimal redMedicalH2Proportion, decimal amberMedicalH2Proportion, decimal greenMedicalH2Proportion)
        {
            return new RAMTonnage
            {
                Tonnage = h1RAMTonnage.Tonnage,
                RedTonnage = h1RAMTonnage.RedTonnage + (tonnageWithoutRAM * redH2Proportion),
                AmberTonnage = h1RAMTonnage.AmberTonnage + (tonnageWithoutRAM * amberH2Proportion),
                GreenTonnage = h1RAMTonnage.GreenTonnage + (tonnageWithoutRAM * greenH2Proportion),
                RedMedicalTonnage = h1RAMTonnage.RedMedicalTonnage + (tonnageWithoutRAM * redMedicalH2Proportion),
                AmberMedicalTonnage = h1RAMTonnage.AmberMedicalTonnage + (tonnageWithoutRAM * amberMedicalH2Proportion),
                GreenMedicalTonnage = h1RAMTonnage.GreenMedicalTonnage + (tonnageWithoutRAM * greenMedicalH2Proportion)
            };
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
                        RedH2Proportion = GetSummedH2Proportion(kvp.Key, t => t.RedH2Proportion),
                        AmberH2Proportion = GetSummedH2Proportion(kvp.Key, t => t.AmberH2Proportion),
                        GreenH2Proportion = GetSummedH2Proportion(kvp.Key, t => t.GreenH2Proportion),
                        RedMedicalH2Proportion = GetSummedH2Proportion(kvp.Key, t => t.RedMedicalH2Proportion),
                        AmberMedicalH2Proportion = GetSummedH2Proportion(kvp.Key, t => t.AmberMedicalH2Proportion),
                        GreenMedicalH2Proportion = GetSummedH2Proportion(kvp.Key, t => t.GreenMedicalH2Proportion),
                        TotalTonnage = prodGroup.Sum(p => p.ProjectedTonnageByMaterial[kvp.Key].TotalTonnage),
                        H2TotalTonnage = prodGroup.Sum(p => p.ProjectedTonnageByMaterial[kvp.Key].H2TotalTonnage),
                        ProjectedHouseholdRAMTonnage = sumRam(kvp.Key, p => p.ProjectedHouseholdRAMTonnage),
                        ProjectedPublicBinRAMTonnage = sumRam(kvp.Key, p => p.ProjectedPublicBinRAMTonnage),
                        ProjectedHouseholdDrinksContainerRAMTonnage = kvp.Key == MaterialCodes.Glass ? sumRam(kvp.Key, p => p.ProjectedHouseholdDrinksContainerRAMTonnage) : null
                    })
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

            foreach (var material in materials)
            {
                columnHeaders.AddRange(GetMaterialColumnHeaders());

                if (material.Code == MaterialCodes.Glass)
                {
                    columnHeaders.AddRange(GetGlassColumnHeaders());
                }

                columnHeaders.AddRange(GetH2ProportionHeaders().Concat(GetProjectedMaterialColumnHeaders()));

                if (material.Code == MaterialCodes.Glass)
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