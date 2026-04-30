using System.Diagnostics.CodeAnalysis;
using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.RejectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter;

public interface IBillingFileCsvWriter
{
    string WriteToString(BillingRunContext runContext, CalcResult calcResult);
}

[method: SuppressMessage("Constructor has 8 parameters, which is greater than the 7 authorized.", "S107")]
public class BillingFileCsvWriter(
    ILateReportingExporter lateReportingExporter,
    ICalcResultDetailExporter resultDetailExporter,
    IOnePlusFourApportionmentExporter onePlusFourApportionmentExporter,
    ICalcResultLaDisposalCostExporter laDisposalCostExporter,
    ICalcResultScaledupProducersExporter calcResultScaledUpProducersExporter,
    ICalcResultPartialObligationsExporter calcResultPartialObligationsExporter,
    ICalcResultProjectedProducersExporter calcResultProjectedProducersExporter,
    ILapcaptDetailExporter lapcaptDetailExporter,
    ICalcResultParameterOtherCostExporter parameterOtherCostsExporter,
    ICommsCostExporter commsCostExporter,
    ICalcResultSummaryExporter calcResultSummaryExporter,
    ICalcResultCancelledProducersExporter calcResultCancelledProducersExporter,
    ICalcResultRejectedProducersExporter calcResultRejectedProducersExporter)
    : IBillingFileCsvWriter
{
    public string WriteToString(BillingRunContext runContext, CalcResult calcResult)
    {
        var csvContent = new StringBuilder();
        resultDetailExporter.Export(calcResult.CalcResultDetail, csvContent);

        lapcaptDetailExporter.Export(calcResult.CalcResultLapcapData, csvContent);

        csvContent.Append(lateReportingExporter.Export(calcResult.CalcResultLateReportingTonnageData));

        parameterOtherCostsExporter.Export(calcResult.CalcResultParameterOtherCost, csvContent);

        onePlusFourApportionmentExporter.Export(calcResult.CalcResultOnePlusFourApportionment, csvContent);

        commsCostExporter.Export(calcResult.CalcResultCommsCostReportDetail, csvContent);

        laDisposalCostExporter.Export(calcResult.CalcResultLaDisposalCostData, csvContent);

        calcResultCancelledProducersExporter.Export(calcResult.CalcResultCancelledProducers, csvContent);

        if (calcResult.CalcResultModulation is not null)
        {
            var accepted = GetProjectedProducerForExport(calcResult.CalcResultProjectedProducers, runContext.AcceptedProducerIds);
            calcResultProjectedProducersExporter.Export(accepted, csvContent);
        }
        else
        {
            var acceptedProducers = GetScaledUpProducersForExport(calcResult.CalcResultScaledupProducers, runContext.AcceptedProducerIds);
            calcResultScaledUpProducersExporter.Export(acceptedProducers, csvContent);
        }

        calcResultPartialObligationsExporter.Export(GetPartialObligationsForExport(calcResult.CalcResultPartialObligations, runContext.AcceptedProducerIds), csvContent);

        var acceptedCalcResultSummary = GetAcceptedProducersCalcResults(calcResult.CalcResultSummary, runContext.AcceptedProducerIds);

        calcResultSummaryExporter.Export(acceptedCalcResultSummary, csvContent, calcResult.CalcResultModulation is not null);

        csvContent = ResetTotals(csvContent.ToString());

        calcResultRejectedProducersExporter.Export(calcResult.CalcResultRejectedProducers, csvContent);

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
            TwoCCommsCostsByCountryWithoutBadDebtProvision = calcResultSummary.TwoCCommsCostsByCountryWithoutBadDebtProvision
        };
    }

    private IEnumerable<CalcResultSummaryProducerDisposalFees> GetAcceptedProducerDisposalFees(
        IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
        IEnumerable<int> acceptedProducerIds)
    {
        var acceptedProducerFees = producerDisposalFees.Where(x => acceptedProducerIds.Contains(x.ProducerIdInt)
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
                ? GetScaledupProducers(producers.ScaledupProducers, acceptedProducerIds)
                : new List<CalcResultScaledupProducer>(),
            TitleHeader = producers.TitleHeader
        };
    }

    private IEnumerable<CalcResultScaledupProducer> GetScaledupProducers(
        IEnumerable<CalcResultScaledupProducer> scaledupProducers,
        IEnumerable<int> acceptedProducerIds)
    {
        var acceptedProducers = scaledupProducers.Where(x => acceptedProducerIds.Contains(x.ProducerId)
                                                             || x.ProducerId == 0).ToList();

        acceptedProducers.ForEach(x =>
        {
            if (x.IsTotalRow)
            {
                x.ScaledupProducerTonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>();

                ResetObjectUtil.ResetObject(x);
            }
        });
        return acceptedProducers;
    }

    public CalcResultPartialObligations GetPartialObligationsForExport(
        CalcResultPartialObligations producers,
        IEnumerable<int> acceptedProducerIds)
    {
        var acceptedProducers = producers.PartialObligations?.Where(x => acceptedProducerIds.Contains(x.ProducerId) || x.ProducerId == 0
        ).ToList() ?? new List<CalcResultPartialObligation>();

        return new CalcResultPartialObligations
        {
            ColumnHeaders = producers.ColumnHeaders,
            MaterialBreakdownHeaders = producers.MaterialBreakdownHeaders,
            PartialObligations = acceptedProducers,
            TitleHeader = producers.TitleHeader
        };
    }

    private CalcResultProjectedProducers GetProjectedProducerForExport(
        CalcResultProjectedProducers producers,
        IEnumerable<int> acceptedProducerIds)
    {
        var acceptedProducers = producers.H2ProjectedProducers?.Where(x => acceptedProducerIds.Contains(x.ProducerId) || x.ProducerId == 0
        ).ToList() ?? new List<CalcResultH2ProjectedProducer>();

        return new CalcResultProjectedProducers
        {
            H2ProjectedProducersHeaders = producers.H2ProjectedProducersHeaders,
            H2ProjectedProducers = acceptedProducers
        };
    }

    private StringBuilder ResetTotals(string sb)
    {
        var exceptTotals = sb.Substring(0, sb.LastIndexOf(CommonConstants.Totals) + CommonConstants.Totals.Length + 2);
        return new StringBuilder().Append(exceptTotals);
    }
}