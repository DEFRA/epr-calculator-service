using System;
using System.Collections.Generic;
using System.Text;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CalculationResults;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2A;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.LateReportingTonnage;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter
{
    public class CalcResultsJsonExporter : ICalcBillingJsonExporter<CalcResult>
    {
        private readonly ICalcResultDetailJsonExporter calcResultDetailExporter;
        private readonly ICalcResultLapcapExporter lapcapExporter;
        private readonly ILateReportingTonnage lateReportingTonnageExporter;
        private readonly IOnePlusFourApportionmentJsonExporter onePlusFourApportionmentJsonExporter;
        private readonly ICommsCostJsonExporter commsCostExporter;
        private readonly ICommsCostByMaterial2AExporter commsCostByMaterial2AExporter;
        private readonly ICancelledProducersExporter cancelledProducersExporter;
        private readonly ICalcResultScaledupProducersJsonExporter calcResultScaledupProducersJsonExporter;
        private readonly ICalculationResultsExporter calculationResultsExporter;

        public CalcResultsJsonExporter(
            ICalcResultDetailJsonExporter calcResultDetailExporter,
            ICalcResultLapcapExporter calcResultLapcapExporter,
            ILateReportingTonnage lateReportingTonnageExporter,
            IOnePlusFourApportionmentJsonExporter onePlusFourApportionmentJsonExporter,
            ICommsCostJsonExporter commsCostExporter,
            ICommsCostByMaterial2AExporter commsCostByMaterial2AExporter,
            ICancelledProducersExporter cancelledProducersExporter,
            ICalcResultScaledupProducersJsonExporter calcResultScaledupProducersJsonExporter,
            ICalculationResultsExporter calculationResultsExporter)
        {
            this.calcResultDetailExporter = calcResultDetailExporter;
            this.lapcapExporter = calcResultLapcapExporter;
            this.lateReportingTonnageExporter = lateReportingTonnageExporter;
            this.onePlusFourApportionmentJsonExporter = onePlusFourApportionmentJsonExporter;
            this.commsCostExporter = commsCostExporter;
            this.commsCostByMaterial2AExporter = commsCostByMaterial2AExporter;
            this.cancelledProducersExporter = cancelledProducersExporter;
            this.calcResultScaledupProducersJsonExporter = calcResultScaledupProducersJsonExporter;
            this.calculationResultsExporter = calculationResultsExporter;
        }

        public string Export(CalcResult results, IEnumerable<int> acceptedProducerIds)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results), "The results parameter cannot be null.");
            }

            var content = new StringBuilder();

            // Detail section
            var caclResultDetail = calcResultDetailExporter.Export(results.CalcResultDetail);
            content.Append(caclResultDetail);

            // Lapcap data section
            var lapcapData = lapcapExporter.ConvertToJson(results.CalcResultLapcapData);
            content.Append(lapcapData);

            // Late reporting tonnages section
            var lateReportingTonnage = lateReportingTonnageExporter.Export(results.CalcResultLateReportingTonnageData);
            content.Append(lateReportingTonnage);

            // One plus four apportionment percentage section
            var onePlusFourApportionment = onePlusFourApportionmentJsonExporter.Export(results.CalcResultOnePlusFourApportionment);
            content.Append(onePlusFourApportionment);

            // Parameter communication costs section
            var parameterCommsCost = commsCostExporter.Export(results.CalcResultCommsCostReportDetail);
            content.Append(parameterCommsCost);

            // Communication costs by material 2a section
            var commsCostByMaterial2A = commsCostByMaterial2AExporter.Export(results.CalcResultCommsCostReportDetail.CalcResultCommsCostCommsCostByMaterial);
            content.Append(commsCostByMaterial2A);

            // La disposal cost data section


            // Cancelled producers section
            var cancelledProducers = cancelledProducersExporter.Export(results.CalcResultCancelledProducers);
            content.Append(cancelledProducers);

            // Scaledup Producers section
            var scaledupProducers = calcResultScaledupProducersJsonExporter.Export(results.CalcResultScaledupProducers, acceptedProducerIds);
            content.Append(scaledupProducers);

            // Summary section
            var summary = calculationResultsExporter.Export(results.CalcResultSummary, acceptedProducerIds);
            content.Append(summary);

            return content.ToString();
        }
    }
}
