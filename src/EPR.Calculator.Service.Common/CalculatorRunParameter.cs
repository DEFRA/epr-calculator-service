namespace EPR.Calculator.Service.Common
{
    public class CalculatorRunParameter
    {
        public int Id { get; set; }

        required public string User { get; set; }

        required public string FinancialYear { get; set; }
    }
}
