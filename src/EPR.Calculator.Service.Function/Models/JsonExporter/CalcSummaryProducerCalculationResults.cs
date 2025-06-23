using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcSummaryProducerCalculationResults
    {

        [JsonProperty("producerId")]
        public required string ProducerId { get; set; }

        [JsonProperty("subsidiaryId")]
        public required string SubsidiaryId { get; set; }

        [JsonProperty("producerName")]
        public required string ProducerName { get; set; }

        [JsonProperty("tradingName")]
        public string? TradingName { get; set; }

        [JsonProperty("level")]
        public string? Level { get; set; }

        [JsonProperty("scaledUpTonnages")]
        public required string ScaledUpTonnages { get; set; }

        [JsonProperty("producerDisposalFeesWithBadDebtProvision1")]
        public required ProducerDisposalFeesWithBadDebtProvision1 ProducerDisposalFeesWithBadDebtProvision1 { get; set; }

        [JsonProperty("disposalFeeSummary1")]
        public required DisposalFeeSummary1 DisposalFeeSummary1 { get; set; }

        [JsonProperty("feesForCommsCostsWithBadDebtProvision2a")]
        public required CalcResultCommsCostByMaterial2AJson FeesForCommsCostsWithBadDebtProvision2a { get; set; }

        [JsonProperty("commsCostsByMaterialFeesSummary2a")]
        public required CalcResultSummaryCommsCostsByMaterialFeesSummary2a? CommsCostsByMaterialFeesSummary2a { get; set; }

        [JsonProperty("feeForLADisposalCosts1")]
        public required CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1 FeeForLADisposalCosts1 { get; set; }

        [JsonProperty("feeForCommsCostsWithBadDebtProvision_2a")]
        public required CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2a FeeForCommsCostsWithBadDebtProvision_2a { get; set; }

        [JsonProperty("feeForCommsCostsWithBadDebtProvision_2b")]
        public required CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2b FeeForCommsCostsWithBadDebtProvision_2b { get; set; }

        [JsonProperty("feeForCommsCostsWithBadDebtProvision_2c")]
        public required CalcResultsCommsCostsWithBadDebtProvision2c FeeForCommsCostsWithBadDebtProvision_2c { get; set; }

        [JsonProperty("totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c")]
        public required TotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c TotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c { get; set; }

        [JsonProperty("feeForSAOperatingCostsWithBadDebtProvision_3")]
        public required CalcResultSAOperatingCostsWithBadDebtProvision FeeForSAOperatingCostsWithBadDebtProvision_3 { get; set; }

        [JsonProperty("feeForSASetUpCostsWithBadDebtProvision_5")]
        public required FeeForSASetUpCostsWithBadDebtProvision_5 FeeForSASetUpCostsWithBadDebtProvision_5 { get; set; }

        [JsonProperty("totalProducerBillWithBadDebtProvision")]
        public required TotalProducerBillWithBadDebtProvision TotalProducerBillWithBadDebtProvision { get; set; }

        [JsonProperty("calculationOfSuggestedBillingInstructionsAndInvoiceAmounts")]
        public required CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts { get; set; }
    }
}
