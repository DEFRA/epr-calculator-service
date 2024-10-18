namespace EPR.Calculator.Service.Common.AzureSynapse
{
    public record PipelineParameters
    {
        /// <summary>
        /// Gets the calculation ID.
        /// </summary>
        required public string CalculationId { get; init; }

        /// <summary>
        /// Gets the financial year.
        /// </summary>
        required public string FinancialYear { get; init; }

        /// <summary>
        /// Gets the User ID.
        /// </summary>
        required public string UserId { get; init; }
    }
}
