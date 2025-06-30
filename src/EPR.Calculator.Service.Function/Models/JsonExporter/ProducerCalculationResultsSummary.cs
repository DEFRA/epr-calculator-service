using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class ProducerCalculationResultsSummary
    {
        [JsonProperty(PropertyName = "feeForLaDisposalCostsWithoutBadDebtprovision1")]
        public required string FeeForLaDisposalCostsWithoutBadDebtprovision1 { get; init; }
        
        [JsonProperty(PropertyName = "badDebtProvision1")]
        public required string BadDebtProvision1 { get; init; }

        [JsonProperty(PropertyName = "feeForLaDisposalCostsWithBadDebtprovision1")]
        public required string FeeForLaDisposalCostsWithBadDebtprovision1 { get; init; }

        [JsonProperty(PropertyName = "feeForCommsCostsByMaterialWithoutBadDebtprovision2a")]
        public required string FeeForCommsCostsByMaterialWithoutBadDebtprovision2a { get; init; }

        [JsonProperty(PropertyName = "badDebtProvision2a")]
        public required string BadDebtProvision2a { get; init; }

        [JsonProperty(PropertyName = "feeForCommsCostsByMaterialWitBadDebtprovision2a")]
        public required string FeeForCommsCostsByMaterialWitBadDebtprovision2a { get; init; }

        [JsonProperty(PropertyName = "feeForCommsCostsUkWideWithoutBadDebtprovision2b")]
        public required string FeeForCommsCostsUkWideWithoutBadDebtprovision2b { get; init; }

        [JsonProperty(PropertyName = "badDebtProvision2b")]
        public required string BadDebtProvision2b { get; init; }

        [JsonProperty(PropertyName = "feeForCommsCostsUkWideWithBadDebtprovision2b")]
        public required string FeeForCommsCostsUkWideWithBadDebtprovision2b { get; init; }

        [JsonProperty(PropertyName = "feeForCommsCostsByCountryWithoutBadDebtprovision2c")]
        public required string FeeForCommsCostsByCountryWithoutBadDebtprovision2c { get; init; }

        [JsonProperty(PropertyName = "badDebtProvision2c")]
        public required string BadDebtProvision2c { get; init; }

        [JsonProperty(PropertyName = "feeForCommsCostsByCountryWideWithBadDebtprovision2c")]
        public required string FeeForCommsCostsByCountryWideWithBadDebtprovision2c { get; init; }

        [JsonProperty(PropertyName = "total12a2b2cWithBadDebt")]
        public required string Total12a2b2cWithBadDebt { get; init; }

        [JsonProperty(PropertyName = "saOperatingCostsWithoutBadDebtProvision3")]
        public required string SaOperatingCostsWithoutBadDebtProvision3 { get; init; }

        [JsonProperty(PropertyName = "badDebtProvision3")]
        public required string BadDebtProvision3 { get; init; }

        [JsonProperty(PropertyName = "saOperatingCostsWithBadDebtProvision3")]
        public required string SaOperatingCostsWithBadDebtProvision3 { get; init; }

        [JsonProperty(PropertyName = "laDataPrepCostsWithoutBadDebtProvision4")]
        public required string LaDataPrepCostsWithoutBadDebtProvision4 { get; init; }

        [JsonProperty(PropertyName = "badDebtProvision4")]
        public required string BadDebtProvision4 { get; init; }

        [JsonProperty(PropertyName = "laDataPrepCostsWithbadDebtProvision4")]
        public required string LaDataPrepCostsWithbadDebtProvision4 { get; init; }

        [JsonProperty(PropertyName = "oneOffFeeSaSetuCostsWithbadDebtProvision5")]
        public required string OneOffFeeSaSetuCostsWithbadDebtProvision5 { get; init; }

        [JsonProperty(PropertyName = "badDebtProvision5")]
        public required string BadDebtProvision5 { get; init; }

        [JsonProperty(PropertyName = "oneOffFeeSaSetuCostsWithoutbadDebtProvision5")]
        public required string OneOffFeeSaSetuCostsWithoutbadDebtProvision5 { get; init; }
    }
}
