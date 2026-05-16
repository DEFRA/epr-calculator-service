namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultParameterOtherCost
    {
        public CalcResultParameterOtherCostDetail SaOperatingCost { get; set; } = new CalcResultParameterOtherCostDetail { England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0, Total = 0 };
        public CalcResultParameterOtherCostDetail LaDataPrepCharge { get; set; } = new CalcResultParameterOtherCostDetail { England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0, Total = 0 };
        public CalcResultParameterOtherCostDetail CountryApportionment { get; set; } = new CalcResultParameterOtherCostDetail { England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0, Total = 0 };
        public CalcResultParameterOtherCostDetail SchemeSetupCost { get; set; } = new CalcResultParameterOtherCostDetail
        {
            England = 0m,
            Wales = 0m,
            Scotland = 0m,
            NorthernIreland = 0m,
            Total = 0m
        };
        public Materiality MaterialityIncrease { get; set; } = new Materiality { Amount = 0, Percentage = 0 };
        public Materiality MaterialityDecrease { get; set; } = new Materiality { Amount = 0, Percentage = 0 };
        public Materiality TonnageChangeIncrease { get; set; } = new Materiality { Amount = 0, Percentage = 0 };
        public Materiality TonnageChangeDecrease { get; set; } = new Materiality { Amount = 0, Percentage = 0 };
        public decimal BadDebtValue { get; set; }
    }

    public class Materiality
    {
        public decimal Amount  { get; set; }
        public decimal Percentage { get; set; }
    }
}
