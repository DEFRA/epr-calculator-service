using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System;
using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Mappers
{
    public static class ProducerDisposalFeesWithBadDebtProvision1JsonMapper
    {
        public static ProducerDisposalFeesWithBadDebtProvision1 Map(Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> ProducerDisposalFeesByMaterial)
        {
            return new ProducerDisposalFeesWithBadDebtProvision1
            {
                MaterialBreakdown = GetMaterialBreakdown(ProducerDisposalFeesByMaterial)

            };
        }

        private static IEnumerable<ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown> GetMaterialBreakdown(Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> ProducerDisposalFeesByMaterial)
        {
            var materialBreakdown = new List<ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown>();

            foreach (var producerTonnage in ProducerDisposalFeesByMaterial)
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
                    NorthernIrelandWithBadDebtProvision = producerTonnage.Value.NorthernIrelandWithBadDebtProvision
                };
                materialBreakdown.Add(breakdown);
            }

            return materialBreakdown;
        }
    }
}