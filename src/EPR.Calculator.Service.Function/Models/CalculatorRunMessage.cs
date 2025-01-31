namespace EPR.Calculator.Service.Function.Models
{
    public class CalculatorRunMessage
    {
        public required int CalculatorRunId { get; set; }

        public required string FinancialYear { get; set; }

        public required string CreatedBy { get; set; }
    }
}