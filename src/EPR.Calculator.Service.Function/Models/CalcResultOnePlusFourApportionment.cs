namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultOnePlusFourApportionment
    {
        public ByCountryValue LaDisposalCost { get; set; }
        public CalcResultParameterOtherCostDetail LADataPrepCharge { get; set; }
        public CalcResultOnePlusFourApportionmentDetail TotalOnePlusFour { get; set; }
        public CountryApportionmentData OnePlusFourApportionment { get; set; }
    }
}
