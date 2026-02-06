namespace EPR.Calculator.Service.Common.AzureSynapse
{
    public record AzureSynapseRunnerParameters
    {
        /// <summary>
        /// Gets the calculator run ID.
        /// </summary>
        public int CalculatorRunId { get; init; }

        /// <summary>
        /// Stores the relative year as a primitive integer for infrastructure purposes
        /// (e.g., database mapping or serialization). This value represents the year in YYYY format.
        /// </summary>
        required public int RelativeYearValue { get; init; }

        /// <summary>
        /// Converts the stored <see cref="RelativeYearValue"/> into a <see cref="RelativeYear"/> value object
        /// for use in domain logic. This allows code to work with the type-safe wrapper while keeping
        /// storage infrastructure simple.
        /// </summary>
        public RelativeYear RelativeYear() => new RelativeYear(this.RelativeYearValue);

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
