namespace EPR.Calculator.Service.Common.AzureSynapse
{
    using Azure.Identity;

    /// <summary>
    /// Runs Azure Synapse pipelines.
    /// </summary>
    /// <param name="pipelineClientFactory">A factory that initialises pipeline clients
    /// (or generates mock clients when unit testing).</param>
    /// <param name="pipelineUrl">The URL of the pipeline to run.</param>
    /// <param name="pipelineName">The name of the pipeline to run.</param>
    /// <param name="maxChecks">The maximum number of times to check whether the pipeline has completed,
    /// before reporting a failure.</param>
    /// <param name="checkInterval">The time to wait before re-checking to see
    /// if the pipeline has run successfully.</param>
    public class AzureSynapseRunner(
        PipelineClientFactory pipelineClientFactory,
        Uri pipelineUrl,
        string pipelineName,
        int maxChecks,
        int checkInterval)
        : IAzureSynapseRunner
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureSynapseRunner"/> class.
        /// </summary>
        /// <param name="pipelineUrl">The URL of the pipeline to run.</param>
        /// <param name="pipelineName">The name of the pipeline to run.</param>
        /// <param name="maxChecks">The maximum number of times to check whether the pipeline has completed,
        /// before reporting a failure.</param>
        /// <param name="checkInterval">The time to wait before re-checking to see
        /// if the pipeline has run successfully.</param>
        public AzureSynapseRunner(
        Uri pipelineUrl,
        string pipelineName,
        int maxChecks,
        int checkInterval)
        : this(new PipelineClientFactory(), pipelineUrl, pipelineName, maxChecks, checkInterval)
        {
        }

        private int CheckInterval { get; } = checkInterval;

        private int MaxChecks { get; } = maxChecks;

        private PipelineClientFactory PipelineClientFactory { get; init; } = pipelineClientFactory;

        private string PipelineName { get; } = pipelineName;

        private Uri PipelineUrl { get; } = pipelineUrl;

        /// <inheritdoc/>
        public async Task<bool> Process(PipelineParameters parameters)
        {
            ArgumentNullException.ThrowIfNull(parameters);
            ArgumentException.ThrowIfNullOrWhiteSpace(parameters.CalculationId);
            ArgumentException.ThrowIfNullOrWhiteSpace(parameters.FinancialYear);
            ArgumentException.ThrowIfNullOrWhiteSpace(parameters.UserId);

            // Trigger the pipeline.
            Guid runId;
            runId = await this.StartPipelineRun(this.PipelineClientFactory, parameters);

            // Periodically check the status of pipeline until it's completed.
            var checkCount = 0;
            var pipelineStatus = string.Empty;
            do
            {
                checkCount++;
                try
                {
                    pipelineStatus = await this.GetPipelineRunStatus(this.PipelineClientFactory, runId);
                    if (pipelineStatus != nameof(PipelineStatus.Running))
                    {
                        break;
                    }
                }
                catch
                {
                    // Something went wrong retrieving the status,but we're going to try again,
                    // so ignore it unless this is the last try.
                    if (checkCount >= this.MaxChecks)
                    {
                        throw;
                    }
                }

                await Task.Delay(this.CheckInterval);
            }
            while (checkCount < this.MaxChecks);

            // Record success/failure.
            if (pipelineStatus == nameof(PipelineStatus.Succeeded))
            {
                // TODO: Update the status in the database.
                return true;
            }
            else
            {
                // TODO: Update the status in the database.
                return false;
            }
        }

        private async Task<Guid> StartPipelineRun(PipelineClientFactory factory, PipelineParameters parameters)
        {
#if DEBUG
            var credentials = new DefaultAzureCredential();
#else
            var credentials = new ManagedIdentityCredential();
#endif

            var pipelineClient = factory.GetPipelineClient(this.PipelineUrl, credentials);

            var result = await pipelineClient.CreatePipelineRunAsync(
            this.PipelineName,
            parameters: new Dictionary<string, object>
            {
                // Parameter names are placeholders
                // - we need to find out what names the pipeline is expecting for them.
                { "Calculation ID", parameters.CalculationId },
                { "Financial Year", parameters.FinancialYear },
                { "User ID", parameters.UserId },
            });

            return Guid.Parse(result.Value.RunId);
        }

        private async Task<string> GetPipelineRunStatus(PipelineClientFactory factory, Guid runId)
        {
#if DEBUG
            var credentials = new DefaultAzureCredential();
#else
            var credentials = new ManagedIdentityCredential();
#endif

            var pipelineClient = factory.GetPipelineRunClient(this.PipelineUrl, credentials);

            var result = await pipelineClient.GetPipelineRunAsync(runId.ToString());
            return result.Value.Status;
        }
    }
}