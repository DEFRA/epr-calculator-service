namespace EPR.Calculator.Service.Common.AzureSynapse
{
    /// <summary>
    /// Statuses that the pipeline might have.
    /// </summary>
    public enum PipelineStatus
    {
        // TODO: This enum needs to be updated with the actual values that the pipeline returns.

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
