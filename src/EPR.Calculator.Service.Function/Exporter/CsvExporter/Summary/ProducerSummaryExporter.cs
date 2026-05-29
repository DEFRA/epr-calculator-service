using System.Diagnostics.CodeAnalysis;
using System.Text;
using EPR.Calculator.API.Data.Enums;
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
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public interface IProducerSummaryExporter : ICalcResultSummaryPartExporter { }

public class ProducerSummaryExporter : IProducerSummaryExporter
{
    public IEnumerable<CalcResultSummaryHeader> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        var headers = new List<CalcResultSummaryHeader>();
        headers.AddRange(CalcResultSummaryUtil.Section1Materials(materials, applyModulation));
        headers.AddRange(CalcResultSummaryUtil.Section1DisposalFee());
        headers.AddRange(CalcResultSummaryUtil.Section2aMaterials(materials));
        headers.AddRange(CalcResultSummaryUtil.Section2aComms());
        headers.AddRange(CalcResultSummaryUtil.Section1Disposal());
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
        bool isNotTotal = producer.LeaverDate != CommonConstants.Totals;

        foreach (var (key, disposalFee) in producer.ProducerDisposalFeesByMaterial)
        {
            AppendProducerDisposalFeesByMaterial(csvContent, producer, key, disposalFee, applyModulation, isNotTotal);
        }

        AppendProducerDisposalSummary(csvContent, producer);

        AppendProducerCommsFeesByMaterial(csvContent, producer, isNotTotal);

        AppendProducerCommsFee(csvContent, producer);

        AppendSectionContent(csvContent, producer.LocalAuthorityDisposalCostsSectionOne);
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

    private static void AppendProducerCommsFee(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
    {
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerCommsFee, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionComms, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerCommsFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalComms, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalComms, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalComms, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalComms, DecimalPlaces.Two, null, true));
    }

    private void AppendProducerDisposalSummary(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
    {
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerDisposalFee, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerDisposalFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotal, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotal, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotal, DecimalPlaces.Two, null, true));
        csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotal, DecimalPlaces.Two, null, true));
        AppendCsvValue(csvContent, producer.TonnageChangeCount, producer.isOverallTotalRow);
        AppendCsvValue(csvContent, producer.TonnageChangeAdvice, producer.isOverallTotalRow);
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

    private static void AppendCsvValue(StringBuilder csvContent, object? value, bool isOverallTotalRow = false,
                                       DecimalPlaces decimalPlaces = DecimalPlaces.Zero,
                                       DecimalFormats decimalFormat = DecimalFormats.F2)
    {
        if (value == null && !isOverallTotalRow)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Hyphen));
        }
        else if (value is int or decimal or double)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(value, decimalPlaces, decimalFormat));
        }
        else
        {
            csvContent.Append(CsvSanitiser.SanitiseData(value));
        }
    }

    private static void AppendProducerCommsFeesByMaterial(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool isNotTotal)
    {
        if (producer.ProducerCommsFeesByMaterial == null) { return; }
        foreach (var disposalFee in producer.ProducerCommsFeesByMaterial!)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.HouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.PublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            if (disposalFee.Key == MaterialCodes.Glass)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.HouseholdDrinksContainersTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            }
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.TotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(isNotTotal ? CsvSanitiser.SanitiseData(disposalFee.Value.PriceperTonne, DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true) : CommonConstants.CsvFileDelimiter);
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ProducerTotalCostWithoutBadDebtProvision, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.BadDebtProvision, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ProducerTotalCostwithBadDebtProvision, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.EnglandWithBadDebtProvision, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.WalesWithBadDebtProvision, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ScotlandWithBadDebtProvision, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.NorthernIrelandWithBadDebtProvision, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        }
    }

    private static RagRating GroupedRagRating(RagRating rating) => rating switch
    {
        RagRating.Red   or RagRating.RedMedical   => RagRating.Red,
        RagRating.Amber or RagRating.AmberMedical => RagRating.Amber,
        RagRating.Green or RagRating.GreenMedical => RagRating.Green,
        _ => throw new ArgumentOutOfRangeException(nameof(rating))
    };

    [SuppressMessage(
        "Critical Code Smell",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "Temporaraly suppress - will refactor later.")]
    private void AppendProducerDisposalFeesByMaterial(
        StringBuilder csvContent,
        CalcResultSummaryProducerDisposalFees producer,
        string key,
        CalcResultSummaryProducerDisposalFeesByMaterial disposalFee,
        bool applyModulation,
        bool isNotTotal)
    {
        csvContent.Append(
            !producer.isOverallTotalRow && (producer.Level != "1" || disposalFee.PreviousInvoicedTonnage == null)
                ? CsvSanitiser.SanitiseData(CommonConstants.Hyphen)
                : CsvSanitiser.SanitiseData(disposalFee.PreviousInvoicedTonnage, DecimalPlaces.Three, DecimalFormats.F3));

        foreach (var tonnage in MaterialTonnagePackages(key, disposalFee)) {
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage, DecimalPlaces.Three, DecimalFormats.F3));
        }

        if (applyModulation) {
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.TotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));

            foreach (var (_, v) in disposalFee.TotalReportedTonnageRagRating.OrderBy(x => x.Key))
            {
                csvContent.Append(CsvSanitiser.SanitiseData(v, DecimalPlaces.Three, DecimalFormats.F3));
            }

            foreach (var group in disposalFee.TotalReportedTonnageRagRating.GroupBy(x => GroupedRagRating(x.Key)).OrderBy(x => x.Key))
            {
                csvContent.Append(CsvSanitiser.SanitiseData(group.Sum(x => x.Value), DecimalPlaces.Three, DecimalFormats.F3));
            }

            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.SelfManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ActionedSelfManagedConsumerWasteTonnage.total, DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ActionedSelfManagedConsumerWasteTonnage.red, DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ActionedSelfManagedConsumerWasteTonnage.amber, DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ActionedSelfManagedConsumerWasteTonnage.green, DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetReportedTonnage.total, DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetReportedTonnage.red, DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetReportedTonnage.amber, DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetReportedTonnage.green, DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ResidualSelfManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
        } else {
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.TotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.SelfManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetReportedTonnage.total, DecimalPlaces.Three, DecimalFormats.F3));
        }

        AppendCsvValue(csvContent, disposalFee.TonnageChange, producer.isOverallTotalRow, DecimalPlaces.Three, DecimalFormats.F3);

        if (applyModulation) {
            csvContent.Append(isNotTotal ? CsvSanitiser.SanitiseData(disposalFee.PricePerTonne.red  , DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true, canBeEmpty: true) : CommonConstants.CsvFileDelimiter);
            csvContent.Append(isNotTotal ? CsvSanitiser.SanitiseData(disposalFee.PricePerTonne.amber, DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true, canBeEmpty: true) : CommonConstants.CsvFileDelimiter);
            csvContent.Append(isNotTotal ? CsvSanitiser.SanitiseData(disposalFee.PricePerTonne.green, DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true, canBeEmpty: true) : CommonConstants.CsvFileDelimiter);
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ProducerDisposalFee.red  , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ProducerDisposalFee.amber, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ProducerDisposalFee.green, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true, canBeEmpty: true));
        } else {
            csvContent.Append(isNotTotal ? CsvSanitiser.SanitiseData(disposalFee.PricePerTonne.total ?? 0, DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true) : CommonConstants.CsvFileDelimiter);
        }

        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ProducerDisposalFee.total ?? 0         , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.BadDebtProvision                       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ProducerDisposalFeeWithBadDebtProvision, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.EnglandWithBadDebtProvision            , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.WalesWithBadDebtProvision              , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ScotlandWithBadDebtProvision           , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NorthernIrelandWithBadDebtProvision    , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
    }

    private static IEnumerable<decimal> MaterialTonnagePackages(string materialCode, CalcResultSummaryProducerDisposalFeesByMaterial mb)
    {
        yield return mb.HouseholdPackagingWasteTonnage;

        foreach (var dict in mb.HouseholdPackagingWasteTonnageRagRating.OrderBy(x => x.Key))
        {
            yield return dict.Value;
        }

        yield return mb.PublicBinTonnage;

        foreach (var dict in mb.PublicBinTonnageRagRating.OrderBy(x => x.Key))
        {
            yield return dict.Value;
        }

        if (materialCode == MaterialCodes.Glass)
        {
            yield return mb.HouseholdDrinksContainersTonnage;

            foreach (var dict in mb.HouseholdDrinksContainersTonnageRagRating.OrderBy(x => x.Key))
            {
                yield return dict.Value;
            }
        }
    }
}
