namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultSummaryBadDebtProvision
    {
        public decimal TotalProducerFeeWithoutBadDebtProvision { get; set; }

        public decimal BadDebtProvision { get; set; }

        public required ByCountryCost TotalProducerFeeWithBadDebtProvision { get; set; }
    }
}
