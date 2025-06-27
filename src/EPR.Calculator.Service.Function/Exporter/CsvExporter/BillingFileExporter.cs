using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter
{
    public class BillingFileExporter : IBillingFileExporter<CalcResult>
    {
        private readonly ICalcResultSummaryExporter calcResultSummaryExporterCsv;
        private readonly ICalcResultDetailExporter resultDetailexporterCsv;
        private readonly IOnePlusFourApportionmentExporter onePlusFourApportionmentExporterCsv;
        private readonly ILapcaptDetailExporter lapcaptDetailExporterCsv;
        private readonly ICalcResultParameterOtherCostExporter parameterOtherCostsCsv;
        private readonly ILateReportingExporter lateReportingExporterCsv;
        private readonly ICalcResultScaledupProducersExporter calcResultScaledupProducersExporterCsv;
        private readonly ICalcResultLaDisposalCostExporter laDisposalCostExporterCsv;
        private readonly ICommsCostExporter commsCostExporterCsv;
        private readonly ICalcResultCancelledProducersExporter calcResultCancelledProducersExporterCsv;

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S107:Methods should not have too many parameters",
            Justification = "Temporaraly suppressed - will refactor later.")]
        public BillingFileExporter(
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
            this.resultDetailexporterCsv = resultDetailexporter;
            this.onePlusFourApportionmentExporterCsv = onePlusFourApportionmentExporter;
            this.lateReportingExporterCsv = lateReportingExporter;
            this.calcResultScaledupProducersExporterCsv = calcResultScaledupProducersExporter;
            this.lapcaptDetailExporterCsv = lapcaptDetailExporter;
            this.parameterOtherCostsCsv = parameterOtherCosts;
            this.calcResultSummaryExporterCsv = calcResultSummaryExporter;
            this.laDisposalCostExporterCsv = laDisposalCostExporter;
            this.commsCostExporterCsv = commsCostExporter;
            this.calcResultCancelledProducersExporterCsv = calcResultCancelledProducersExporter;
        }

        public string Export(CalcResult results, IEnumerable<int> acceptedProducerIds)
        {
            var csvContent = new StringBuilder();
            resultDetailexporterCsv.Export(results.CalcResultDetail, csvContent);

            lapcaptDetailExporterCsv.Export(results.CalcResultLapcapData, csvContent);

            csvContent.Append(lateReportingExporterCsv.Export(results.CalcResultLateReportingTonnageData));

            parameterOtherCostsCsv.Export(results.CalcResultParameterOtherCost, csvContent);

            onePlusFourApportionmentExporterCsv.Export(results.CalcResultOnePlusFourApportionment, csvContent);

            commsCostExporterCsv.Export(results.CalcResultCommsCostReportDetail, csvContent);

            laDisposalCostExporterCsv.Export(results.CalcResultLaDisposalCostData, csvContent);

            calcResultCancelledProducersExporterCsv.Export(results.CalcResultCancelledProducers, csvContent);

            var acceptedProducers = GetScaledUpProducersForExport(
                results.CalcResultScaledupProducers,
                acceptedProducerIds);
            calcResultScaledupProducersExporterCsv.Export(acceptedProducers, csvContent);

            var acceptedCalcResultSummary = GetAcceptedProducersCalcResults(results.CalcResultSummary, acceptedProducerIds);
            calcResultSummaryExporterCsv.Export(acceptedCalcResultSummary, csvContent);

            return csvContent.ToString();
        }

        private CalcResultSummary GetAcceptedProducersCalcResults(CalcResultSummary calcResultSummary, IEnumerable<int> acceptedProducerIds)
        {
            return new CalcResultSummary
            {
                BadDebtProvisionFor1 = calcResultSummary.BadDebtProvisionFor1,
                BadDebtProvisionFor2A = calcResultSummary.BadDebtProvisionFor2A,
                BadDebtProvisionTitleSection3 = calcResultSummary.BadDebtProvisionTitleSection3,
                ColumnHeaders = calcResultSummary.ColumnHeaders,
                CommsCostHeaderBadDebtProvisionFor2bTitle = calcResultSummary.CommsCostHeaderBadDebtProvisionFor2bTitle,
                CommsCostHeaderWithBadDebtFor2bTitle = calcResultSummary.CommsCostHeaderWithBadDebtFor2bTitle,
                CommsCostHeaderWithoutBadDebtFor2bTitle = calcResultSummary.CommsCostHeaderWithoutBadDebtFor2bTitle,
                LaDataPrepCostsBadDebtProvisionTitleSection4 = calcResultSummary.LaDataPrepCostsBadDebtProvisionTitleSection4,
                LaDataPrepCostsTitleSection4 = calcResultSummary.LaDataPrepCostsTitleSection4,
                LaDataPrepCostsWithBadDebtProvisionTitleSection4 = calcResultSummary.LaDataPrepCostsWithBadDebtProvisionTitleSection4,
                MaterialBreakdownHeaders = calcResultSummary.MaterialBreakdownHeaders,
                NotesHeader = calcResultSummary.NotesHeader,
                ProducerDisposalFees = GetAcceptedProducerDisposalFees(calcResultSummary.ProducerDisposalFees, acceptedProducerIds),
                ProducerDisposalFeesHeaders = calcResultSummary.ProducerDisposalFeesHeaders,
                ResultSummaryHeader = calcResultSummary.ResultSummaryHeader,
                SaOperatingCostsWithTitleSection3 = calcResultSummary.SaOperatingCostsWithTitleSection3,
                SaOperatingCostsWoTitleSection3 = calcResultSummary.SaOperatingCostsWoTitleSection3,
                SaSetupCostsBadDebtProvisionTitleSection5 = calcResultSummary.SaSetupCostsBadDebtProvisionTitleSection5,
                SaSetupCostsTitleSection5 = calcResultSummary.SaSetupCostsTitleSection5,
                SaSetupCostsWithBadDebtProvisionTitleSection5 = calcResultSummary.SaSetupCostsWithBadDebtProvisionTitleSection5,
                TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A = calcResultSummary.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A,
                TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A = calcResultSummary.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A,
                TotalFeeforLADisposalCostswithBadDebtprovision1 = calcResultSummary.TotalFeeforLADisposalCostswithBadDebtprovision1,
                TotalFeeforLADisposalCostswoBadDebtprovision1 = calcResultSummary.TotalFeeforLADisposalCostswoBadDebtprovision1,
                TotalOnePlus2A2B2CFeeWithBadDebtProvision = calcResultSummary.TotalOnePlus2A2B2CFeeWithBadDebtProvision,
                TwoCBadDebtProvision = calcResultSummary.TwoCBadDebtProvision,
                TwoCCommsCostsByCountryWithBadDebtProvision = calcResultSummary.TwoCCommsCostsByCountryWithBadDebtProvision,
                TwoCCommsCostsByCountryWithoutBadDebtProvision = calcResultSummary.TwoCCommsCostsByCountryWithoutBadDebtProvision,
            };
        }

        private IEnumerable<CalcResultSummaryProducerDisposalFees> GetAcceptedProducerDisposalFees(
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            IEnumerable<int> acceptedProducerIds)
        {
            var acceptedProducerFees = producerDisposalFees.Where(
                x => acceptedProducerIds.Contains(x.ProducerIdInt)
                || x.ProducerIdInt == 0).ToList();            

            acceptedProducerFees.ForEach(x =>
            {
                if (x.isOverallTotalRow)
                {
                    x.ProducerCommsFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>();
                    x.ProducerDisposalFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>();
                    ResetObjectUtil.ResetObject(x);
                }
            });
            return acceptedProducerFees;
        }

        public CalcResultScaledupProducers GetScaledUpProducersForExport(
            CalcResultScaledupProducers producers,
            IEnumerable<int> acceptedProducerIds)
        {
            return new CalcResultScaledupProducers
            {
                ColumnHeaders = producers.ColumnHeaders,
                MaterialBreakdownHeaders = producers.MaterialBreakdownHeaders,
                ScaledupProducers = producers.ScaledupProducers != null
                    ?
                    GetScaledupProducers(producers.ScaledupProducers, acceptedProducerIds)
                    :
                    new List<CalcResultScaledupProducer>(),
                TitleHeader = producers.TitleHeader,
            };
        }

        private IEnumerable<CalcResultScaledupProducer> GetScaledupProducers(
            IEnumerable<CalcResultScaledupProducer> scaledupProducers,
            IEnumerable<int> acceptedProducerIds)
        {
            var acceptedProducers = scaledupProducers.Where(
                x => acceptedProducerIds.Contains(x.ProducerId)
                || x.ProducerId == 0).ToList();

            acceptedProducers.ForEach(x =>
            {
                if (x.IsTotalRow)
                {
                    ResetObjectUtil.ResetObject(x);
                }
            });
            return acceptedProducers;
        }
       
    }
}