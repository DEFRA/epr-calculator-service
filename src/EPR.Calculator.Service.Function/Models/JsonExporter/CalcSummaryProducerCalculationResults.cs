namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcSummaryProducerCalculationResults
    {
        public required ProducerDisposalFeesWithBadDebtProvision1 ProducerDisposalFeesWithBadDebtProvision1 { get; set; }

        public required CalcResultCommsCostByMaterial2AJson CalcResultCommsCostByMaterial2AJson { get; set; }

        public required CalcResultSummaryCommsCostsByMaterialFeesSummary2a? CommsCostsByMaterialFeesSummary2a { get; set; }

        public required CalcResultSAOperatingCostsWithBadDebtProvision CalcResultSAOperatingCostsWithBadDebtProvision { get; set; }

        public CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2a? FeeForCommsCostsWithBadDebtProvision2a { get; set; }

        public required CalcResultBillingInstructionJson CalcResultBillingInstructionJson { get; set; }

        public required TotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c TotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c { get; set; }
    }
}
