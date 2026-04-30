using System.Diagnostics.CodeAnalysis;
using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ErrorReport;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
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
    ILateReportingExporter lateReportingExporter,
    ICalcResultDetailExporter resultDetailexporter,
    IOnePlusFourApportionmentExporter onePlusFourApportionmentExporter,
    ICalcResultLaDisposalCostExporter laDisposalCostExporter,
    ICalcResultScaledupProducersExporter calcResultScaledupProducersExporter,
    ICalcResultPartialObligationsExporter calcResultPartialObligationsExporter,
    ICalcResultProjectedProducersExporter calcResultProjectedProducersExporter,
    ILapcaptDetailExporter lapcaptDetailExporter,
    ICalcResultParameterOtherCostExporter parameterOtherCostsExporter,
    ICommsCostExporter commsCostExporter,
    ICalcResultSummaryExporter calcResultSummaryExporter,
    ICalcResultCancelledProducersExporter calcResultCancelledProducersExporter,
    ICalcResultErrorReportExporter calcResultErrorReportExporter)
    : IResultsFileCsvWriter
{
    public string WriteToString(CalculatorRunContext runContext, CalcResult calcResult)
    {
        var csvContent = new StringBuilder();
        resultDetailexporter.Export(calcResult.CalcResultDetail, csvContent);

        lapcaptDetailExporter.Export(calcResult.CalcResultLapcapData, csvContent);

        csvContent.Append(lateReportingExporter.Export(calcResult.CalcResultLateReportingTonnageData));

        parameterOtherCostsExporter.Export(calcResult.CalcResultParameterOtherCost, csvContent);

        onePlusFourApportionmentExporter.Export(calcResult.CalcResultOnePlusFourApportionment, csvContent);

        commsCostExporter.Export(calcResult.CalcResultCommsCostReportDetail, csvContent);

        laDisposalCostExporter.Export(calcResult.CalcResultLaDisposalCostData, csvContent);

        calcResultCancelledProducersExporter.Export(calcResult.CalcResultCancelledProducers, csvContent);

        if (calcResult.CalcResultModulation is not null)
        {
            calcResultProjectedProducersExporter.Export(calcResult.CalcResultProjectedProducers, csvContent);
        }
        else
        {
            calcResultScaledupProducersExporter.Export(calcResult.CalcResultScaledupProducers, csvContent);
        }

        calcResultPartialObligationsExporter.Export(calcResult.CalcResultPartialObligations, csvContent);

        calcResultSummaryExporter.Export(calcResult.CalcResultSummary, csvContent, calcResult.CalcResultModulation is not null);

        calcResultErrorReportExporter.Export(calcResult.CalcResultErrorReports, csvContent);

        return csvContent.ToString();
    }
}