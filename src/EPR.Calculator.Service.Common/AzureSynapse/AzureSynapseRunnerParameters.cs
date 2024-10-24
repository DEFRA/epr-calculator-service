namespace EPR.Calculator.Service.Common.AzureSynapse
{
    public record AzureSynapseRunnerParameters
    {
        public int CalculatorRunId { get; init; }

        public FinancialYear FinancialYear { get; init; }
    }
}
