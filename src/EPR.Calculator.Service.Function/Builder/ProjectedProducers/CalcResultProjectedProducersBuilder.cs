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

    public interface ICalcResultProjectedProducersBuilder
    {
        Task<CalcResultProjectedProducers> ConstructAsync(CalcResultsRequestDto resultsRequestDto);
    }

    public class CalcResultProjectedProducersBuilder : ICalcResultProjectedProducersBuilder
    {
        private readonly ApplicationDBContext context;

        public CalcResultProjectedProducersBuilder(ApplicationDBContext dbContext)
        {
            context = dbContext;
        }

        public async Task<CalcResultProjectedProducers> ConstructAsync(CalcResultsRequestDto resultsRequestDto)
        {
            var runId = resultsRequestDto.RunId;
            var materialsFromDb = await context.Material.ToListAsync();
            var materials = MaterialMapper.Map(materialsFromDb);

            var h2ProjectedProduers = await GetH2ProjectedProducers(runId, materials);

            var h2ProjectedProducersWithSubtotals = AddSubtotals(h2ProjectedProduers);

            return new CalcResultProjectedProducers
            {
                H2ProjectedProducersHeaders = new ProjectedProducersHeaders {
                    TitleHeader = new ProjectedProducersHeader
                    {
                        Name = CalcResultProjectedProducersHeaders.H2ProjectedProducers,
                        ColumnIndex = 1,
                    },
                    MaterialBreakdownHeaders = GetMaterialsBreakdownHeader(materials),
                    ColumnHeaders = GetColumnHeaders(materials)
                },
                H2ProjectedProducers = h2ProjectedProducersWithSubtotals
                                        .OrderBy(p => p.ProducerId)
                                        .ThenBy(p => p.Level)
                                        .ThenBy(p => p.SubsidiaryId)
                                        .ToList()
            };
        }

        public async Task<List<CalcResultH2ProjectedProducer>> GetH2ProjectedProducers(int runId, List<MaterialDetail> materials)
        {
            var result = await (from run in context.CalculatorRuns.AsNoTracking()
                    join pd in context.ProducerDetail.AsNoTracking() on run.Id equals pd.CalculatorRunId
                    join prm in context.ProducerReportedMaterial.AsNoTracking() on pd.Id equals prm.ProducerDetailId
                    let submissionPeriod = (run.RelativeYearValue - 1).ToString() + "-H2"
                    where pd.CalculatorRunId == runId && prm.SubmissionPeriod == submissionPeriod
                    group prm by new { pd.ProducerId, pd.SubsidiaryId, submissionPeriod = submissionPeriod } into prms 
                    select new CalcResultH2ProjectedProducer
                    {
                        ProducerId = prms.Key.ProducerId,
                        SubsidiaryId = prms.Key.SubsidiaryId,
                        Level = string.Empty, // Level will be set in AddSubtotals method
                        SubmissionPeriodCode = prms.Key.submissionPeriod,
                        ProjectedTonnageByMaterial = GetH2ProjectedTonnages(materials, prms.ToList())
                    }).ToListAsync();

            return result;
        }

        public static Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage> GetH2ProjectedTonnages(List<MaterialDetail> materials, List<ProducerReportedMaterial> reportedMaterials)
        {
            return materials.ToDictionary(m => m.Code, m => GetH2ProjectedTonnage(m, reportedMaterials.Where(rm => rm.MaterialId == m.Id).ToList()));
        }

        public static CalcResultH2ProjectedProducerMaterialTonnage GetH2ProjectedTonnage(MaterialDetail material, List<ProducerReportedMaterial> reportedMaterials)
        {
            decimal DefaultRAMToRed(RAMTonnage tonnage)
            {
                var ramTonnage = tonnage.RedTonnage + tonnage.RedMedicalTonnage + tonnage.AmberTonnage + tonnage.AmberMedicalTonnage + tonnage.GreenTonnage + tonnage.GreenMedicalTonnage;
                var diffTonnage = tonnage.Tonnage - ramTonnage;
                return diffTonnage > 0 ? diffTonnage : 0;
            }

            var householdRAMTonnage = GetRAMTonnage(PackagingTypes.Household, reportedMaterials);
            var publicBinRAMTonnage = GetRAMTonnage(PackagingTypes.PublicBin, reportedMaterials);
            var hdcRAMTonnage = (material.Code == MaterialCodes.Glass) ? GetRAMTonnage(PackagingTypes.HouseholdDrinksContainers, reportedMaterials) : null;

            return new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdRAMTonnage = householdRAMTonnage,
                PublicBinRAMTonnage = publicBinRAMTonnage,
                HouseholdDrinksContainerRAMTonnage = hdcRAMTonnage,
                HouseholdTonnageDefaultedRed = DefaultRAMToRed(householdRAMTonnage),
                PublicBinTonnageDefaultedRed = DefaultRAMToRed(publicBinRAMTonnage),
                HouseholdDrinksContainerDefaultedRed = (hdcRAMTonnage != null) ? DefaultRAMToRed(hdcRAMTonnage) : null,
                TotalTonnage = householdRAMTonnage.Tonnage + publicBinRAMTonnage.Tonnage + (hdcRAMTonnage?.Tonnage ?? 0)
            };
        }

        private static RAMTonnage GetRAMTonnage(string packagingType, List<ProducerReportedMaterial> reportedMaterials) {
            decimal GetReportedTonnage(string packagingType, Func<ProducerReportedMaterial, decimal?> tonnageFunc) {
                return reportedMaterials.Where(p => p.PackagingType == packagingType).Sum(t => tonnageFunc(t) ?? 0);
            }

            return new RAMTonnage {
                Tonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnage),
                RedTonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnageRed),
                RedMedicalTonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnageRedMedical),
                AmberTonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnageAmber),
                AmberMedicalTonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnageAmberMedical),
                GreenTonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnageGreen),
                GreenMedicalTonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnageGreenMedical),
            };
        }

        public static List<CalcResultH2ProjectedProducer> AddSubtotals(List<CalcResultH2ProjectedProducer> projectedProducers)
        {
            var producersWithSubtotals = new List<CalcResultH2ProjectedProducer>();
            var producerGroups = projectedProducers.GroupBy(p => p.ProducerId);

            foreach(var prodGroup in producerGroups)
            {
                if(prodGroup.Count() > 1)
                {
                    producersWithSubtotals.AddRange(prodGroup.Select(p => {
                        p.Level = CommonConstants.LevelTwo.ToString();
                        return p;
                    }));

                    producersWithSubtotals.Add(
                        SumProducerGroupTonnages(prodGroup, prodGroup.First())
                    );
                }
                else {
                    var producer = prodGroup.First();
                    if(producer.SubsidiaryId != null)
                    {
                        producer.Level = CommonConstants.LevelTwo.ToString();
                        producersWithSubtotals.AddRange(new List<CalcResultH2ProjectedProducer>
                        {
                            new ()
                            {
                                ProducerId = producer.ProducerId,
                                SubsidiaryId = null,
                                Level = CommonConstants.LevelOne.ToString(),
                                SubmissionPeriodCode = producer.SubmissionPeriodCode,
                                IsSubtotal = true,
                                ProjectedTonnageByMaterial = producer.ProjectedTonnageByMaterial
                            },
                            producer
                        });
                    } else {
                        producer.Level = CommonConstants.LevelOne.ToString();  
                        producersWithSubtotals.Add(producer);
                    }
                }
            }

            return producersWithSubtotals;

        }

        private static CalcResultH2ProjectedProducer SumProducerGroupTonnages(IGrouping<int, CalcResultH2ProjectedProducer> prodGroup, CalcResultH2ProjectedProducer producer)
        {
            RAMTonnage SumRAMTonnages(List<CalcResultH2ProjectedProducer> producers, string materialCode, Func<CalcResultH2ProjectedProducerMaterialTonnage, RAMTonnage?> getRAMTonnage) 
            {
                return new RAMTonnage {
                    Tonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.Tonnage ?? 0),
                    RedTonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.RedTonnage ?? 0),
                    RedMedicalTonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.RedMedicalTonnage ?? 0),
                    AmberTonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.AmberTonnage ?? 0),
                    AmberMedicalTonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.AmberMedicalTonnage ?? 0),
                    GreenTonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.GreenTonnage ?? 0),
                    GreenMedicalTonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.GreenMedicalTonnage ?? 0),
                };
            }

            return new CalcResultH2ProjectedProducer
            {
                ProducerId = prodGroup.Key,
                SubsidiaryId = null,
                Level = CommonConstants.LevelOne.ToString(),
                SubmissionPeriodCode = producer.SubmissionPeriodCode,
                IsSubtotal = true,
                ProjectedTonnageByMaterial = producer.ProjectedTonnageByMaterial.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => new CalcResultH2ProjectedProducerMaterialTonnage {
                        HouseholdRAMTonnage = SumRAMTonnages(prodGroup.ToList(), kvp.Key, p => p.HouseholdRAMTonnage),
                        PublicBinRAMTonnage = SumRAMTonnages(prodGroup.ToList(), kvp.Key, p => p.PublicBinRAMTonnage),
                        HouseholdDrinksContainerRAMTonnage = kvp.Key == MaterialCodes.Glass ? SumRAMTonnages(prodGroup.ToList(), kvp.Key, p => p.HouseholdDrinksContainerRAMTonnage) : null,
                        HouseholdTonnageDefaultedRed = prodGroup.Sum(p => p.ProjectedTonnageByMaterial[kvp.Key].HouseholdTonnageDefaultedRed),
                        PublicBinTonnageDefaultedRed = prodGroup.Sum(p => p.ProjectedTonnageByMaterial[kvp.Key].PublicBinTonnageDefaultedRed),
                        HouseholdDrinksContainerDefaultedRed = kvp.Key == MaterialCodes.Glass ? prodGroup.Sum(p => p.ProjectedTonnageByMaterial[kvp.Key].HouseholdDrinksContainerDefaultedRed ?? 0) : null,
                        TotalTonnage = prodGroup.Sum(p => p.ProjectedTonnageByMaterial[kvp.Key].TotalTonnage) 
                    })
            };
        }

        public static List<ProjectedProducersHeader> GetMaterialsBreakdownHeader(IEnumerable<MaterialDetail> materials)
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

        public static List<ProjectedProducersHeader> GetColumnHeaders(IEnumerable<MaterialDetail> materials)
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