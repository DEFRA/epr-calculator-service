using System.Diagnostics.CodeAnalysis;
using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ErrorReport;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Modulation;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter;

public interface ICalcResultsExporter
{
    Task<string> Export(CalculatorRunContext runContext, CalcResult calcResult);
}

[SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
public class CalcResultsExporter(
    IMaterialService materialService,
    ICalcResultLateReportingExporter lateReporting,
    ICalcResultDetailExporter resultDetail,
    ICalcResultOnePlusFourApportionmentExporter onePlusFourApportionment,
    ICalcResultLaDisposalCostExporter laDisposalCost,
    ICalcResultModulationExporter modulation,
    ICalcResultScaledupProducersExporter scaledUpProducers,
    ICalcResultPartialObligationsExporter partialObligations,
    ICalcResultProjectedProducersExporter projectedProducers,
    ICalcResultLapcapDataExporter lapcapData,
    ICalcResultParameterOtherCostExporter parameterOtherCosts,
    ICalcResultCommsCostExporter commsCost,
    ICalcResultSummaryExporter summary,
    ICalcResultCancelledProducersExporter cancelledProducers,
    ICalcResultErrorReportExporter calcResultErrorReport)
    : ICalcResultsExporter
{
    public async Task<string> Export(CalculatorRunContext runContext, CalcResult calcResult)
    {
        var materials = await materialService.GetMaterials();
        var csvContent = new StringBuilder();

        resultDetail.Export(calcResult.CalcResultDetail, csvContent);
        lapcapData.Export(calcResult.CalcResultLapcapData, materials, csvContent);
        lateReporting.Export(calcResult.CalcResultLateReportingTonnageData, materials, csvContent);
        parameterOtherCosts.Export(calcResult.CalcResultParameterOtherCost, csvContent);
        onePlusFourApportionment.Export(calcResult.CalcResultOnePlusFourApportionment, csvContent);
        commsCost.Export(calcResult.CalcResultCommsCostReportDetail, materials, csvContent);
        laDisposalCost.Export(runContext, calcResult.CalcResultLaDisposalCostData, materials, csvContent);

        if (calcResult.Smcw is not null && calcResult.CalcResultModulation is not null) {
            modulation.Export(calcResult.CalcResultLaDisposalCostData, calcResult.Smcw, calcResult.CalcResultModulation, csvContent);
        }

        cancelledProducers.Export(calcResult.CalcResultCancelledProducers, csvContent);

        if (runContext.RequiresModulation)
            projectedProducers.Export(calcResult.CalcResultProjectedProducers, materials, csvContent);
        else
            scaledUpProducers.Export(calcResult.CalcResultScaledupProducers, materials, showTotal : true, csvContent);

        partialObligations.Export(runContext, calcResult.CalcResultPartialObligations, materials, csvContent);
        summary.Export(runContext, calcResult.CalcResultSummary, csvContent);
        calcResultErrorReport.Export(calcResult.CalcResultErrorReports, csvContent);

        return csvContent.ToString();
    }
}
