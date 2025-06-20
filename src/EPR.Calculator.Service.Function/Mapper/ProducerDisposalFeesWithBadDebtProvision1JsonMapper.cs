namespace EPR.Calculator.Service.Function.Mapper
{
    using System.Collections.Generic;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Models.JsonExporter;

    public class ProducerDisposalFeesWithBadDebtProvision1JsonMapper : IProducerDisposalFeesWithBadDebtProvision1JsonMapper
    {
        public ProducerDisposalFeesWithBadDebtProvision1 Map(Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> producerDisposalFeesByMaterial)
        {
            return new ProducerDisposalFeesWithBadDebtProvision1
            {
                MaterialBreakdown = GetMaterialBreakdown(producerDisposalFeesByMaterial),
            };
        }

        private static IEnumerable<ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown> GetMaterialBreakdown(Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> producerDisposalFeesByMaterial)
        {
            var materialBreakdown = new List<ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown>();

            foreach (var producerTonnage in producerDisposalFeesByMaterial)
            {
                var breakdown = new ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown
                {
                    MaterialName = producerTonnage.Key,
                    PreviousInvoicedTonnage = producerTonnage.Value.PreviousInvoicedTonnage,
                    HouseholdPackagingWasteTonnage = producerTonnage.Value.HouseholdPackagingWasteTonnage,
                    PublicBinTonnage = producerTonnage.Value.PublicBinTonnage,
                    TotalTonnage = producerTonnage.Value.TotalReportedTonnage,
                    SelfManagedConsumerWasteTonnage = producerTonnage.Value.ManagedConsumerWasteTonnage,
                    NetTonnage = producerTonnage.Value.NetReportedTonnage,
                    TonnageChange = producerTonnage.Value.TonnageChange,
                    PricePerTonne = producerTonnage.Value.PricePerTonne,
                    ProducerDisposalFeeWithoutBadDebtProvision = producerTonnage.Value.ProducerDisposalFee,
                    BadDebtProvision = producerTonnage.Value.BadDebtProvision,
                    ProducerDisposalFeeWithBadDebtProvision = producerTonnage.Value.ProducerDisposalFeeWithBadDebtProvision,
                    EnglandWithBadDebtProvision = producerTonnage.Value.EnglandWithBadDebtProvision,
                    WalesWithBadDebtProvision = producerTonnage.Value.WalesWithBadDebtProvision,
                    ScotlandWithBadDebtProvision = producerTonnage.Value.ScotlandWithBadDebtProvision,
                    NorthernIrelandWithBadDebtProvision = producerTonnage.Value.NorthernIrelandWithBadDebtProvision,
                };

                if (producerTonnage.Key == MaterialCodes.Glass)
                {
                    breakdown.HouseholdDrinksContainersTonnageGlass = producerTonnage.Value.HouseholdDrinksContainersTonnage;
                }
                materialBreakdown.Add(breakdown);
            }

            return materialBreakdown;
        }
    }
}