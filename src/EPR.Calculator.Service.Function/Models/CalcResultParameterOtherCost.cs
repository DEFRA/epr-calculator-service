namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultParameterOtherCost
    {
        public ByCountryCost SaOperatingCost { get; set; } = new ByCountryCost { England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0 };
        public ByCountryCost LaDataPrepCharge { get; set; } = new ByCountryCost { England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0 };
        public ByCountryApportionment CountryApportionment { get; set; } = new ByCountryApportionment { England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0 }; // TODO default is inconsistent - i.e. doesn't add to 100%
        public ByCountryCost SchemeSetupCost { get; set; } = new ByCountryCost { England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0 };
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
