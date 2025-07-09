using System.Collections.Generic;
using System.Linq;
using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

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
                    PricePerTonne = CurrencyConverter.ConvertToCurrency(item.Value.PriceperTonne, (int)DecimalPlaces.Four),
                    ProducerTotalCostWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(item.Value.ProducerTotalCostWithoutBadDebtProvision),
                    BadDebtProvision = CurrencyConverter.ConvertToCurrency(item.Value.BadDebtProvision),
                    ProducerTotalCostwithBadDebtProvision = CurrencyConverter.ConvertToCurrency(item.Value.ProducerTotalCostwithBadDebtProvision),
                    EnglandWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(item.Value.EnglandWithBadDebtProvision),
                    WalesWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(item.Value.WalesWithBadDebtProvision),
                    ScotlandWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(item.Value.ScotlandWithBadDebtProvision),
                    NorthernIrelandWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(item.Value.NorthernIrelandWithBadDebtProvision)
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
