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
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter
{
    public class CalcResultsExporter : ICalcResultsExporter<CalcResult>
    {
        private readonly ICalcResultSummaryExporter calcResultSummaryExporter;
        private readonly ICalcResultDetailExporter resultDetailexporter;
        private readonly IOnePlusFourApportionmentExporter onePlusFourApportionmentExporter;
        private readonly ILapcaptDetailExporter lapcaptDetailExporter;
        private readonly ICalcResultParameterOtherCostExporter parameterOtherCosts;
        private readonly ILateReportingExporter lateReportingExporter;
        private readonly ICalcResultScaledupProducersExporter calcResultScaledupProducersExporter;
        private readonly ICalcResultPartialObligationsExporter calcResultPartialObligationsExporter;
        private readonly ICalcResultLaDisposalCostExporter laDisposalCostExporter;
        private readonly ICommsCostExporter commsCostExporter;
        private readonly ICalcResultCancelledProducersExporter calcResultCancelledProducersExporter;
        private readonly ICalcResultErrorReportExporter calcResultErrorReportExporter;

        // Suppress SonarQube warning for constructor parameter count
        [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
        public CalcResultsExporter(
            ILateReportingExporter lateReportingExporter,
            ICalcResultDetailExporter resultDetailexporter,
            IOnePlusFourApportionmentExporter onePlusFourApportionmentExporter,
            ICalcResultLaDisposalCostExporter laDisposalCostExporter,
            ICalcResultScaledupProducersExporter calcResultScaledupProducersExporter,
            ICalcResultPartialObligationsExporter calcResultPartialObligationsExporter,
            ILapcaptDetailExporter lapcaptDetailExporter,
            ICalcResultParameterOtherCostExporter parameterOtherCosts,
            ICommsCostExporter commsCostExporter,
            ICalcResultSummaryExporter calcResultSummaryExporter,
            ICalcResultCancelledProducersExporter calcResultCancelledProducersExporter,
            ICalcResultErrorReportExporter calcResultErrorReportExporter)
        {
            this.resultDetailexporter = resultDetailexporter;
            this.onePlusFourApportionmentExporter = onePlusFourApportionmentExporter;
            this.lateReportingExporter = lateReportingExporter;
            this.calcResultScaledupProducersExporter = calcResultScaledupProducersExporter;
            this.calcResultPartialObligationsExporter = calcResultPartialObligationsExporter;
            this.lapcaptDetailExporter = lapcaptDetailExporter;
            this.parameterOtherCosts = parameterOtherCosts;
            this.calcResultSummaryExporter = calcResultSummaryExporter;
            this.laDisposalCostExporter = laDisposalCostExporter;
            this.commsCostExporter = commsCostExporter;
            this.calcResultCancelledProducersExporter = calcResultCancelledProducersExporter;
            this.calcResultErrorReportExporter = calcResultErrorReportExporter;
        }

        public string Export(CalcResult calcResult)
        {
            if (calcResult == null)
            {
                throw new ArgumentNullException(nameof(calcResult), "The calcResult parameter cannot be null.");
            }

            var csvContent = new StringBuilder();
            resultDetailexporter.Export(calcResult.CalcResultDetail, csvContent);

            lapcaptDetailExporter.Export(calcResult.CalcResultLapcapData, csvContent);

            csvContent.Append(lateReportingExporter.Export(calcResult.CalcResultLateReportingTonnageData));

            parameterOtherCosts.Export(calcResult.CalcResultParameterOtherCost, csvContent);

            onePlusFourApportionmentExporter.Export(calcResult.CalcResultOnePlusFourApportionment, csvContent);

            commsCostExporter.Export(calcResult.CalcResultCommsCostReportDetail, csvContent);

            laDisposalCostExporter.Export(calcResult.CalcResultLaDisposalCostData, csvContent);

            calcResultCancelledProducersExporter.Export(calcResult.CalcResultCancelledProducers, csvContent);

            calcResultScaledupProducersExporter.Export(calcResult.CalcResultScaledupProducers, csvContent);

            calcResultPartialObligationsExporter.Export(calcResult.CalcResultPartialObligations, csvContent);

            calcResultSummaryExporter.Export(calcResult.CalcResultSummary, csvContent, calcResult.CalcResultModulation is not null);

            calcResultErrorReportExporter.Export(calcResult.CalcResultErrorReports, csvContent);

            return csvContent.ToString();
        }
    }
}