using System.Diagnostics.CodeAnalysis;
using System.Text;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class Section1MaterialsExporter : ICalcResultSummaryPartExporter
{
    public IEnumerable<CalcResultSummaryHeader> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
        => CalcResultSummaryUtil.Section1Materials(materials, applyModulation);

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        bool isNotTotal = producer.LeaverDate != CommonConstants.Totals;

        foreach (var (key, disposalFee) in producer.ProducerDisposalFeesByMaterial)
        {
            AppendProducerDisposalFeesByMaterial(csvContent, producer, key, disposalFee, applyModulation, isNotTotal);
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
}
