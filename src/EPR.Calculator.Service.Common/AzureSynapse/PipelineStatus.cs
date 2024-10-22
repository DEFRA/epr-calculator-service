namespace EPR.Calculator.Service.Common.AzureSynapse
{
    /// <summary>
    /// Statuses that the pipeline can have.
    /// </summary>
    public enum PipelineStatus
    {
        /// <summary>
        /// The pipeline hasn't been started.
        /// </summary>
        NotStarted,

        /// <summary>
        /// The pipeline is currently running.
        /// </summary>
        InProgress,

        /// <summary>
        /// The pipeline completed as a success.
        /// </summary>
        Succeeded,

        /// <summary>
        /// The pipeline completed as a failure.
        /// </summary>
        Failed,
    }
}
