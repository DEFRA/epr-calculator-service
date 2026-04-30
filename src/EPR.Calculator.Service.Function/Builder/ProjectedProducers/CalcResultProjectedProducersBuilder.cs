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
    IMaterialService materialService)
    : ICalcResultProjectedProducersBuilder
{
    public async Task<CalcResultProjectedProducers> ConstructAsync(RunContext runContext)
    {
        var materials = await materialService.GetMaterials();

        var reportedMaterialsForRun = await GetReportedMaterialsForRun(runContext.RunId);
        var submissionPeriod = (string i) => $"{runContext.RelativeYear.Value - 1}-{i}";
        var h2ReportedMaterials = reportedMaterialsForRun.Where(r => r.SubmissionPeriod == submissionPeriod("H2")).ToList();
        var h1ReportedMaterials = reportedMaterialsForRun.Where(r => r.SubmissionPeriod == submissionPeriod("H1")).ToList();

        var h2ProjectedProduers = H2ProjectedProducersBuilderUtils.GetProjectedProducers(h2ReportedMaterials, materials);
        var h2ProjectedProducersWithSubtotals = AddSubtotals(
            h2ProjectedProduers,
            H2ProjectedProducersBuilderUtils.CreateParentProducer,
            H2ProjectedProducersBuilderUtils.SumProducerGroupTonnages
        );
        var h1ProjectedProduers = H1ProjectedProducersBuilderUtils.GetProjectedProducers(h1ReportedMaterials, h2ProjectedProduers, materials);
        var h1ProjectedProducersWithSubtotals = AddSubtotals(
            h1ProjectedProduers,
            H1ProjectedProducersBuilderUtils.CreateParentProducer,
            H1ProjectedProducersBuilderUtils.SumProducerGroupTonnages
        );

        return new CalcResultProjectedProducers
        {
            H2ProjectedProducersHeaders = H2ProjectedProducersBuilderUtils.GetProjectedProducerHeaders(materials),
            H1ProjectedProducersHeaders = H1ProjectedProducersBuilderUtils.GetProjectedProducerHeaders(materials),
            H2ProjectedProducers = h2ProjectedProducersWithSubtotals.OrderBy(p => p.ProducerId).ThenBy(p => p.Level).ThenBy(p => p.SubsidiaryId).ToList(),
            H1ProjectedProducers = h1ProjectedProducersWithSubtotals.OrderBy(p => p.ProducerId).ThenBy(p => p.Level).ThenBy(p => p.SubsidiaryId).ToList()
        };
    }

    private async Task<List<ProducerReportedMaterialsForSubmissionPeriod>> GetReportedMaterialsForRun(int runId)
    {
        return await (from run in dbContext.CalculatorRuns.AsNoTracking()
            join pd in dbContext.ProducerDetail.AsNoTracking() on run.Id equals pd.CalculatorRunId
            join prm in dbContext.ProducerReportedMaterial.AsNoTracking() on pd.Id equals prm.ProducerDetailId
            where pd.CalculatorRunId == runId
            group prm by new { pd.ProducerId, pd.SubsidiaryId, prm.SubmissionPeriod }
            into prms
            select new ProducerReportedMaterialsForSubmissionPeriod(prms.Key.ProducerId, prms.Key.SubsidiaryId, prms.Key.SubmissionPeriod, prms.ToList())).ToListAsync();
    }

    public static RAMTonnage GetRAMTonnage(string packagingType, List<ProducerReportedMaterial> reportedMaterials)
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

    public static decimal TonnageWithoutRAM(RAMTonnage tonnage)
    {
        return Math.Max(0, tonnage.Tonnage - tonnage.GetTotalRamTonnage());
    }

    public static RAMTonnage SumRAMTonnages(List<ICalcResultProjectedProducer> producers, string materialCode, Func<CalcResultProjectedProducerMaterialTonnage, RAMTonnage?> getRAMTonnage)
    {
        decimal tonnage = 0, red = 0, redMed = 0, amber = 0, amberMed = 0, green = 0, greenMed = 0;

        foreach (var p in producers)
        {
            var material = p.ProjectedTonnageByMaterial.FirstOrDefault(v => v.Key == materialCode).Value;
            if (material == null) continue;

            var ram = getRAMTonnage(material);
            if (ram == null) continue;

            tonnage += ram.Tonnage;
            red += ram.RedTonnage;
            redMed += ram.RedMedicalTonnage;
            amber += ram.AmberTonnage;
            amberMed += ram.AmberMedicalTonnage;
            green += ram.GreenTonnage;
            greenMed += ram.GreenMedicalTonnage;
        }

        return new RAMTonnage
        {
            Tonnage = tonnage,
            RedTonnage = red,
            RedMedicalTonnage = redMed,
            AmberTonnage = amber,
            AmberMedicalTonnage = amberMed,
            GreenTonnage = green,
            GreenMedicalTonnage = greenMed
        };
    }

    private static List<TSubmissionPeriodProducer> AddSubtotals<TSubmissionPeriodProducer>(
        List<TSubmissionPeriodProducer> projectedProducers,
        Func<TSubmissionPeriodProducer, TSubmissionPeriodProducer> createSubtotal,
        Func<IEnumerable<TSubmissionPeriodProducer>, TSubmissionPeriodProducer> sumProducerGroupTonnages)
        where TSubmissionPeriodProducer : ICalcResultProjectedProducer
    {
        var result = new List<TSubmissionPeriodProducer>();
        var producerGroups = projectedProducers.GroupBy(p => p.ProducerId);

        foreach (var group in producerGroups)
        {
            if (group.Count() > 1)
            {
                var updatedGroup = group.Select(p => p with { Level = CommonConstants.LevelTwo.ToString() });

                result.AddRange(updatedGroup);
                result.Add(
                    sumProducerGroupTonnages(group)
                );
            }
            else
            {
                var producer = group.First();

                if (producer.SubsidiaryId != null)
                {
                    var levelTwoProd = producer with { Level = CommonConstants.LevelTwo.ToString() };

                    var subtotal = createSubtotal(producer) with
                    {
                        Level = CommonConstants.LevelOne.ToString(),
                        IsSubtotal = true,
                        SubsidiaryId = null
                    };

                    result.Add(subtotal);
                    result.Add(levelTwoProd);
                }
                else
                {
                    var levelOneProd = producer with { Level = CommonConstants.LevelOne.ToString() };
                    result.Add(levelOneProd);
                }
            }
        }

        return result;
    }
}

public class ProducerReportedMaterialsForSubmissionPeriod
{
    public ProducerReportedMaterialsForSubmissionPeriod(int producerId, string? subsidiaryId, string submissionPeriod, List<ProducerReportedMaterial> reportedMaterials)
    {
        ProducerId = producerId;
        SubsidiaryId = subsidiaryId;
        SubmissionPeriod = submissionPeriod;
        ReportedMaterials = reportedMaterials;
    }

    public int ProducerId { get; }
    public string? SubsidiaryId { get; }
    public string SubmissionPeriod { get; }
    public List<ProducerReportedMaterial> ReportedMaterials { get; }
}
