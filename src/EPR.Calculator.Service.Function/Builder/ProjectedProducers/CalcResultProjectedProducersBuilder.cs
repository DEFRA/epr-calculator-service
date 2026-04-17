using System.Collections.Immutable;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.ProjectedProducers;

public interface ICalcResultProjectedProducersBuilder
{
    Task<CalcResultProjectedProducers> ConstructAsync(RunContext runContext);
}

public class CalcResultProjectedProducersBuilder(
    ApplicationDBContext dbContext,
    IMaterialService materialService
    )
    : ICalcResultProjectedProducersBuilder
{
    public async Task<CalcResultProjectedProducers> ConstructAsync(RunContext runContext)
    {
        var materials = await materialService.GetMaterials();

        var h2ProjectedProduers = await GetH2ProjectedProducers(runContext.RunId, materials);

        var h2ProjectedProducersWithSubtotals = AddSubtotals(h2ProjectedProduers);

        return new CalcResultProjectedProducers
        {
            H2ProjectedProducersHeaders = new ProjectedProducersHeaders
            {
                TitleHeader = new ProjectedProducersHeader
                {
                    Name = CalcResultProjectedProducersHeaders.H2ProjectedProducers,
                    ColumnIndex = 1
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

    public async Task<List<CalcResultH2ProjectedProducer>> GetH2ProjectedProducers(int runId, ImmutableArray<MaterialDto> materials)
    {
        var result = await (from run in dbContext.CalculatorRuns.AsNoTracking()
            join pd in dbContext.ProducerDetail.AsNoTracking() on run.Id equals pd.CalculatorRunId
            join prm in dbContext.ProducerReportedMaterial.AsNoTracking() on pd.Id equals prm.ProducerDetailId
            let submissionPeriod = (run.RelativeYearValue - 1) + "-H2"
            where pd.CalculatorRunId == runId && prm.SubmissionPeriod == submissionPeriod
            group prm by new { pd.ProducerId, pd.SubsidiaryId, submissionPeriod }
            into prms
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

    public static Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage> GetH2ProjectedTonnages(ImmutableArray<MaterialDto> materials, List<ProducerReportedMaterial> reportedMaterials)
    {
        return materials.ToDictionary(m => m.Code, m => GetH2ProjectedTonnage(m, reportedMaterials.Where(rm => rm.MaterialId == m.Id).ToList()));
    }

    public static CalcResultH2ProjectedProducerMaterialTonnage GetH2ProjectedTonnage(MaterialDto material, List<ProducerReportedMaterial> reportedMaterials)
    {
        decimal DefaultRAMToRed(RAMTonnage tonnage)
        {
            var ramTonnage = tonnage.RedTonnage + tonnage.RedMedicalTonnage + tonnage.AmberTonnage + tonnage.AmberMedicalTonnage + tonnage.GreenTonnage + tonnage.GreenMedicalTonnage;
            var diffTonnage = tonnage.Tonnage - ramTonnage;
            return diffTonnage > 0 ? diffTonnage : 0;
        }

        var householdRAMTonnage = GetRAMTonnage(PackagingTypes.Household, reportedMaterials);
        var publicBinRAMTonnage = GetRAMTonnage(PackagingTypes.PublicBin, reportedMaterials);
        var hdcRAMTonnage = material.Code == MaterialCodes.Glass ? GetRAMTonnage(PackagingTypes.HouseholdDrinksContainers, reportedMaterials) : null;

        return new CalcResultH2ProjectedProducerMaterialTonnage
        {
            HouseholdRAMTonnage = householdRAMTonnage,
            PublicBinRAMTonnage = publicBinRAMTonnage,
            HouseholdDrinksContainerRAMTonnage = hdcRAMTonnage,
            HouseholdTonnageDefaultedRed = DefaultRAMToRed(householdRAMTonnage),
            PublicBinTonnageDefaultedRed = DefaultRAMToRed(publicBinRAMTonnage),
            HouseholdDrinksContainerDefaultedRed = hdcRAMTonnage != null ? DefaultRAMToRed(hdcRAMTonnage) : null,
            TotalTonnage = householdRAMTonnage.Tonnage + publicBinRAMTonnage.Tonnage + (hdcRAMTonnage?.Tonnage ?? 0)
        };
    }

    private static RAMTonnage GetRAMTonnage(string packagingType, List<ProducerReportedMaterial> reportedMaterials)
    {
        decimal GetReportedTonnage(string packagingType, Func<ProducerReportedMaterial, decimal?> tonnageFunc) => reportedMaterials.Where(p => p.PackagingType == packagingType).Sum(t => tonnageFunc(t) ?? 0);

        return new RAMTonnage
        {
            Tonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnage),
            RedTonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnageRed),
            RedMedicalTonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnageRedMedical),
            AmberTonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnageAmber),
            AmberMedicalTonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnageAmberMedical),
            GreenTonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnageGreen),
            GreenMedicalTonnage = GetReportedTonnage(packagingType, t => t.PackagingTonnageGreenMedical)
        };
    }

    public static List<CalcResultH2ProjectedProducer> AddSubtotals(List<CalcResultH2ProjectedProducer> projectedProducers)
    {
        var producersWithSubtotals = new List<CalcResultH2ProjectedProducer>();
        var producerGroups = projectedProducers.GroupBy(p => p.ProducerId);

        foreach (var prodGroup in producerGroups)
        {
            if (prodGroup.Count() > 1)
            {
                producersWithSubtotals.AddRange(prodGroup.Select(p =>
                {
                    p.Level = CommonConstants.LevelTwo.ToString();
                    return p;
                }));

                producersWithSubtotals.Add(
                    SumProducerGroupTonnages(prodGroup, prodGroup.First())
                );
            }
            else
            {
                var producer = prodGroup.First();
                if (producer.SubsidiaryId != null)
                {
                    producer.Level = CommonConstants.LevelTwo.ToString();
                    producersWithSubtotals.AddRange(new List<CalcResultH2ProjectedProducer>
                    {
                        new()
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
                }
                else
                {
                    producer.Level = CommonConstants.LevelOne.ToString();
                    producersWithSubtotals.Add(producer);
                }
            }
        }

        return producersWithSubtotals;
    }

    private static CalcResultH2ProjectedProducer SumProducerGroupTonnages(IGrouping<int, CalcResultH2ProjectedProducer> prodGroup, CalcResultH2ProjectedProducer producer)
    {
        RAMTonnage SumRAMTonnages(List<CalcResultH2ProjectedProducer> producers, string materialCode, Func<CalcResultH2ProjectedProducerMaterialTonnage, RAMTonnage?> getRAMTonnage) =>
            new()
            {
                Tonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.Tonnage ?? 0),
                RedTonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.RedTonnage ?? 0),
                RedMedicalTonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.RedMedicalTonnage ?? 0),
                AmberTonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.AmberTonnage ?? 0),
                AmberMedicalTonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.AmberMedicalTonnage ?? 0),
                GreenTonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.GreenTonnage ?? 0),
                GreenMedicalTonnage = producers.Sum(p => getRAMTonnage(p.ProjectedTonnageByMaterial[materialCode])?.GreenMedicalTonnage ?? 0)
            };

        return new CalcResultH2ProjectedProducer
        {
            ProducerId = prodGroup.Key,
            SubsidiaryId = null,
            Level = CommonConstants.LevelOne.ToString(),
            SubmissionPeriodCode = producer.SubmissionPeriodCode,
            IsSubtotal = true,
            ProjectedTonnageByMaterial = producer.ProjectedTonnageByMaterial.ToDictionary(
                kvp => kvp.Key,
                kvp => new CalcResultH2ProjectedProducerMaterialTonnage
                {
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

    public static List<ProjectedProducersHeader> GetMaterialsBreakdownHeader(ImmutableArray<MaterialDto> materials)
    {
        var materialsBreakdownHeaders = new List<ProjectedProducersHeader>();
        var columnIndex = GetInitialColumnHeaders().Count + 1;

        foreach (var material in materials)
        {
            materialsBreakdownHeaders.Add(new ProjectedProducersHeader
            {
                Name = $"{material.Name} Breakdown",
                ColumnIndex = columnIndex
            });

            var materialHeaderCount = GetMaterialColumnHeaders().Count + GetPostFixColumnHeaders().Count;

            columnIndex = material.Code == MaterialCodes.Glass
                ? columnIndex + materialHeaderCount + GetGlassColumnHeaders().Count
                : columnIndex + materialHeaderCount;
        }

        return materialsBreakdownHeaders;
    }

    public static List<ProjectedProducersHeader> GetColumnHeaders(IEnumerable<MaterialDto> materials)
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
            new() { Name = CalcResultProjectedProducersHeaders.ProducerId },
            new() { Name = CalcResultProjectedProducersHeaders.SubsidiaryId },
            new() { Name = CalcResultProjectedProducersHeaders.Level },
            new() { Name = CalcResultProjectedProducersHeaders.SubmissionPeriodCode }
        };
    }

    private static List<ProjectedProducersHeader> GetPostFixColumnHeaders()
    {
        return new List<ProjectedProducersHeader>
        {
            new() { Name = CalcResultProjectedProducersHeaders.TotalTonnage }
        };
    }


    private static List<ProjectedProducersHeader> GetMaterialColumnHeaders()
    {
        return new List<ProjectedProducersHeader>
        {
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdPackagingTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdRedTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdAmberTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdGreenTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdRedMedicalTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdAmberMedicalTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdGreenMedicalTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdTonnageWithoutRAMDefaultedToRed },
            new() { Name = CalcResultProjectedProducersHeaders.PublicBinPackagingTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.PublicBinRedTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.PublicBinAmberTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.PublicBinGreenTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.PublicBinRedMedicalTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.PublicBinAmberMedicalTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.PublicBinGreenMedicalTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.PublicBinTonnageWithoutRAMDefaultedToRed }
        };
    }

    private static List<ProjectedProducersHeader> GetGlassColumnHeaders()
    {
        return new List<ProjectedProducersHeader>
        {
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersPackagingTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedMedicalTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberMedicalTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenMedicalTonnage },
            new() { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersTonnageWithoutRAMDefaultedToRed }
        };
    }
}