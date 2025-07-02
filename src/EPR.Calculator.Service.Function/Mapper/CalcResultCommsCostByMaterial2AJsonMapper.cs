using EPR.Calculator.API.Utils;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System.Collections.Generic;
using System.Linq;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CalcResultCommsCostByMaterial2AJsonMapper : ICalcResultCommsCostByMaterial2AJsonMapper
    {
        public CalcResultCommsCostByMaterial2AJson Map(
            Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostByMaterial,
            List<MaterialDetail> materials)
        {
            return new CalcResultCommsCostByMaterial2AJson
            {
                MaterialBreakdown = GetMaterialBreakdown(commsCostByMaterial, materials)
            };
        }

        public IEnumerable<CalcResultCommsCostByMaterial2AMaterialBreakdown> GetMaterialBreakdown(
            Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostByMaterial,
            List<MaterialDetail> materials)
        {
            var materialBreakdown = new List<CalcResultCommsCostByMaterial2AMaterialBreakdown>();

            foreach (var item in commsCostByMaterial)
            {
                var material = materials.Single(m => m.Code == item.Key);

                var breakdown = new CalcResultCommsCostByMaterial2AMaterialBreakdown
                {
                    MaterialName = material.Name,
                    HouseholdPackagingWasteTonnage = item.Value.HouseholdPackagingWasteTonnage,
                    PublicBinTonnage = item.Value.ReportedPublicBinTonnage,
                    TotalTonnage = item.Value.TotalReportedTonnage,
                    PricePerTonne = CsvSanitiser.SanitiseData(item.Value.PriceperTonne, DecimalPlaces.Four, null, true, false, false),
                    ProducerTotalCostWithoutBadDebtProvision = CsvSanitiser.SanitiseData(item.Value.ProducerTotalCostWithoutBadDebtProvision, DecimalPlaces.Two, null, true, false, false),
                    BadDebtProvision = CsvSanitiser.SanitiseData(item.Value.BadDebtProvision, DecimalPlaces.Two, null, true, false, false),
                    ProducerTotalCostwithBadDebtProvision = CsvSanitiser.SanitiseData(item.Value.ProducerTotalCostwithBadDebtProvision, DecimalPlaces.Two, null, true, false, false),
                    EnglandWithBadDebtProvision = CsvSanitiser.SanitiseData(item.Value.EnglandWithBadDebtProvision, DecimalPlaces.Two, null, true, false, false),
                    WalesWithBadDebtProvision = CsvSanitiser.SanitiseData(item.Value.WalesWithBadDebtProvision, DecimalPlaces.Two, null, true, false, false),
                    ScotlandWithBadDebtProvision = CsvSanitiser.SanitiseData(item.Value.ScotlandWithBadDebtProvision, DecimalPlaces.Two, null, true, false, false),
                    NorthernIrelandWithBadDebtProvision = CsvSanitiser.SanitiseData(item.Value.NorthernIrelandWithBadDebtProvision, DecimalPlaces.Two, null, true, false, false),
                };

                if (item.Key == MaterialCodes.Glass)
                {
                    breakdown.HouseholdDrinksContainersTonnageGlass = item.Value.HouseholdDrinksContainers;
                }

                materialBreakdown.Add(breakdown);
            }

            return materialBreakdown;
        }
    }
}
