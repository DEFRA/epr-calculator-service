﻿namespace EPR.Calculator.Service.Common.AzureSynapse
{
    using Azure.Analytics.Synapse.Artifacts;
    using Azure.Core;

    /// <summary>
    /// Factory for initialising Azure Synapse pipeline clients.
    /// </summary>
    /// <remarks>
    /// Used by <see cref="AzureSynapsePipelineTestController"/> via dependancy injection
    /// so that the clients can be replaced with mocks when unit testing.
    /// </remarks>
    public class PipelineClientFactory()
    {
        /// <summary>
        /// Initialises a new <see cref="PipelineClient"/>.
        /// </summary>
        /// <param name="pipelineUrl">The URL of the pipeline the client will connect to.</param>
        /// <param name="tokenCredential">The credentials the client will use to connect to Azure.</param>
        /// <returns>A new <see cref="PipelineClient"/>.</returns>
        public virtual PipelineClient GetPipelineClient(Uri pipelineUrl, TokenCredential tokenCredential)
            => new PipelineClient(pipelineUrl, tokenCredential);

        /// <summary>
        /// Initialises a new <see cref="PipelineRunClient"/>.
        /// </summary>
        /// <param name="pipelineUrl">The URL of the pipeline the client will connect to.</param>
        /// <param name="tokenCredential">The credentials the client will use to connect to Azure.</param>
        /// <returns>A new <see cref="PipelineRunClient"/>.</returns>
        public virtual PipelineRunClient GetPipelineRunClient(Uri pipelineUrl, TokenCredential tokenCredential)
            => new PipelineRunClient(pipelineUrl, tokenCredential);
    }
}
