namespace EPR.Calculator.Service.Function
{
    public record CalculatorParameter
    {
        public int CalculatorRunId { get; set; }

        public string FinancialYear { get; set; }

        public string CreatedBy { get; set; }
    }
}
