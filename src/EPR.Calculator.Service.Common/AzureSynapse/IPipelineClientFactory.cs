namespace EPR.Calculator.Service.Common.AzureSynapse
{
    using Azure.Analytics.Synapse.Artifacts;
    using Azure.Core;

    /// <summary>
    /// Defines interface methods to create clients for interacting with Azure Synapse pipelines.
    /// </summary>
    public interface IPipelineClientFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="PipelineClient"/> for the specified pipeline URL and token credential.
        /// </summary>
        /// <param name="pipelineUrl">The URL of the pipeline.</param>
        /// <param name="tokenCredential">The token credential for authentication.</param>
        /// <returns>A new instance of <see cref="PipelineClient"/>.</returns>
        public PipelineClient GetPipelineClient(Uri pipelineUrl, TokenCredential tokenCredential);

        /// <summary>
        /// Creates a new instance of <see cref="PipelineRunClient"/> for the specified pipeline URL and token credential.
        /// </summary>
        /// <param name="pipelineUrl">The URL of the pipeline.</param>
        /// <param name="tokenCredential">The token credential for authentication.</param>
        /// <returns>A new instance of <see cref="PipelineRunClient"/>.</returns>
        public PipelineRunClient GetPipelineRunClient(Uri pipelineUrl, TokenCredential tokenCredential);

        /// <summary>
        /// Creates a new instance of <see cref="HttpClient"/> for the specified status update URL.
        /// </summary>
        /// <param name="statusUpdateUrl">The URL for status updates.</param>
        /// <returns>A new instance of <see cref="HttpClient"/>.</returns>
        public HttpClient GetHttpClient(Uri statusUpdateUrl);
    }
}