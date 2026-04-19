using System;
using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ErrorReport;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.Service.Function.Models;
using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Modulation;

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
        private readonly ICalcResultProjectedProducersExporter calcResultProjectedProducersExporter;
        private readonly ICalcResultLaDisposalCostExporter laDisposalCostExporter;
        private readonly ICalcResultModulationExporter modulationExporter;
        private readonly ICommsCostExporter commsCostExporter;
        private readonly ICalcResultCancelledProducersExporter calcResultCancelledProducersExporter;
        private readonly ICalcResultErrorReportExporter calcResultErrorReportExporter;

        // Suppress SonarQube warning for constructor parameter count
        [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
        public CalcResultsExporter(
            ILateReportingExporter lateReporting,
            ICalcResultDetailExporter resultDetail,
            IOnePlusFourApportionmentExporter onePlusFourApportionment,
            ICalcResultLaDisposalCostExporter laDisposalCost,
            ICalcResultModulationExporter modulationExporter,
            ICalcResultScaledupProducersExporter calcResultScaledupProducers,
            ICalcResultPartialObligationsExporter calcResultPartialObligations,
            ICalcResultProjectedProducersExporter calcResultProjectedProducers,
            ILapcaptDetailExporter lapcaptDetail,
            ICalcResultParameterOtherCostExporter parameterOt,
            ICommsCostExporter commsCost,
            ICalcResultSummaryExporter calcResultSummary,
            ICalcResultCancelledProducersExporter calcResultCancelledProducers,
            ICalcResultErrorReportExporter calcResultErrorReport
        )
        {
            resultDetailexporter = resultDetail;
            onePlusFourApportionmentExporter = onePlusFourApportionment;
            lateReportingExporter = lateReporting;
            calcResultScaledupProducersExporter = calcResultScaledupProducers;
            calcResultPartialObligationsExporter = calcResultPartialObligations;
            calcResultProjectedProducersExporter = calcResultProjectedProducers;
            lapcaptDetailExporter = lapcaptDetail;
            parameterOtherCosts = parameterOt;
            calcResultSummaryExporter = calcResultSummary;
            laDisposalCostExporter = laDisposalCost;
            this.modulationExporter = modulationExporter;
            commsCostExporter = commsCost;
            calcResultCancelledProducersExporter = calcResultCancelledProducers;
            calcResultErrorReportExporter = calcResultErrorReport;
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

            modulationExporter.Export(calcResult.CalcResultLaDisposalCostData, calcResult.CalcResultModulation, csvContent);

            calcResultCancelledProducersExporter.Export(calcResult.CalcResultCancelledProducers, csvContent);

            if (calcResult.ApplyModulation)
            {
                calcResultProjectedProducersExporter.Export(calcResult.CalcResultProjectedProducers, csvContent);
            }
            else {
                calcResultScaledupProducersExporter.Export(calcResult.CalcResultScaledupProducers, csvContent);
            }

            calcResultPartialObligationsExporter.Export(calcResult.CalcResultPartialObligations, csvContent);

            calcResultSummaryExporter.Export(calcResult.CalcResultSummary, csvContent, calcResult.ApplyModulation);

            calcResultErrorReportExporter.Export(calcResult.CalcResultErrorReports, csvContent);

            return csvContent.ToString();
        }
    }
}
