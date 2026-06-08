namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultSummaryBadDebtProvision
    {
        public decimal TotalProducerFeeWithoutBadDebtProvision { get; set; }

        public decimal BadDebtProvision { get; set; }

        //public decimal TotalProducerFeeWithBadDebtProvision { get; set; }

        public ByCountryCost TotalProducerFeeWithBadDebtProvision { get; set; }
    }
}
