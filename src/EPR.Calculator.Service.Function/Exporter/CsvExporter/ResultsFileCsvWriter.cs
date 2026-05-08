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
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter;

public interface IResultsFileCsvWriter
{
    string WriteToString(CalculatorRunContext runContext, CalcResult calcResult);
}

[SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
public class ResultsFileCsvWriter(
    ILateReportingExporter lateReporting,
    ICalcResultDetailExporter resultDetail,
    IOnePlusFourApportionmentExporter onePlusFourApportionment,
    ICalcResultLaDisposalCostExporter laDisposalCost,
    ICalcResultModulationExporter modulationExporter,
    ICalcResultScaledupProducersExporter calcResultScaledupProducers,
    ICalcResultPartialObligationsExporter calcResultPartialObligations,
    ICalcResultProjectedProducersExporter calcResultProjectedProducers,
    ILapcaptDetailExporter lapcaptDetail,
    ICalcResultParameterOtherCostExporter parameterOtherCosts,
    ICommsCostExporter commsCost,
    ICalcResultSummaryExporter calcResultSummary,
    ICalcResultCancelledProducersExporter calcResultCancelledProducers,
    ICalcResultErrorReportExporter calcResultErrorReport
)
    : IResultsFileCsvWriter
{
    public string WriteToString(CalculatorRunContext runContext, CalcResult calcResult)
    {
        var csvContent = new StringBuilder();
        resultDetail.Export(calcResult.CalcResultDetail, csvContent);

        lapcaptDetail.Export(calcResult.CalcResultLapcapData, csvContent);

        csvContent.Append(lateReporting.Export(calcResult.CalcResultLateReportingTonnageData));

        parameterOtherCosts.Export(calcResult.CalcResultParameterOtherCost, csvContent);

        onePlusFourApportionment.Export(calcResult.CalcResultOnePlusFourApportionment, csvContent);

        commsCost.Export(calcResult.CalcResultCommsCostReportDetail, csvContent);

        laDisposalCost.Export(calcResult.CalcResultLaDisposalCostData, csvContent);

        if (calcResult.Smcw is not null && calcResult.CalcResultModulation is not null)
            modulationExporter.Export(calcResult.CalcResultLaDisposalCostData, calcResult.Smcw, calcResult.CalcResultModulation, csvContent);

        calcResultCancelledProducers.Export(calcResult.CalcResultCancelledProducers, csvContent);

        if (runContext.RequiresModulation)
            calcResultProjectedProducers.Export(calcResult.CalcResultProjectedProducers, csvContent);
        else
            calcResultScaledupProducers.Export(calcResult.CalcResultScaledupProducers, csvContent);

        calcResultPartialObligations.Export(calcResult.CalcResultPartialObligations, csvContent, runContext.RequiresModulation);

        calcResultSummary.Export(calcResult.CalcResultSummary, csvContent, runContext.RequiresModulation);

        calcResultErrorReport.Export(calcResult.CalcResultErrorReports, csvContent);

        return csvContent.ToString();
    }
}
