using EPR.Calculator.API.Exporter;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter
{
    using System;
    using System.Linq;
    using System.Text;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.RejectedProducers;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
    using EPR.Calculator.Service.Function.Models;

    public class CalcResultsExporter : ICalcResultsExporter<CalcResult>
    {
        private readonly ICalcResultSummaryExporter calcResultSummaryExporter;
        private readonly ICalcResultDetailExporter resultDetailexporter;
        private readonly IOnePlusFourApportionmentExporter onePlusFourApportionmentExporter;
        private readonly ILapcaptDetailExporter lapcaptDetailExporter;
        private readonly ICalcResultParameterOtherCostExporter parameterOtherCosts;
        private readonly ILateReportingExporter lateReportingExporter;
        private readonly ICalcResultScaledupProducersExporter calcResultScaledupProducersExporter;
        private readonly ICalcResultLaDisposalCostExporter laDisposalCostExporter;
        private readonly ICommsCostExporter commsCostExporter;
        private readonly ICalcResultCancelledProducersExporter calcResultCancelledProducersExporter;

        // Suppress SonarQube warning for constructor parameter count  
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
        public CalcResultsExporter(
            ILateReportingExporter lateReportingExporter,
            ICalcResultDetailExporter resultDetailexporter,
            IOnePlusFourApportionmentExporter onePlusFourApportionmentExporter,
            ICalcResultLaDisposalCostExporter laDisposalCostExporter,
            ICalcResultScaledupProducersExporter calcResultScaledupProducersExporter,
            ILapcaptDetailExporter lapcaptDetailExporter,
            ICalcResultParameterOtherCostExporter parameterOtherCosts,
            ICommsCostExporter commsCostExporter,
            ICalcResultSummaryExporter calcResultSummaryExporter,
            ICalcResultCancelledProducersExporter calcResultCancelledProducersExporter)
        {
            this.resultDetailexporter = resultDetailexporter;
            this.onePlusFourApportionmentExporter = onePlusFourApportionmentExporter;
            this.lateReportingExporter = lateReportingExporter;
            this.calcResultScaledupProducersExporter = calcResultScaledupProducersExporter;
            this.lapcaptDetailExporter = lapcaptDetailExporter;
            this.parameterOtherCosts = parameterOtherCosts;
            this.calcResultSummaryExporter = calcResultSummaryExporter;
            this.laDisposalCostExporter = laDisposalCostExporter;
            this.commsCostExporter = commsCostExporter;
            this.calcResultCancelledProducersExporter = calcResultCancelledProducersExporter;
        }

        public string Export(CalcResult results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results), "The results parameter cannot be null.");
            }

            var csvContent = new StringBuilder();
            resultDetailexporter.Export(results.CalcResultDetail, csvContent);

            lapcaptDetailExporter.Export(results.CalcResultLapcapData, csvContent);

            csvContent.Append(lateReportingExporter.Export(results.CalcResultLateReportingTonnageData));

            parameterOtherCosts.Export(results.CalcResultParameterOtherCost, csvContent);

            onePlusFourApportionmentExporter.Export(results.CalcResultOnePlusFourApportionment, csvContent);

            commsCostExporter.Export(results.CalcResultCommsCostReportDetail, csvContent);

            laDisposalCostExporter.Export(results.CalcResultLaDisposalCostData, csvContent);

            calcResultCancelledProducersExporter.Export(results.CalcResultCancelledProducers, csvContent);

            calcResultScaledupProducersExporter.Export(results.CalcResultScaledupProducers, csvContent);

            calcResultSummaryExporter.Export(results.CalcResultSummary, csvContent);

            return csvContent.ToString();
        }
    }
}