
namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultSummaryProducerDisposalFeesByMaterial
        : CalcResultSummaryProducerMaterialBase
    {
        public decimal SelfManagedConsumerWasteTonnage { get; set; }

        public decimal? ActionedSelfManagedConsumerWasteTonnage { get; set; }

        public decimal? ResidualSelfManagedConsumerWasteTonnage { get; set; }

        public (decimal? total, decimal? red,  decimal? amber, decimal? green) NetReportedTonnage { get; set; }

        public (decimal? total, decimal? red,  decimal? amber, decimal? green) PricePerTonne { get; set; }

        public (decimal? total, decimal? red,  decimal? amber, decimal? green) ProducerDisposalFee { get; set; }

        public decimal ProducerDisposalFeeWithBadDebtProvision { get; set; }

        public decimal? PreviousInvoicedTonnage { get; set; }

        public decimal? TonnageChange { get; set; }
    }
}