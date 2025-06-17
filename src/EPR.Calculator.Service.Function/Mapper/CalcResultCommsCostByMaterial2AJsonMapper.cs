using System.Collections.Generic;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CalcResultCommsCostByMaterial2AJsonMapper : ICalcResultCommsCostByMaterial2AJsonMapper
    {
        public CalcResultCommsCostByMaterial2AJson Map(
            Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostByMaterial)
        {
            return new CalcResultCommsCostByMaterial2AJson
            {
                MaterialBreakdown = GetMaterialBreakdown(commsCostByMaterial)
            };
        }

        public IEnumerable<CalcResultCommsCostByMaterial2AMaterialBreakdown> GetMaterialBreakdown(
            Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostByMaterial)
        {
            var materialBreakdown = new List<CalcResultCommsCostByMaterial2AMaterialBreakdown>();

            foreach (var item in commsCostByMaterial)
            {
                materialBreakdown.Add(new CalcResultCommsCostByMaterial2AMaterialBreakdown
                {
                    MaterialName = item.Key,
                    HouseholdPackagingWasteTonnage = item.Value.HouseholdPackagingWasteTonnage,
                    ReportedPublicBinTonnage = item.Value.ReportedPublicBinTonnage,
                    TotalReportedTonnage = item.Value.TotalReportedTonnage,
                    HouseholdDrinksContainers = item.Value.HouseholdDrinksContainers,
                    PriceperTonne = item.Value.PriceperTonne,
                    ProducerTotalCostWithoutBadDebtProvision = item.Value.ProducerTotalCostWithoutBadDebtProvision,
                    BadDebtProvision = item.Value.BadDebtProvision,
                    ProducerTotalCostwithBadDebtProvision = item.Value.ProducerTotalCostwithBadDebtProvision,
                    EnglandWithBadDebtProvision = item.Value.EnglandWithBadDebtProvision,
                    WalesWithBadDebtProvision = item.Value.WalesWithBadDebtProvision,
                    ScotlandWithBadDebtProvision = item.Value.ScotlandWithBadDebtProvision,
                    NorthernIrelandWithBadDebtProvision = item.Value.NorthernIrelandWithBadDebtProvision
                });
            }

            return materialBreakdown;
        }
    }
}
