namespace EPR.Calculator.Service.Common.AzureSynapse
{
    public record AzureSynapseRunnerParameters
    {
        /// <summary>
        /// Gets the calculator run ID.
        /// </summary>
        public int CalculatorRunId { get; init; }

        /// <summary>
        /// Gets the Calendar Year, in the format yyyy.
        /// </summary>
        required public CalendarYear CalendarYear { get; init; }

        /// <summary>Gets the URL of the pipeline to run.</summary>
        required public Uri PipelineUrl { get; init; }

        /// <summary>Gets the name of the pipeline to run.</summary>
        required public string PipelineName { get; init; }

        /// <summary>Gets the maximum number of times to check whether the pipeline has completed,
        /// before reporting a failure.</summary>
        public int MaxCheckCount { get; init; }

        /// <summary>Gets the time to wait before re-checking to see
        /// if the pipeline has run successfully.</summary>
        public int CheckInterval { get; init; }

    }
}
