using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using static EPR.Calculator.Service.Function.Builder.ProjectedProducers.CalcResultProjectedProducersBuilder;

namespace EPR.Calculator.Service.Function.Builder.ProjectedProducers
{
    public static class H2ProjectedProducersBuilderUtils
    {
        public static List<CalcResultH2ProjectedProducer> GetProjectedProducers(List<ProducerDetail> producerDetails, List<MaterialDetail> materials, string submissionPeriod)
        {
            return producerDetails.Select(pd => new CalcResultH2ProjectedProducer
            {
                ProducerId = pd.ProducerId,
                SubsidiaryId = pd.SubsidiaryId,
                Level = string.Empty,  // Level will be set later when subtotals are added
                SubmissionPeriodCode = submissionPeriod,
                H2ProjectedTonnageByMaterial = GetProjectedTonnages(
                    materials,
                    pd.ProducerReportedMaterials.Where(rm => rm.SubmissionPeriod == submissionPeriod).ToList()
                )
            }).ToList();
        }

        private static Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage> GetProjectedTonnages(List<MaterialDetail> materials, List<ProducerReportedMaterial> reportedMaterials)
        {
            return materials.ToDictionary(m => m.Code, m => GetProjectedTonnage(m, reportedMaterials.Where(rm => rm.MaterialId == m.Id).ToList()));
        }

        private static CalcResultH2ProjectedProducerMaterialTonnage GetProjectedTonnage(MaterialDetail material, List<ProducerReportedMaterial> reportedMaterials)
        {
            RAMTonnage GetProjectedRam(RAMTonnage tonnage, decimal tonnageWithoutRam)
            {
                return new RAMTonnage
                {
                    RedTonnage = tonnage.RedTonnage + tonnageWithoutRam,
                    AmberTonnage = tonnage.AmberTonnage,
                    GreenTonnage = tonnage.GreenTonnage,
                    RedMedicalTonnage = tonnage.RedMedicalTonnage,
                    AmberMedicalTonnage = tonnage.AmberMedicalTonnage,
                    GreenMedicalTonnage = tonnage.GreenMedicalTonnage,
                };
            }
            
            var hhTonnage = RAMTonnage.GetReportedTonnage(reportedMaterials, PackagingTypes.Household, rm => rm.PackagingTonnage);
            var householdRAMTonnage = RAMTonnage.ToRAMTonnage(PackagingTypes.Household, reportedMaterials);
            var pbTonnage = RAMTonnage.GetReportedTonnage(reportedMaterials, PackagingTypes.PublicBin, rm => rm.PackagingTonnage);
            var publicBinRAMTonnage = RAMTonnage.ToRAMTonnage(PackagingTypes.PublicBin, reportedMaterials);
            decimal? hdcTonnage = (material.Code == MaterialCodes.Glass) ? RAMTonnage.GetReportedTonnage(reportedMaterials, PackagingTypes.HouseholdDrinksContainers, rm => rm.PackagingTonnage) : null;
            var hdcRAMTonnage = (material.Code == MaterialCodes.Glass) ? RAMTonnage.ToRAMTonnage(PackagingTypes.HouseholdDrinksContainers, reportedMaterials) : null;
            var hhWithoutRam = CalcResultProjectedProducersBuilder.TonnageWithoutRAM(hhTonnage, householdRAMTonnage);
            var pbWithoutRam = CalcResultProjectedProducersBuilder.TonnageWithoutRAM(pbTonnage, publicBinRAMTonnage);
            decimal? hdcWithoutRam = (hdcTonnage != null && hdcRAMTonnage != null) ? CalcResultProjectedProducersBuilder.TonnageWithoutRAM(hdcTonnage.Value, hdcRAMTonnage) : null;

            return new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage = hhTonnage,
                HouseholdRAMTonnage = householdRAMTonnage,
                PublicBinTonnage = pbTonnage,
                PublicBinRAMTonnage = publicBinRAMTonnage,
                HouseholdDrinksContainerTonnage = hdcTonnage,
                HouseholdDrinksContainerRAMTonnage = hdcRAMTonnage,
                HouseholdTonnageWithoutRAM = hhWithoutRam,
                PublicBinTonnageWithoutRAM = pbWithoutRam,
                HouseholdDrinksContainerTonnageWithoutRAM = hdcWithoutRam,
                ProjectedHouseholdTonnage = hhTonnage,
                ProjectedHouseholdRAMTonnage = GetProjectedRam(householdRAMTonnage, hhWithoutRam),
                ProjectedPublicBinTonnage = pbTonnage,
                ProjectedPublicBinRAMTonnage = GetProjectedRam(publicBinRAMTonnage, pbWithoutRam),
                ProjectedHouseholdDrinksContainerTonnage = hdcTonnage,
                ProjectedHouseholdDrinksContainerRAMTonnage = hdcRAMTonnage != null ? GetProjectedRam(hdcRAMTonnage, hdcWithoutRam!.Value) : null,
                TotalTonnage = hhTonnage + pbTonnage + (hdcTonnage ?? 0)
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
                H2ProjectedTonnageByMaterial = p.H2ProjectedTonnageByMaterial
            };
        }

        public static CalcResultH2ProjectedProducer SumProducerGroupTonnages(List<CalcResultH2ProjectedProducer> prodGroup)
        {
            var producer = prodGroup.First();
            var sumRam = (string matKey, Func<CalcResultProjectedProducerMaterialTonnage, RAMTonnage?> tonnageFunc) =>
                CalcResultProjectedProducersBuilder.SumRAMTonnages(prodGroup.Cast<ICalcResultProjectedProducer>().ToList(), matKey, tonnageFunc);

            return new CalcResultH2ProjectedProducer
            {
                ProducerId = producer.ProducerId,
                SubsidiaryId = null,
                Level = CommonConstants.LevelOne.ToString(),
                SubmissionPeriodCode = producer.SubmissionPeriodCode,
                IsSubtotal = true,
                H2ProjectedTonnageByMaterial = producer.H2ProjectedTonnageByMaterial.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new CalcResultH2ProjectedProducerMaterialTonnage {
                        HouseholdTonnage = prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].HouseholdTonnage),
                        HouseholdRAMTonnage = sumRam(kvp.Key, p => p.HouseholdRAMTonnage),
                        PublicBinTonnage = prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].PublicBinTonnage),
                        PublicBinRAMTonnage = sumRam(kvp.Key, p => p.PublicBinRAMTonnage),
                        HouseholdDrinksContainerTonnage = kvp.Key == MaterialCodes.Glass ? prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].HouseholdDrinksContainerTonnage ?? 0) : null,
                        HouseholdDrinksContainerRAMTonnage = kvp.Key == MaterialCodes.Glass ? sumRam(kvp.Key, p => p.HouseholdDrinksContainerRAMTonnage) : null,
                        HouseholdTonnageWithoutRAM = prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].HouseholdTonnageWithoutRAM),
                        PublicBinTonnageWithoutRAM = prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].PublicBinTonnageWithoutRAM),
                        HouseholdDrinksContainerTonnageWithoutRAM = kvp.Key == MaterialCodes.Glass ? prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].HouseholdDrinksContainerTonnageWithoutRAM ?? 0) : null,
                        ProjectedHouseholdTonnage = prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].ProjectedHouseholdTonnage),
                        ProjectedHouseholdRAMTonnage = sumRam(kvp.Key, p => p.ProjectedHouseholdRAMTonnage),
                        ProjectedPublicBinTonnage = prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].ProjectedPublicBinTonnage),
                        ProjectedPublicBinRAMTonnage = sumRam(kvp.Key, p => p.ProjectedPublicBinRAMTonnage),
                        ProjectedHouseholdDrinksContainerTonnage = kvp.Key == MaterialCodes.Glass ? prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].ProjectedHouseholdDrinksContainerTonnage ?? 0) : null,
                        ProjectedHouseholdDrinksContainerRAMTonnage = kvp.Key == MaterialCodes.Glass ? sumRam(kvp.Key, p => p.ProjectedHouseholdDrinksContainerRAMTonnage) : null,
                        TotalTonnage = prodGroup.Sum(p => p.H2ProjectedTonnageByMaterial[kvp.Key].TotalTonnage)
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

            foreach (var material in materials.Select(m => m.Code))
            {
                columnHeaders.AddRange(GetMaterialColumnHeaders());

                if (material == MaterialCodes.Glass)
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
