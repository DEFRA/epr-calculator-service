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

        /// <summary>Gets the URL of the pipeline to run.</summary>
        required public Uri PipelineUrl { get; init; }

        /// <summary>Gets the name of the pipeline to run.</summary>
        required public string PipelineName { get; init; }

        /// <summary>Gets the maximum number of times to check whether the pipeline has completed,
        /// before reporting a failure.</summary>
        public int MaxChecks { get; init; }

        /// <summary>Gets the time to wait before re-checking to see
        /// if the pipeline has run successfully.</summary>
        public int CheckInterval { get; init; }

        /// <summary>
        /// Gets the URL of the endpoint that's used to access the database and update the status of the run.
        /// </summary>
        required public Uri StatusUpdateEndpoint { get; init; }
    }
}
