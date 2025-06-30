using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CalculationResults;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CancelledProducersData;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2A;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.LaDisposalCostData;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.LateReportingTonnage;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter
{
    public class CalcResultsJsonExporter : ICalcBillingJsonExporter<CalcResult>
    {
        private readonly IMaterialService materialService;
        private readonly ICalcResultDetailJsonExporter calcResultDetailExporter;
        private readonly ICalcResultLapcapExporter lapcapExporter;
        private readonly ILateReportingTonnage lateReportingTonnageExporter;
        private readonly IParametersOtherJsonExporter parametersOtherExporter;
        private readonly IOnePlusFourApportionmentJsonExporter onePlusFourApportionmentJsonExporter;
        private readonly ICommsCostJsonExporter commsCostExporter;
        private readonly ICommsCostByMaterial2AExporter commsCostByMaterial2AExporter;
        private readonly ICalcResultCommsCostOnePlusFourApportionmentExporter calcResultCommsCostOnePlusFourApportionmentExporter;
        private readonly ICalcResultLaDisposalCostDataExporter calcResultLaDisposalCostDataExporter;
        private readonly ICancelledProducersExporter cancelledProducersExporter;
        private readonly ICalcResultScaledupProducersJsonExporter calcResultScaledupProducersJsonExporter;
        private readonly ICalculationResultsExporter calculationResultsExporter;

        [SuppressMessage("Constructor has 8 parameters, which is greater than the 7 authorized.", "S107", Justification = "This is suppressed for now and will be refactored later")]
        public CalcResultsJsonExporter(
            IMaterialService materialService,
            ICalcResultDetailJsonExporter calcResultDetailExporter,
            ICalcResultLapcapExporter calcResultLapcapExporter,
            ILateReportingTonnage lateReportingTonnageExporter,
            IParametersOtherJsonExporter parametersOtherExporter,
            IOnePlusFourApportionmentJsonExporter onePlusFourApportionmentJsonExporter,
            ICommsCostJsonExporter commsCostExporter,
            ICommsCostByMaterial2AExporter commsCostByMaterial2AExporter,
            ICalcResultCommsCostOnePlusFourApportionmentExporter calcResultCommsCostOnePlusFourApportionmentExporter,
            ICalcResultLaDisposalCostDataExporter calcResultLaDisposalCostDataExporter,
            ICancelledProducersExporter cancelledProducersExporter,
            ICalcResultScaledupProducersJsonExporter calcResultScaledupProducersJsonExporter,
            ICalculationResultsExporter calculationResultsExporter)
        {
            this.materialService = materialService;
            this.calcResultDetailExporter = calcResultDetailExporter;
            this.lapcapExporter = calcResultLapcapExporter;
            this.lateReportingTonnageExporter = lateReportingTonnageExporter;
            this.parametersOtherExporter = parametersOtherExporter;
            this.onePlusFourApportionmentJsonExporter = onePlusFourApportionmentJsonExporter;
            this.commsCostExporter = commsCostExporter;
            this.commsCostByMaterial2AExporter = commsCostByMaterial2AExporter;
            this.calcResultCommsCostOnePlusFourApportionmentExporter = calcResultCommsCostOnePlusFourApportionmentExporter;
            this.calcResultLaDisposalCostDataExporter = calcResultLaDisposalCostDataExporter;
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

            var materials = this.materialService.GetMaterials().Result;

            var billingFileContent = new JsonBillingFileExporter();
            billingFileContent.CalcResultDetail = calcResultDetailExporter.Export(results.CalcResultDetail);
            billingFileContent.CalcResultLapcapData = lapcapExporter.Export(results.CalcResultLapcapData);
            billingFileContent.CalcResultLateReportingTonnageData = lateReportingTonnageExporter.Export(results.CalcResultLateReportingTonnageData);
            billingFileContent.ParametersOther = parametersOtherExporter.Export(results.CalcResultParameterOtherCost);
            billingFileContent.OnePlusFourApportionment = onePlusFourApportionmentJsonExporter.Export(results.CalcResultOnePlusFourApportionment);
            billingFileContent.ParametersCommsCost = commsCostExporter.Export(results.CalcResultCommsCostReportDetail);
            billingFileContent.CalcResult2aCommsDataByMaterial = commsCostByMaterial2AExporter.Export(results.CalcResultCommsCostReportDetail.CalcResultCommsCostCommsCostByMaterial);
            billingFileContent.CalcResult2bCommsDataByUkWide = calcResultCommsCostOnePlusFourApportionmentExporter.ConvertToJsonByUKWide(results.CalcResultCommsCostReportDetail);
            billingFileContent.CalcResult2cCommsDataByCountry = calcResultCommsCostOnePlusFourApportionmentExporter.ConvertToJsonByCountry(results.CalcResultCommsCostReportDetail);
            billingFileContent.CalcResultLaDisposalCostData = calcResultLaDisposalCostDataExporter.Export(results.CalcResultLaDisposalCostData.CalcResultLaDisposalCostDetails);
            billingFileContent.CancelledProducers = cancelledProducersExporter.Export(results.CalcResultCancelledProducers);
            billingFileContent.ScaleUpProducers = calcResultScaledupProducersJsonExporter.Export(results.CalcResultScaledupProducers, acceptedProducerIds, materials);
            billingFileContent.CalculationResults = calculationResultsExporter.Export(results.CalcResultSummary, acceptedProducerIds, materials);

           
            return JsonSerializer.Serialize(
                billingFileContent,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                });
        }
    }
}
