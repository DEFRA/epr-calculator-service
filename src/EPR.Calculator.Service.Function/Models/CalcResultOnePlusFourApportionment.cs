namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultOnePlusFourApportionment
    {
        public CalcResultLapcapDataDetail LaDisposalCost { get; set; }
        public CalcResultParameterOtherCostDetail LADataPrepCharge { get; set; }
        public CalcResultOnePlusFourApportionmentDetail TotalOnePlusFour { get; set; }
        // TODO reuse CountryApportionmentData
        public CalcResultOnePlusFourApportionmentDetail OnePlusFourApportionment { get; set; }
    }

}
