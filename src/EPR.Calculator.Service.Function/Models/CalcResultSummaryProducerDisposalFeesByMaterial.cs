
using EPR.Calculator.API.Data.Enums;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultSummaryProducerDisposalFeesByMaterial
    {
        public Dictionary<RagRating, decimal> HouseholdPackagingWasteTonnageRagRating { get; set; } = new();

        public Dictionary<RagRating, decimal> PublicBinTonnageRagRating { get; set; } = new();

        public Dictionary<RagRating, decimal> HouseholdDrinksContainersTonnageRagRating { get; set; } = new();

        public Dictionary<RagRating, decimal> TotalReportedTonnageRagRating { get; set; } = new();

        public decimal HouseholdPackagingWasteTonnage { get; set; }

        public decimal PublicBinTonnage { get; set; }

        public decimal HouseholdDrinksContainersTonnage { get; set; }

        public decimal TotalReportedTonnage { get; set; }

        public decimal BadDebtProvision { get; set; }

        public required ByCountryCost ProducerDisposalFeeWithBadDebtProvision { get; set; }

        public decimal SelfManagedConsumerWasteTonnage {
            get; set; }

        public (decimal? total, decimal? red,  decimal? amber, decimal? green) ActionedSelfManagedConsumerWasteTonnage { get; set; }

        public decimal? ResidualSelfManagedConsumerWasteTonnage { get; set; }

        public (decimal? total, decimal? red,  decimal? amber, decimal? green) NetReportedTonnage { get; set; }

        public (decimal? total, decimal? red,  decimal? amber, decimal? green) PricePerTonne { get; set; }

        public (decimal? total, decimal? red,  decimal? amber, decimal? green) ProducerDisposalFee { get; set; }

        public decimal? PreviousInvoicedTonnage { get; set; }

        public decimal? TonnageChange { get; set; }
    }
}
