using System;
using System.Text;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CalculationResults;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.LateReportingTonnage;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter
{
    public class CalcResultsJsonExporter : ICalcResultsExporter<CalcResult>
    {
        private readonly ICalcResultDetailExporter calcResultDetailExporter;
        private readonly ICalcResultLapcapExporter lapcapExporter;
        private readonly ILateReportingTonnage lateReportingTonnageExporter;
        private readonly IOnePlusFourApportionmentJsonExporter onePlusFourApportionmentJsonExporter;
        private readonly ICalcResultScaledupProducersJsonExporter calcResultScaledupProducersJsonExporter;
        private readonly ICalculationResultsExporter calculationResultsExporter;

        public CalcResultsJsonExporter(
            ICalcResultDetailExporter calcResultDetailExporter,
            ICalcResultLapcapExporter calcResultLapcapExporter,
            ILateReportingTonnage lateReportingTonnageExporter,
            IOnePlusFourApportionmentJsonExporter onePlusFourApportionmentJsonExporter,
            ICalcResultScaledupProducersJsonExporter calcResultScaledupProducersJsonExporter,
            ICalculationResultsExporter calculationResultsExporter)
        {
            this.calcResultDetailExporter = calcResultDetailExporter;
            this.lapcapExporter = lapcapExporter;
            this.lateReportingTonnageExporter = lateReportingTonnageExporter;
            this.onePlusFourApportionmentJsonExporter = onePlusFourApportionmentJsonExporter;
            this.calcResultScaledupProducersJsonExporter = calcResultScaledupProducersJsonExporter;
            this.calculationResultsExporter = calculationResultsExporter;
        }

        public string Export(CalcResult results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results), "The results parameter cannot be null.");
            }

            var content = new StringBuilder();

            var caclResultDetail = calcResultDetailExporter.Export(results.CalcResultDetail);
            content.Append(caclResultDetail);

            var lateReportingTonnage = lateReportingTonnageExporter.Export(results.CalcResultLateReportingTonnageData);
            content.Append(lateReportingTonnage);

            var onePlusFourApportionment = onePlusFourApportionmentJsonExporter.Export(results.CalcResultOnePlusFourApportionment);
            content.Append(onePlusFourApportionment);

            var scaledupProducers = calcResultScaledupProducersJsonExporter.Export(results.CalcResultScaledupProducers, []);
            content.Append(scaledupProducers);

            var summary = calculationResultsExporter.Export(results.CalcResultSummary, null, []);
            content.Append(summary);

            return content.ToString();
        }
    }
}
