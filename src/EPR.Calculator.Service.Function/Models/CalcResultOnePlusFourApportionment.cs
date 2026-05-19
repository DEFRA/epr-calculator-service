namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultOnePlusFourApportionment
    {
        public ByCountryCost LaDisposalCost { get; set; }
        public ByCountryCost LADataPrepCharge { get; set; }
        public ByCountryCost TotalOnePlusFour { get; set; }
        public ByCountryApportionment OnePlusFourApportionment { get; set; }
    }
}
