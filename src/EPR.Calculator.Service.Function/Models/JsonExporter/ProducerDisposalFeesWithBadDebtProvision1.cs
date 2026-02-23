using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Converter;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record ProducerDisposalFeesWithBadDebtProvision1
    {
        [JsonPropertyName("materialBreakdown")]
        public required IEnumerable<ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown> MaterialBreakdown { get; set; }

        public static ProducerDisposalFeesWithBadDebtProvision1 From(
            Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>? producerDisposalFeesByMaterial,
            List<MaterialDetail> materials,
            string level)
        {
            IEnumerable<ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown> GetMaterialBreakdown(
                Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>? producerDisposalFeesByMaterial,
                List<MaterialDetail> materials,
                string level)
            {
                var materialBreakdown = new List<ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown>();

                if (producerDisposalFeesByMaterial != null)
                {
                    foreach (var producerTonnage in producerDisposalFeesByMaterial)
                    {
                        var material = materials.Single(m => m.Code == producerTonnage.Key);

                        var breakdown = ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown.From(producerTonnage.Value, material.Name, level);

                        if (producerTonnage.Key == MaterialCodes.Glass)
                        {
                            breakdown.HouseholdDrinksContainersTonnageGlass =
                                producerTonnage.Value.HouseholdDrinksContainersTonnage;
                        }

                        materialBreakdown.Add(breakdown);
                    }
                }

                return materialBreakdown;
            }

            return new ProducerDisposalFeesWithBadDebtProvision1
            {
                MaterialBreakdown = GetMaterialBreakdown(producerDisposalFeesByMaterial, materials, level),
            };
        }
    }

    public record ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown
    {
        [JsonPropertyName("materialName")]
        public required string MaterialName { get; init; }

        [JsonPropertyName("previousInvoicedTonnage")]
        public required string PreviousInvoicedTonnage { get; init; }

        [JsonPropertyName("householdPackagingWasteTonnage")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public required decimal HouseholdPackagingWasteTonnage { get; init; }

        [JsonPropertyName("publicBinTonnage")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public required decimal PublicBinTonnage { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("householdDrinksContainersTonnageGlass")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public decimal? HouseholdDrinksContainersTonnageGlass { get; set; }

        [JsonPropertyName("totalTonnage")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public required decimal TotalTonnage { get; init; }

        [JsonPropertyName("selfManagedConsumerWasteTonnage")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public required decimal SelfManagedConsumerWasteTonnage { get; init; }

        [JsonPropertyName("netTonnage")]
        [JsonConverter(typeof(DecimalPrecision3Converter))]
        public required decimal NetTonnage { get; init; }

        [JsonPropertyName("tonnageChange")]
        public required string TonnageChange { get; init; }

        [JsonPropertyName("pricePerTonne")]
        public required string PricePerTonne { get; init; }

        [JsonPropertyName("producerDisposalFeeWithoutBadDebtProvision")]
        public required string ProducerDisposalFeeWithoutBadDebtProvision { get; init; }

        [JsonPropertyName("badDebtProvision")]
        public required string BadDebtProvision { get; init; }

        [JsonPropertyName("producerDisposalFeeWithBadDebtProvision")]
        public required string ProducerDisposalFeeWithBadDebtProvision { get; init; }

        [JsonPropertyName("englandWithBadDebtProvision")]
        public required string EnglandWithBadDebtProvision { get; init; }

        [JsonPropertyName("walesWithBadDebtProvision")]
        public required string WalesWithBadDebtProvision { get; init; }

        [JsonPropertyName("scotlandWithBadDebtProvision")]
        public required string ScotlandWithBadDebtProvision { get; init; }

        [JsonPropertyName("northernIrelandWithBadDebtProvision")]
        public required string NorthernIrelandWithBadDebtProvision { get; init; }

        public static ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown From(CalcResultSummaryProducerDisposalFeesByMaterial producerTonnage, string materialName, string level)
        {
            return new ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown
            {
                MaterialName = materialName,
                PreviousInvoicedTonnage = level == "1" ? (producerTonnage.PreviousInvoicedTonnage?.ToString() ?? CommonConstants.Hyphen) : CommonConstants.Hyphen,
                HouseholdPackagingWasteTonnage = producerTonnage.HouseholdPackagingWasteTonnage,
                PublicBinTonnage = producerTonnage.PublicBinTonnage,
                TotalTonnage = producerTonnage.TotalReportedTonnage,
                SelfManagedConsumerWasteTonnage = producerTonnage.ManagedConsumerWasteTonnage,
                NetTonnage = producerTonnage.NetReportedTonnage,
                TonnageChange = level == "1" ? (producerTonnage.TonnageChange?.ToString() ?? CommonConstants.Hyphen) : CommonConstants.Hyphen,
                PricePerTonne = CurrencyConverterUtils.ConvertToCurrency(producerTonnage.PricePerTonne, 4),
                ProducerDisposalFeeWithoutBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(producerTonnage.ProducerDisposalFee),
                BadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(producerTonnage.BadDebtProvision),
                ProducerDisposalFeeWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(producerTonnage.ProducerDisposalFeeWithBadDebtProvision),
                EnglandWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(producerTonnage.EnglandWithBadDebtProvision),
                WalesWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(producerTonnage.WalesWithBadDebtProvision),
                ScotlandWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(producerTonnage.ScotlandWithBadDebtProvision),
                NorthernIrelandWithBadDebtProvision = CurrencyConverterUtils.ConvertToCurrency(producerTonnage.NorthernIrelandWithBadDebtProvision),
            };
        }
    }

}
