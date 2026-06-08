
namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultSummaryProducerCommsFeesCostByMaterial
    {
        public decimal HouseholdPackagingWasteTonnage { get; set; }
        public decimal PublicBinTonnage { get; set; }
        public decimal HouseholdDrinksContainersTonnage { get; set; }
        public decimal TotalReportedTonnage { get; set; }
        public decimal PriceperTonne { get; set; }

        public required CalcResultSummaryBadDebtProvision Costs { get; set; }
    }
}
