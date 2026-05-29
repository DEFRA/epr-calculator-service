using System.Text;
using EPR.Calculator.Service.Function.Builder.Summary.BillingInstructions;
using EPR.Calculator.Service.Function.Builder.Summary.LaDataPrepCosts;
using EPR.Calculator.Service.Function.Builder.Summary.OnePlus2A2B2C;
using EPR.Calculator.Service.Function.Builder.Summary.SaSetupCosts;
using EPR.Calculator.Service.Function.Builder.Summary.ThreeSa;
using EPR.Calculator.Service.Function.Builder.Summary.TotalBillBreakdown;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public interface IProducerSummaryExporter : ICalcResultSummaryPartExporter { }

public class ProducerSummaryExporter : IProducerSummaryExporter
{
    public IEnumerable<CalcResultSummaryHeader> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        var headers = new List<CalcResultSummaryHeader>();
        headers.AddRange(CalcResultSummaryUtil.Section2aComms2a());
        headers.AddRange(CalcResultSummaryUtil.CommsCost2aPercentage());
        headers.AddRange(CalcResultSummaryUtil.CommsCost2b());
        headers.AddRange(CalcResultSummaryUtil.CommsCost2c());
        headers.AddRange(OnePlus2A2B2CProducer.GetHeaders());
        headers.AddRange(ThreeSaCostsProducer.GetHeaders());
        headers.AddRange(LaDataPrepCostsProducer.GetHeaders());
        headers.AddRange(SaSetupCostsProducer.GetHeaders());
        headers.AddRange(TotalBillBreakdownProducer.GetHeaders());
        headers.AddRange(BillingInstructionsProducer.GetHeaders());
        return headers;
    }

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        AppendSectionContent(csvContent, producer.CommunicationCostsSectionTwoA);

        csvContent.Append(CsvSanitiser.SanitiseData(producer.PercentageofProducerReportedTonnagevsAllProducers, DecimalPlaces.Eight, null, false, true));

        AppendSectionContent(csvContent, producer.CommunicationCostsSectionTwoB);

        AppendTwoC(csvContent, producer);

        csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerTotalOnePlus2A2B2CWithBadDeptProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, DecimalPlaces.Eight, DecimalFormats.F8, false, true));

        AppendSectionContent(csvContent, producer.SchemeAdministratorOperatingCosts);
        AppendSectionContent(csvContent, producer.LocalAuthorityDataPreparationCosts);
        AppendSectionContent(csvContent, producer.OneOffSchemeAdministrationSetupCosts);
        AppendSectionContent(csvContent, producer.TotalProducerBillBreakdownCosts);

        AppendBillingInstructionsSection(csvContent, producer);
    }

    private static void AppendBillingInstructionsSection(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
    {
        csvContent.Append(CsvSanitiser.SanitiseData(CurrencyConverterUtils.FormattedCurrencyValue(producer.BillingInstructionSection!.CurrentYearInvoiceTotalToDate), DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.TonnageChangeSinceLastInvoice ?? CommonConstants.Hyphen));
        csvContent.Append(CsvSanitiser.SanitiseData(CurrencyConverterUtils.FormattedCurrencyValue(producer.BillingInstructionSection.LiabilityDifference), DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.MaterialThresholdBreached, appendLrmCharacterToPreventRenderedAsFormula: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.TonnageThresholdBreached, appendLrmCharacterToPreventRenderedAsFormula: true));
        csvContent.Append(CsvSanitiser.SanitiseData((producer.BillingInstructionSection.PercentageLiabilityDifference != null ? producer.BillingInstructionSection.PercentageLiabilityDifference.ToString() : CommonConstants.Hyphen), DecimalPlaces.Two, null, false, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.MaterialPercentageThresholdBreached, appendLrmCharacterToPreventRenderedAsFormula: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.TonnagePercentageThresholdBreached, appendLrmCharacterToPreventRenderedAsFormula: true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.SuggestedBillingInstruction));
        csvContent.Append(CsvSanitiser.SanitiseData(CurrencyConverterUtils.FormattedCurrencyValue(producer.BillingInstructionSection.SuggestedInvoiceAmount), DecimalPlaces.Two, null, true));
    }

    private static void AppendTwoC(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
    {
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCTotalProducerFeeForCommsCostsWithBadDebt, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCEnglandTotalWithBadDebt, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCWalesTotalWithBadDebt, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCScotlandTotalWithBadDebt, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCNorthernIrelandTotalWithBadDebt, DecimalPlaces.Two, null, true));
    }

    private static void AppendSectionContent(StringBuilder csvContent, CalcResultSummaryBadDebtProvision? costs)
    {
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.TotalProducerFeeWithoutBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.BadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.TotalProducerFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.EnglandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.WalesTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.ScotlandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(costs?.NorthernIrelandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
    }

}
