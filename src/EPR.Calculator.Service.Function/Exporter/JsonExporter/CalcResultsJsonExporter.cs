using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using EPR.Calculator.Service.Function.Models.JsonExporter;
using Newtonsoft.Json;

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

        [SuppressMessage("Constructor has 8 parameters, which is greater than the 7 authorized.", "S107", Justification = "This is suppressed for now and will be refactored later")]
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

            var billingFileContent = new JsonBillingFileExporter()
            {
                CalcResultDetail = calcResultDetailExporter.Export(results.CalcResultDetail),
                CalcResultLapcapData = lapcapExporter.ConvertToJson(results.CalcResultLapcapData),
                CalcResultLateReportingTonnageData = lateReportingTonnageExporter.Export(results.CalcResultLateReportingTonnageData),
                OnePlusFourApportionment = onePlusFourApportionmentJsonExporter.Export(results.CalcResultOnePlusFourApportionment),
                ParametersCommsCost = commsCostExporter.Export(results.CalcResultCommsCostReportDetail),
                CalcResult2aCommsDataByMaterial = commsCostByMaterial2AExporter.Export(results.CalcResultCommsCostReportDetail.CalcResultCommsCostCommsCostByMaterial),
                CancelledProducers = cancelledProducersExporter.Export(results.CalcResultCancelledProducers),
                ScaleUpProducers = calcResultScaledupProducersJsonExporter.Export(results.CalcResultScaledupProducers, acceptedProducerIds),
                CalculationResults = calculationResultsExporter.Export(results.CalcResultSummary, acceptedProducerIds)
            };

            return JsonConvert.SerializeObject(billingFileContent);
        }
    }
}
