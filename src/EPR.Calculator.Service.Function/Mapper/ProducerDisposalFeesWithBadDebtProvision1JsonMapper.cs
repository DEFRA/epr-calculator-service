namespace EPR.Calculator.Service.Function.Mapper
{
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Models.JsonExporter;
    using System.Collections.Generic;
    using System.Linq;

    public class ProducerDisposalFeesWithBadDebtProvision1JsonMapper : IProducerDisposalFeesWithBadDebtProvision1JsonMapper
    {
        public ProducerDisposalFeesWithBadDebtProvision1 Map(
            Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> producerDisposalFeesByMaterial,
            List<MaterialDetail> materials)
        {
            return new ProducerDisposalFeesWithBadDebtProvision1
            {
                MaterialBreakdown = GetMaterialBreakdown(producerDisposalFeesByMaterial, materials),
            };
        }

        private static IEnumerable<ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown> GetMaterialBreakdown(
            Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> producerDisposalFeesByMaterial,
            List<MaterialDetail> materials)
        {
            var materialBreakdown = new List<ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown>();

            foreach (var producerTonnage in producerDisposalFeesByMaterial)
            {
                var material = materials.Single(m => m.Code == producerTonnage.Key);

                var breakdown = new ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown
                {
                    MaterialName = material.Name,
                    PreviousInvoicedTonnage = producerTonnage.Value.PreviousInvoicedTonnage,
                    HouseholdPackagingWasteTonnage = producerTonnage.Value.HouseholdPackagingWasteTonnage,
                    PublicBinTonnage = producerTonnage.Value.PublicBinTonnage,
                    TotalTonnage = producerTonnage.Value.TotalReportedTonnage,
                    SelfManagedConsumerWasteTonnage = producerTonnage.Value.ManagedConsumerWasteTonnage,
                    NetTonnage = producerTonnage.Value.NetReportedTonnage,
                    TonnageChange = producerTonnage.Value.TonnageChange,
                    PricePerTonne = CurrencyConverter.ConvertToCurrency(producerTonnage.Value.PricePerTonne),
                    ProducerDisposalFeeWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(producerTonnage.Value.ProducerDisposalFee),
                    BadDebtProvision = CurrencyConverter.ConvertToCurrency(producerTonnage.Value.BadDebtProvision),
                    ProducerDisposalFeeWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(producerTonnage.Value.ProducerDisposalFeeWithBadDebtProvision),
                    EnglandWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(producerTonnage.Value.EnglandWithBadDebtProvision),
                    WalesWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(producerTonnage.Value.WalesWithBadDebtProvision),
                    ScotlandWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(producerTonnage.Value.ScotlandWithBadDebtProvision),
                    NorthernIrelandWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(producerTonnage.Value.NorthernIrelandWithBadDebtProvision),
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