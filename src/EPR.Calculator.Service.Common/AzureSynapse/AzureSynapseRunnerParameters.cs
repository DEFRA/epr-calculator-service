namespace EPR.Calculator.Service.Common.AzureSynapse
{
    public record AzureSynapseRunnerParameters
    {
        /// <summary>
        /// Gets the calculator run ID.
        /// </summary>
        public int CalculatorRunId { get; init; }

        /// <summary>
        /// Gets the Financial Year.
        /// </summary>
        required public FinancialYear FinancialYear { get; init; }
    }
}
