using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcSummaryProducerCalculationResults
    {

        [JsonPropertyName("producerID")]
        public required string ProducerID { get; set; }

        [JsonPropertyName("subsidiaryID")]
        public required string SubsidiaryID { get; set; }

        [JsonPropertyName("producerName")]
        public required string ProducerName { get; set; }

        [JsonPropertyName("tradingName")]
        public string? TradingName { get; set; }

        [JsonPropertyName("level")]
        public int? Level { get; set; }

        [JsonPropertyName("scaledUpTonnages")]
        public required string ScaledUpTonnages { get; set; }

        [JsonPropertyName("producerDisposalFeesWithBadDebtProvision1")]
        public required ProducerDisposalFeesWithBadDebtProvision1 ProducerDisposalFeesWithBadDebtProvision1 { get; set; }

        [JsonPropertyName("disposalFeeSummary1")]
        public required DisposalFeeSummary1 DisposalFeeSummary1 { get; set; }

        [JsonPropertyName("feesForCommsCostsWithBadDebtProvision2a")]
        public required CalcResultCommsCostByMaterial2AJson FeesForCommsCostsWithBadDebtProvision2a { get; set; }

        [JsonPropertyName("commsCostsByMaterialFeesSummary2a")]
        public required CalcResultSummaryCommsCostsByMaterialFeesSummary2a? CommsCostsByMaterialFeesSummary2a { get; set; }

        [JsonPropertyName("feeForLADisposalCosts1")]
        public required CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1 FeeForLADisposalCosts1 { get; set; }

        [JsonPropertyName("feeForCommsCostsWithBadDebtProvision_2a")]
        public required CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2a FeeForCommsCostsWithBadDebtProvision_2a { get; set; }

        [JsonPropertyName("feeForCommsCostsWithBadDebtProvision_2b")]
        public required CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2b FeeForCommsCostsWithBadDebtProvision_2b { get; set; }

        [JsonPropertyName("feeForCommsCostsWithBadDebtProvision_2c")]
        public required CalcResultsCommsCostsWithBadDebtProvision2c FeeForCommsCostsWithBadDebtProvision_2c { get; set; }

        [JsonPropertyName("totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c")]
        public required TotalProducerFeeWithBadDebtProvisionFor2Con_1_2a_2b_2c TotalProducerFeeWithBadDebtProvisionFor2con_1_2a_2b_2c { get; set; }

        [JsonPropertyName("feeForSAOperatingCostsWithBadDebtProvision_3")]
        public required CalcResultSAOperatingCostsWithBadDebtProvision FeeForSAOperatingCostsWithBadDebtProvision_3 { get; set; }

        [JsonPropertyName("feeForLADataPrepCostsWithBadDebtProvision_4")]
        public required FeeForLADataPrepCostsWithBadDebtProvision_4 FeeForLADataPrepCostsWithBadDebtProvision_4 { get; set; }

        [JsonPropertyName("feeForSASetUpCostsWithBadDebtProvision_5")]
        public required FeeForSASetUpCostsWithBadDebtProvision_5 FeeForSASetUpCostsWithBadDebtProvision_5 { get; set; }

        [JsonPropertyName("totalProducerBillWithBadDebtProvision")]
        public required TotalProducerBillWithBadDebtProvision TotalProducerBillWithBadDebtProvision { get; set; }

        [JsonPropertyName("calculationOfSuggestedBillingInstructionsAndInvoiceAmounts")]
        public required CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts { get; set; }
    }
}
