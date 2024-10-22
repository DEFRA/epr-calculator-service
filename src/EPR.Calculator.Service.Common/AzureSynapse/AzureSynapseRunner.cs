namespace EPR.Calculator.Service.Common.AzureSynapse
{
    using System.Text;
    using System.Text.Json;
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
    /// <param name="statusUpdateEndpoint">
    /// The URL of the endpoint that's used to access the database and update the status of the run.
    /// </param>
    public class AzureSynapseRunner(
        PipelineClientFactory pipelineClientFactory,
        Uri pipelineUrl,
        string pipelineName,
        int maxChecks,
        int checkInterval,
        Uri statusUpdateEndpoint)
        : IAzureSynapseRunner
    {
        /// <summary>
        /// The value to write to the database to indicate that the pipeline succeeded.
        /// </summary>
        public const string DatabaseSuccessValue = "SUCCESS";

        /// <summary>
        /// The value to write to the database to indicate that the pipeline failed.
        /// </summary>
        public const string DatabaseFailureValue = "FAILED";

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureSynapseRunner"/> class.
        /// </summary>
        /// <param name="pipelineUrl">The URL of the pipeline to run.</param>
        /// <param name="pipelineName">The name of the pipeline to run.</param>
        /// <param name="maxChecks">The maximum number of times to check whether the pipeline has completed,
        /// before reporting a failure.</param>
        /// <param name="checkInterval">The time to wait before re-checking to see
        /// if the pipeline has run successfully.</param>
        /// <param name="statusUpdateEndpoint">
        /// The URL of the endpoint that's used to access the database and update the status of the run.
        /// </param>
        public AzureSynapseRunner(
        Uri pipelineUrl,
        string pipelineName,
        int maxChecks,
        int checkInterval,
        Uri statusUpdateEndpoint)
        : this(
              new PipelineClientFactory(),
              pipelineUrl,
              pipelineName,
              maxChecks,
              checkInterval,
              statusUpdateEndpoint)
        {
        }

        private int CheckInterval { get; } = checkInterval;

        private int MaxChecks { get; } = maxChecks;

        private PipelineClientFactory PipelineClientFactory { get; init; } = pipelineClientFactory;

        private string PipelineName { get; } = pipelineName;

        private Uri PipelineUrl { get; } = pipelineUrl;

        private Uri StatusUpdateUrl { get; } = statusUpdateEndpoint;

        /// <inheritdoc/>
        public async Task<bool> Process(FinancialYear financialYear)
        {
            // instead of year will get financial year need to map finacial year to calendar year
            // Trigger the pipeline.
            Guid runId;
            runId = await this.StartPipelineRun(this.PipelineClientFactory, financialYear.ToCalendarYear());

            // Periodically check the status of pipeline until it's completed.
            var checkCount = 0;
            var pipelineStatus = nameof(PipelineStatus.NotStarted);
            do
            {
                checkCount++;
                try
                {
                    pipelineStatus = await this.GetPipelineRunStatus(this.PipelineClientFactory, runId);
                    if (pipelineStatus != nameof(PipelineStatus.InProgress))
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
                        break;
                    }
                }

                await Task.Delay(this.CheckInterval);
            }
            while (checkCount < this.MaxChecks);

            // Record success/failure to the database using the web API.
            var statusUpdateResponse = await this.PipelineClientFactory
                .GetStatusUpdateClient(this.StatusUpdateUrl)
                .PostAsync(
                    this.StatusUpdateUrl.ToString(),
                    GetStatusUpdateMessage(pipelineStatus));

            return pipelineStatus == nameof(PipelineStatus.Succeeded) && statusUpdateResponse.IsSuccessStatusCode;
        }

        /// <summary>
        /// Build the JSON content of the status update API call.
        /// </summary>
        private static StringContent GetStatusUpdateMessage(string pipelineStatus)
        {
            string messageStatus = pipelineStatus switch
            {
                nameof(PipelineStatus.Succeeded) => DatabaseSuccessValue,
                _ => DatabaseFailureValue,
            };

            return new StringContent(
                JsonSerializer.Serialize(new
                {
                    status = messageStatus,
                }),
                Encoding.UTF8,
                "application/json");
        }

        private async Task<Guid> StartPipelineRun(PipelineClientFactory factory, DateTime year)
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
                { "date", year.ToString("yyyy") },
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