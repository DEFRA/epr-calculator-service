using System.Collections.Generic;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CalcResultCommsCostByMaterial2aJsonMapper : ICalcResultCommsCostByMaterial2aJsonMapper
    {
        public IEnumerable<CalcResultCommsCostByMaterial2aJson> Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var result = new List<CalcResultCommsCostByMaterial2aJson>();

            foreach (var item in calcResultSummaryProducerDisposalFees.ProducerCommsFeesByMaterial!)
            {
                result.Add(new CalcResultCommsCostByMaterial2aJson
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

            return result;
        }
    }
}
