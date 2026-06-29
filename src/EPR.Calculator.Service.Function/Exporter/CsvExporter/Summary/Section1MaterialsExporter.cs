using System.Diagnostics.CodeAnalysis;
using System.Text;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class Section1MaterialsExporter : ICalcResultSummaryPartExporter
{
    public IEnumerable<string> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        return materials.SelectMany(material =>
        {
            return Section1MaterialsHeaders(material, applyModulation);
        });
    }

    private static IEnumerable<string> Section1MaterialsHeaders(MaterialDetail material, bool applyModulation)
    {
        var headers = new List<string>();
        headers.Add("Previous Invoiced Tonnage");

        headers.Add("Household Packaging Tonnage");
        if (applyModulation)
        {
            headers.AddRange([
                "Household Red Material Tonnage",
                "Household Amber Material Tonnage",
                "Household Green Material Tonnage",
                "Household Red Medical Material Tonnage",
                "Household Amber Medical Material Tonnage",
                "Household Green Medical Material Tonnage"
            ]);
        }

        headers.Add("Public Bin Tonnage");
        if (applyModulation)
        {
            headers.AddRange([
                "Public Bin Red Material Tonnage",
                "Public Bin Amber Material Tonnage",
                "Public Bin Green Material Tonnage",
                "Public Bin Red Medical Material Tonnage",
                "Public Bin Amber Medical Material Tonnage",
                "Public Bin Green Medical Material Tonnage"
            ]);
        }

        if (material.Code == MaterialCodes.Glass)
        {
            headers.Add("Household Drinks Containers Tonnage - Glass");
            if (applyModulation)
            {
                headers.AddRange([
                    "Household Drinks Containers Red Material Tonnage",
                    "Household Drinks Containers Amber Material Tonnage",
                    "Household Drinks Containers Green Material Tonnage",
                    "Household Drinks Containers Red Medical Material Tonnage",
                    "Household Drinks Containers Amber Medical Material Tonnage",
                    "Household Drinks Containers Green Medical Material Tonnage"
                ]);
            }
        }

        if (applyModulation) {
            headers.AddRange([
                "Total Tonnage",
                "Red Total Tonnage",
                "Amber Total Tonnage",
                "Green Total Tonnage",
                "Red Medical Total Tonnage",
                "Amber Medical Total Tonnage",
                "Green Medical Total Tonnage",
                "Red + Red Medical Total Tonnage",
                "Amber + Amber Medical Total Tonnage",
                "Green + Green Medical Total Tonnage",
                "Self Managed Consumer Waste Tonnage",
                "Actioned Self Managed Consumer Waste Tonnage",
                "Red + Red Medical Actioned Self Managed Consumer Waste Tonnage",
                "Amber + Amber Medical Actioned Self Managed Consumer Waste Tonnage",
                "Green + Green Medical Actioned Self Managed Consumer Waste Tonnage",
                "Net Tonnage",
                "Red + Red Medical Net Tonnage",
                "Amber + Amber Medical Net Tonnage",
                "Green + Green Medical Net Tonnage",
                "Residual SMCW"
            ]);
        } else {
            headers.AddRange([
                "Total Tonnage",
                "Self Managed Consumer Waste Tonnage",
                "Net Tonnage"
            ]);
        }

        headers.Add("Tonnage Change");
        if (applyModulation) {
            headers.AddRange([
                "Red + Red Medical Material Price per Tonne",
                "Amber + Amber Medical Material Price per Tonne",
                "Green + Green Medical Material Price per Tonne",
                "Producer Red + Red Medical Material Disposal Cost",
                "Producer Amber + Amber Medical Material Disposal Cost",
                "Producer Green + Green Medical Material Disposal Cost"
            ]);
        } else {
            headers.Add("Price per Tonne");
        }

        headers.AddRange([
            "Producer Disposal Fee w/o Bad Debt Provision",
            "Bad Debt Provision",
            "Producer Disposal Fee with Bad Debt Provision",
            "England with Bad Debt Provision",
            "Wales with Bad Debt Provision",
            "Scotland with Bad Debt Provision",
            "Northern Ireland with Bad Debt Provision"
        ]);

        return headers;
    }

    public void AppendSectionHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData("1 Producer Disposal Fees with Bad Debt Provision"));
        csvContent.Append(',', count - 1);
    }

    public void AppendGroupHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        foreach (var material in materials)
        {
            int count = Section1MaterialsHeaders(material, applyModulation).Count();
            csvContent.Append(CsvSanitiser.SanitiseData($"{material.Name} Breakdown"));
            csvContent.Append(',', count - 1);
        }
    }

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        bool isNotTotal = !producer.IsOverallTotal;

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
            !producer.IsOverallTotal && (producer.Level != "1" || disposalFee.PreviousInvoicedTonnage == null)
                ? CsvSanitiser.SanitiseData(CommonConstants.Hyphen)
                : CsvSanitiser.SanitiseData(disposalFee.PreviousInvoicedTonnage, DecimalPlaces.Three, DecimalFormats.F3));

        foreach (var tonnage in MaterialTonnagePackages(key, disposalFee, applyModulation)) {
            csvContent.Append(CsvSanitiser.SanitiseData(tonnage, DecimalPlaces.Three, DecimalFormats.F3));
        }

        if (applyModulation) {
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.TotalTonnage, DecimalPlaces.Three, DecimalFormats.F3));

            var totalRed = disposalFee.TotalRAMTonnage!.Red;
            var totalAmber = disposalFee.TotalRAMTonnage!.Amber;
            var totalGreen = disposalFee.TotalRAMTonnage!.Green;
            var totalRedMedical = disposalFee.TotalRAMTonnage!.RedMedical;
            var totalAmberMedical = disposalFee.TotalRAMTonnage!.AmberMedical;
            var totalGreenMedical = disposalFee.TotalRAMTonnage!.GreenMedical;
            csvContent.Append(CsvSanitiser.SanitiseData(totalRed, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(totalAmber, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(totalGreen, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(totalRedMedical, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(totalAmberMedical, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(totalGreenMedical, DecimalPlaces.Three, DecimalFormats.F3));

            csvContent.Append(CsvSanitiser.SanitiseData(totalRed + totalRedMedical, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(totalAmber + totalAmberMedical, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(totalGreen + totalGreenMedical, DecimalPlaces.Three, DecimalFormats.F3));

            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.SelfManagedConsumerWasteTonnage              , DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ActionedSelfManagedConsumerWasteTonnage.Total, DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ActionedSelfManagedConsumerWasteTonnage.Red  , DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ActionedSelfManagedConsumerWasteTonnage.Amber, DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ActionedSelfManagedConsumerWasteTonnage.Green, DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetReportedTonnage.Total                     , DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetReportedTonnage.Red                       , DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetReportedTonnage.Amber                     , DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetReportedTonnage.Green                     , DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ResidualSelfManagedConsumerWasteTonnage      , DecimalPlaces.Three, DecimalFormats.F3, canBeEmpty: true));
        } else {
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.TotalTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.SelfManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.NetReportedTonnage.Total, DecimalPlaces.Three, DecimalFormats.F3));
        }

        AppendCsvValue(csvContent, disposalFee.TonnageChange, producer.IsOverallTotal, DecimalPlaces.Three, DecimalFormats.F3);

        if (applyModulation) {
            csvContent.Append(isNotTotal ? CsvSanitiser.SanitiseData(disposalFee.PricePerTonne.Red  , DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true, canBeEmpty: true) : CommonConstants.CsvFileDelimiter);
            csvContent.Append(isNotTotal ? CsvSanitiser.SanitiseData(disposalFee.PricePerTonne.Amber, DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true, canBeEmpty: true) : CommonConstants.CsvFileDelimiter);
            csvContent.Append(isNotTotal ? CsvSanitiser.SanitiseData(disposalFee.PricePerTonne.Green, DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true, canBeEmpty: true) : CommonConstants.CsvFileDelimiter);
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ProducerDisposalFee.Red  , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ProducerDisposalFee.Amber, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true, canBeEmpty: true));
            csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ProducerDisposalFee.Green, DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true, canBeEmpty: true));
        } else {
            csvContent.Append(isNotTotal ? CsvSanitiser.SanitiseData(disposalFee.PricePerTonne.Total ?? 0, DecimalPlaces.Four, DecimalFormats.F4, isCurrency: true) : CommonConstants.CsvFileDelimiter);
        }

        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ProducerDisposalFee.Total ?? 0         , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.BadDebtProvision                       , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ProducerDisposalFeeWithBadDebtProvision.Total              , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ProducerDisposalFeeWithBadDebtProvision.England            , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ProducerDisposalFeeWithBadDebtProvision.Wales              , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ProducerDisposalFeeWithBadDebtProvision.Scotland           , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
        csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.ProducerDisposalFeeWithBadDebtProvision.NorthernIreland    , DecimalPlaces.Two, DecimalFormats.F2, isCurrency: true));
    }

    private static IEnumerable<decimal> MaterialTonnagePackages(string materialCode, CalcResultSummaryProducerDisposalFeesByMaterial mb, bool applyModulation)
    {
        yield return mb.HouseholdTonnage;

        if (applyModulation)
        {
            yield return mb.HouseholdRAMTonnage?.Red ?? 0m ;
            yield return mb.HouseholdRAMTonnage?.Amber ?? 0m ;
            yield return mb.HouseholdRAMTonnage?.Green ?? 0m ;
            yield return mb.HouseholdRAMTonnage?.RedMedical ?? 0m ;
            yield return mb.HouseholdRAMTonnage?.AmberMedical ?? 0m ;
            yield return mb.HouseholdRAMTonnage?.GreenMedical ?? 0m ;
        }

        yield return mb.PublicBinTonnage;

        if (applyModulation)
        {
            yield return mb.PublicBinRAMTonnage?.Red ?? 0m ;
            yield return mb.PublicBinRAMTonnage?.Amber ?? 0m ;
            yield return mb.PublicBinRAMTonnage?.Green ?? 0m ;
            yield return mb.PublicBinRAMTonnage?.RedMedical ?? 0m ;
            yield return mb.PublicBinRAMTonnage?.AmberMedical ?? 0m ;
            yield return mb.PublicBinRAMTonnage?.GreenMedical ?? 0m ;
        }

        if (materialCode == MaterialCodes.Glass)
        {
            yield return mb.HDCTonnage;

            if (applyModulation)
            {
                yield return mb.HDCRAMTonnage?.Red ?? 0m ;
                yield return mb.HDCRAMTonnage?.Amber ?? 0m ;
                yield return mb.HDCRAMTonnage?.Green ?? 0m ;
                yield return mb.HDCRAMTonnage?.RedMedical ?? 0m ;
                yield return mb.HDCRAMTonnage?.AmberMedical ?? 0m ;
                yield return mb.HDCRAMTonnage?.GreenMedical ?? 0m ;
            }
        }
    }

    private static void AppendCsvValue(
        StringBuilder csvContent,
        object? value,
        bool isOverallTotalRow = false,
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
