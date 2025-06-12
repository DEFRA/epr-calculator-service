using EPR.Calculator.API.Exporter;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter
{
    public class BillingFileExporter : IBillingFileExporter<CalcResult>
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

        public BillingFileExporter(
            ILateReportingExporter lateReportingExporter,
            ICalcResultDetailExporter resultDetailexporter,
            IOnePlusFourApportionmentExporter onePlusFourApportionmentExporter,
            ICalcResultLaDisposalCostExporter laDisposalCostExporter,
            ICalcResultScaledupProducersExporter calcResultScaledupProducersExporter,
            ILapcaptDetailExporter lapcaptDetailExporter,
            ICalcResultParameterOtherCostExporter parameterOtherCosts,
            ICommsCostExporter commsCostExporter,
            ICalcResultSummaryExporter calcResultSummaryExporter)
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
        }

        public string Export(CalcResult results, IEnumerable<int> acceptedOrganisations)
        {
            if(results == null)
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

            calcResultScaledupProducersExporter.Export(results.CalcResultScaledupProducers, csvContent);

            calcResultSummaryExporter.Export(results.CalcResultSummary, csvContent);

            return csvContent.ToString();
        }
    }
}
