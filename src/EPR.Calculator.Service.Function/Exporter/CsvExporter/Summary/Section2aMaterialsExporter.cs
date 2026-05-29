using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

public class Section2aMaterialsExporter : ICalcResultSummaryPartExporter
{
    public IEnumerable<CalcResultSummaryHeader> GetColumnHeaders(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
        => CalcResultSummaryUtil.Section2aMaterials(materials);

    public void AppendSectionHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        int count = GetColumnHeaders(materials, applyModulation).Count();
        csvContent.Append(CsvSanitiser.SanitiseData(CalcResultSummaryHeaders.CommsCostHeader));
        for (int i = 1; i < count; i++)
            csvContent.Append(',');
    }

    public void AppendGroupHeader(StringBuilder csvContent, CalcResultSummary resultSummary, IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        foreach (var material in materials)
        {
            int count = CalcResultSummaryUtil.Section2aMaterials([material]).Count();
            csvContent.Append(CsvSanitiser.SanitiseData($"{material.Name} Breakdown"));
            for (int i = 1; i < count; i++)
                csvContent.Append(',');
        }
    }

    public void AppendRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer, bool applyModulation)
    {
        if (producer.ProducerCommsFeesByMaterial == null) { return; }

        bool isNotTotal = producer.LeaverDate != CommonConstants.Totals;

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
}
