using System.Text;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public interface ICalcResultSummaryExporter
{
    void Export(
        RunContext runContext,
        CalcResultSummary resultSummary,
        IImmutableList<MaterialDetail> materials,
        IReadOnlyList<int> scaledupProducerIds,
        IReadOnlyList<(int, string?)> partialObligations,
        StringBuilder csvContent
    );
}

public class CalcResultSummaryExporter : ICalcResultSummaryExporter
{
    public void Export(
        RunContext runContext,
        CalcResultSummary resultSummary,
        IImmutableList<MaterialDetail> materials,
        IReadOnlyList<int> scaledupProducerIds,
        IReadOnlyList<(int, string?)> partialProducerSubsidiaryIds,
        StringBuilder csvContent
    )
    {
        var partExporters = BuildPartExporters(scaledupProducerIds, partialProducerSubsidiaryIds);

        csvContent.AppendLine();
        csvContent.AppendLine();

        AddSummaryDataHeader(resultSummary, materials, runContext.RequiresModulation, csvContent, partExporters);

        foreach (var producer in resultSummary.ProducerDisposalFees)
            AddNewRow(csvContent, producer, runContext.RequiresModulation, partExporters);

        AddNewRow(csvContent, resultSummary.OverallTotal, runContext.RequiresModulation, partExporters);
    }

    private static void AddNewRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation, IReadOnlyList<ICalcResultSummaryPartExporter> partExporters)
    {
        foreach (var exporter in partExporters)
            exporter.AppendRow(csvContent, producer, applyModulation);
        csvContent.AppendLine();
    }

    private static IReadOnlyList<ICalcResultSummaryPartExporter> BuildPartExporters(
        IReadOnlyList<int> scaledupProducerIds,
        IReadOnlyList<(int, string?)> partialProducerSubsidiaryIds
    ) =>
    [
        new ProducerIdentityExporter(scaledupProducerIds, partialProducerSubsidiaryIds),
        new Section1MaterialsExporter(),
        new Section1DisposalFeeExporter(),
        new Section2aMaterialsExporter(),
        new Section2aCommsExporter(),
        new Section1DisposalExporter(),
        new Section2aComms2aExporter(),
        new CommsCost2aPercentageExporter(),
        new CommsCost2bExporter(),
        new CommsCost2cExporter(),
        new OnePlus2a2b2cExporter(),
        new ThreeSaCostsExporter(),
        new LaDataPrepCostsExporter(),
        new SaSetupCostsExporter(),
        new TotalBillBreakdownExporter(),
        new BillingInstructionsExporter(),
    ];

    private static void AddSummaryDataHeader(CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation, StringBuilder csvContent, IReadOnlyList<ICalcResultSummaryPartExporter> partExporters)
    {
        csvContent.AppendLine(CsvSanitiser.SanitiseData("Calculation Result"))
            .AppendLine()
            .AppendLine();

        csvContent.AppendLine(CsvSanitiser.SanitiseData("NOTE: Rows with 'Scaled-up tonnages?' = " +
            "Yes include reported tonnages for a period that have been scaled-up to a full 6 month equivalent period. " +
            "See 'Scaled-up Producers' table for details."));

        foreach (var exporter in partExporters)
            exporter.AppendSectionHeader(csvContent, resultSummary, materials, applyModulation);
        csvContent.AppendLine();

        foreach (var exporter in partExporters)
            exporter.AppendGroupHeader(csvContent, resultSummary, materials, applyModulation);
        csvContent.AppendLine();

        foreach (var exporter in partExporters)
            foreach (var header in exporter.GetColumnHeaders(materials, applyModulation))
                csvContent.Append(CsvSanitiser.SanitiseData(header));
        csvContent.AppendLine();
    }
}
