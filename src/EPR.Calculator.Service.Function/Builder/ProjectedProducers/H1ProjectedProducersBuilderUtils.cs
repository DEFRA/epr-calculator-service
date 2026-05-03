using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.ProjectedProducers
{
    public static class H1ProjectedProducersBuilderUtils
    {
        public static List<CalcResultH1ProjectedProducer> GetProjectedProducers(List<ProducerDetail> producerDetails, List<CalcResultH2ProjectedProducer> h2ProjectedProducers, List<MaterialDetail> materials, string submissionPeriod)
        {
            CalcResultH2ProjectedProducer? GetH2Producer(ProducerDetail rm)
            {
                var prodGroup = h2ProjectedProducers.Where(p => p.ProducerId == rm.ProducerId);
                var maybeSubtotal = prodGroup.Where(p => p.IsSubtotal);
                return maybeSubtotal.Any() ? maybeSubtotal.First() : prodGroup.FirstOrDefault(p => p.SubsidiaryId == rm.SubsidiaryId);
            }

            return producerDetails.Select(pd => new CalcResultH1ProjectedProducer
            {
                ProducerId = pd.ProducerId,
                SubsidiaryId = pd.SubsidiaryId,
                Level = string.Empty, // Level will be set later when subtotals are added
                SubmissionPeriodCode = submissionPeriod,
                H1ProjectedTonnageByMaterial = GetProjectedTonnages(
                    materials,
                    pd.ProducerReportedMaterials.Where(rm => rm.SubmissionPeriod == submissionPeriod).ToList(),
                    GetH2Producer(pd)
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

            var h2ProjectedTonnage = h2ProjectedProducer != null ? h2ProjectedProducer.H2ProjectedTonnageByMaterial[material.Code] : GetEmptyH2MaterialTonnage(material.Code);
            var h2RamProportions = new RAMProportions{
                Red = GetH2RAMProportion(h2ProjectedTonnage.GetTotalProjectedRedTonnage(), h2ProjectedTonnage.TotalTonnage),
                Amber = GetH2RAMProportion(h2ProjectedTonnage.GetTotalProjectedAmberTonnage(), h2ProjectedTonnage.TotalTonnage),
                Green = GetH2RAMProportion(h2ProjectedTonnage.GetTotalProjectedGreenTonnage(), h2ProjectedTonnage.TotalTonnage),
                RedMedical = GetH2RAMProportion(h2ProjectedTonnage.GetTotalProjectedRedMedicalTonnage(), h2ProjectedTonnage.TotalTonnage),
                AmberMedical = GetH2RAMProportion(h2ProjectedTonnage.GetTotalProjectedAmberMedicalTonnage(), h2ProjectedTonnage.TotalTonnage),
                GreenMedical = GetH2RAMProportion(h2ProjectedTonnage.GetTotalProjectedGreenMedicalTonnage(), h2ProjectedTonnage.TotalTonnage)
            };

            var h1ProportionateRAMTonnage = (RAMTonnage ramTonnage, decimal tonnageWithoutRAM, decimal h2TotalTonnage)
                => GetProjectedTonnage(ramTonnage, tonnageWithoutRAM, h2RamProportions, h2TotalTonnage);

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
                ProjectedHouseholdRAMTonnage = h1ProportionateRAMTonnage(householdRAMTonnage, householdTonnageWithoutRAM, h2ProjectedTonnage.TotalTonnage),
                ProjectedPublicBinRAMTonnage = h1ProportionateRAMTonnage(publicBinRAMTonnage, publicBinTonnageWithoutRAM, h2ProjectedTonnage.TotalTonnage),
                ProjectedHouseholdDrinksContainerRAMTonnage = hdcRAMTonnage != null ? h1ProportionateRAMTonnage(hdcRAMTonnage, householdDrinksContainerTonnageWithoutRAM!.Value, h2ProjectedTonnage.TotalTonnage) : null
            };
        }

        private static decimal GetH2RAMProportion(decimal totalH2MatTonnage, decimal totalH2Tonnage)
        {
            if(totalH2Tonnage <= 0) return 0;

            return Math.Round(totalH2MatTonnage / totalH2Tonnage, 6);
        }

        public static RAMTonnage GetProportionateRam(RAMTonnage h1RAMTonnage, decimal tonnageWithoutRAM, RAMProportions h2RamProportions)
        {
            return h1RAMTonnage with {
                RedTonnage = Math.Round(h1RAMTonnage.RedTonnage + (tonnageWithoutRAM * h2RamProportions.Red), 3),
                AmberTonnage = Math.Round(h1RAMTonnage.AmberTonnage + (tonnageWithoutRAM * h2RamProportions.Amber), 3),
                GreenTonnage = Math.Round(h1RAMTonnage.GreenTonnage + (tonnageWithoutRAM * h2RamProportions.Green), 3),
                RedMedicalTonnage = Math.Round(h1RAMTonnage.RedMedicalTonnage + (tonnageWithoutRAM * h2RamProportions.RedMedical), 3),
                AmberMedicalTonnage = Math.Round(h1RAMTonnage.AmberMedicalTonnage + (tonnageWithoutRAM * h2RamProportions.AmberMedical), 3),
                GreenMedicalTonnage = Math.Round(h1RAMTonnage.GreenMedicalTonnage + (tonnageWithoutRAM * h2RamProportions.GreenMedical), 3)
            };
        }

        private static RAMTonnage GetProjectedTonnage(RAMTonnage h1RAMTonnage, decimal tonnageWithoutRAM, RAMProportions h2RamProportions, decimal h2TotalTonnage)
        {
            if (h2TotalTonnage > 0) {
                var projectedTonnage = GetProportionateRam(h1RAMTonnage, tonnageWithoutRAM, h2RamProportions);
                return ReconcileRoundingDifference(projectedTonnage);
            }
            else
            {
                return h1RAMTonnage with { RedTonnage = h1RAMTonnage.RedTonnage + tonnageWithoutRAM };
            }
        }

        public static RAMTonnage ReconcileRoundingDifference(RAMTonnage projectedTonnage)
        {
            var diffTonnage = projectedTonnage.Tonnage - projectedTonnage.GetTotalRamTonnage();
            if (diffTonnage != 0)
            {
                var dominantRamTonnage = new[]
                {
                    (Priority: 1, Tonnage: projectedTonnage.AmberTonnage, Apply: (Func<RAMTonnage, RAMTonnage>)(t => t with { AmberTonnage = t.AmberTonnage + diffTonnage })),
                    (Priority: 2, Tonnage: projectedTonnage.RedTonnage, Apply: t => t with { RedTonnage = t.RedTonnage + diffTonnage }),
                    (Priority: 3, Tonnage: projectedTonnage.GreenTonnage, Apply: t => t with { GreenTonnage = t.GreenTonnage + diffTonnage }),
                    (Priority: 4, Tonnage: projectedTonnage.AmberMedicalTonnage, Apply: t => t with { AmberMedicalTonnage = t.AmberMedicalTonnage + diffTonnage }),
                    (Priority: 5, Tonnage: projectedTonnage.RedMedicalTonnage, Apply: t => t with { RedMedicalTonnage = t.RedMedicalTonnage + diffTonnage }),
                    (Priority: 6, Tonnage: projectedTonnage.GreenMedicalTonnage, Apply: t => t with { GreenMedicalTonnage = t.GreenMedicalTonnage + diffTonnage }),
                }
                .OrderByDescending(kv => kv.Tonnage)
                .ThenBy(kv => kv.Priority)
                .First();

                return dominantRamTonnage.Apply(projectedTonnage);
            }

            return projectedTonnage;
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
                    H1ProjectedTonnageByMaterial = p.H1ProjectedTonnageByMaterial
                };
        }

        public static CalcResultH1ProjectedProducer SumProducerGroupTonnages(List<CalcResultH1ProjectedProducer> prodGroup)
        {
            var producer = prodGroup.First();
            var sumRam = (string matKey, Func<CalcResultProjectedProducerMaterialTonnage, RAMTonnage?> tonnageFunc) =>
                CalcResultProjectedProducersBuilder.SumRAMTonnages(prodGroup.Cast<ICalcResultProjectedProducer>().ToList(), matKey, tonnageFunc);

            return new CalcResultH1ProjectedProducer
            {
                ProducerId = producer.ProducerId,
                SubsidiaryId = null,
                Level = CommonConstants.LevelOne.ToString(),
                SubmissionPeriodCode = producer.SubmissionPeriodCode,
                IsSubtotal = true,
                H1ProjectedTonnageByMaterial = producer.H1ProjectedTonnageByMaterial.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new CalcResultH1ProjectedProducerMaterialTonnage {
                        HouseholdRAMTonnage = sumRam(kvp.Key, p => p.HouseholdRAMTonnage),
                        PublicBinRAMTonnage = sumRam(kvp.Key, p => p.PublicBinRAMTonnage),
                        HouseholdDrinksContainerRAMTonnage = kvp.Key == MaterialCodes.Glass ? sumRam(kvp.Key, p => p.HouseholdDrinksContainerRAMTonnage) : null,
                        HouseholdTonnageWithoutRAM = prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].HouseholdTonnageWithoutRAM),
                        PublicBinTonnageWithoutRAM = prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].PublicBinTonnageWithoutRAM),
                        HouseholdDrinksContainerTonnageWithoutRAM = kvp.Key == MaterialCodes.Glass ? prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].HouseholdDrinksContainerTonnageWithoutRAM ?? 0) : null,
                        H2RamProportions = producer.H1ProjectedTonnageByMaterial[kvp.Key].H2RamProportions ?? new RAMProportions(),
                        TotalTonnage = prodGroup.Sum(p => p.H1ProjectedTonnageByMaterial[kvp.Key].TotalTonnage),
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
                HouseholdTonnageWithoutRAM = 0,
                PublicBinTonnageWithoutRAM = 0,
                HouseholdDrinksContainerTonnageWithoutRAM = materialCode == MaterialCodes.Glass ? 0 : null,
                ProjectedHouseholdRAMTonnage = new RAMTonnage { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                ProjectedPublicBinRAMTonnage = new RAMTonnage { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 },
                ProjectedHouseholdDrinksContainerRAMTonnage = materialCode == MaterialCodes.Glass ?
                    new RAMTonnage { Tonnage = 0, RedTonnage = 0, AmberTonnage = 0, GreenTonnage = 0, RedMedicalTonnage = 0, AmberMedicalTonnage = 0, GreenMedicalTonnage = 0 } : null,
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
