using EPR.Calculator.Service.Function.Constants;
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
                    ReportedPublicBinTonnage = item.Value.ReportedPublicBinTonnage,
                    TotalReportedTonnage = item.Value.TotalReportedTonnage,
                    PriceperTonne = item.Value.PriceperTonne,
                    ProducerTotalCostWithoutBadDebtProvision = item.Value.ProducerTotalCostWithoutBadDebtProvision,
                    BadDebtProvision = item.Value.BadDebtProvision,
                    ProducerTotalCostwithBadDebtProvision = item.Value.ProducerTotalCostwithBadDebtProvision,
                    EnglandWithBadDebtProvision = item.Value.EnglandWithBadDebtProvision,
                    WalesWithBadDebtProvision = item.Value.WalesWithBadDebtProvision,
                    ScotlandWithBadDebtProvision = item.Value.ScotlandWithBadDebtProvision,
                    NorthernIrelandWithBadDebtProvision = item.Value.NorthernIrelandWithBadDebtProvision
                };

                if (item.Key == MaterialCodes.Glass)
                {
                    breakdown.HouseholdDrinksContainers = item.Value.HouseholdDrinksContainers;
                }

                materialBreakdown.Add(breakdown);
            }

            return materialBreakdown;
        }
    }
}
