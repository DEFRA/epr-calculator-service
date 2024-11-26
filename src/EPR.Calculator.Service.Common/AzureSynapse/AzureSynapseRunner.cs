namespace EPR.Calculator.Service.Common.AzureSynapse
{
    using System.Text;
    using System.Text.Json;
    using Azure.Identity;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Runs Azure Synapse pipelines.
    /// </summary>
    public class AzureSynapseRunner : IAzureSynapseRunner
    {
        private readonly ILogger<AzureSynapseRunner> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureSynapseRunner"/> class.
        /// </summary>
        public AzureSynapseRunner(ILogger<AzureSynapseRunner> logger)
        : this(new PipelineClientFactory(), logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureSynapseRunner"/> class.
        /// </summary>
        /// <param name="pipelineClientFactory">A factory that initialises pipeline clients
        /// (or generates mock clients when unit testing).</param>
        internal AzureSynapseRunner(
            IPipelineClientFactory pipelineClientFactory, ILogger<AzureSynapseRunner> logger)
        {
            this.PipelineClientFactory = pipelineClientFactory;
            this.logger = logger;
        }

        private IPipelineClientFactory PipelineClientFactory { get; }

        /// <inheritdoc/>
        public async Task<bool> Process(AzureSynapseRunnerParameters args)
        {
            this.logger.LogInformation("Azure synapse trigger process started");
            // instead of year will get financial year need to map finacial year to calendar year
            // Trigger the pipeline.
            Guid pipelineRunId;
            pipelineRunId = await StartPipelineRun(
                this.PipelineClientFactory,
                args.PipelineUrl,
                args.PipelineName,
                args.FinancialYear);

            this.logger.LogInformation($"pipelineRunId:{pipelineRunId}");

            // Periodically check the status of pipeline until it's completed.
            var checkCount = 0;
            var pipelineStatus = nameof(PipelineStatus.NotStarted);
            do
            {
                checkCount++;
                try
                {
                    this.logger.LogInformation($"pipelineRunId checkCount:{checkCount}");
                    pipelineStatus = await GetPipelineRunStatus(
                        this.PipelineClientFactory,
                        args.PipelineUrl,
                        pipelineRunId);

                    this.logger.LogInformation($"pipelineStatus for pipelineRunId {pipelineRunId}:{pipelineStatus}");
                    if (pipelineStatus != nameof(PipelineStatus.InProgress))
                    {
                        break;
                    }
                }
                catch(Exception ex)
                {
                    this.logger.LogError($"Error Occurred in Azure Synapse Runner  :{ex.StackTrace}");
                    this.logger.LogInformation($"Check Count inside exception  :{checkCount}");
                    // Something went wrong retrieving the status,but we're going to try again,
                    // so ignore it unless this is the last try.
                    if (checkCount >= 10)
                    {
                        break;
                    }
                }

                var checkInterval = args.CheckInterval == 0
                    ? TimeSpan.FromMilliseconds(120000)
                    : TimeSpan.FromMicroseconds(args.CheckInterval);

                this.logger.LogInformation($"ChekInterval  :{checkInterval} ");

                this.logger.LogInformation($"Task started at :{DateTime.Now}");

                await Task.Delay(checkInterval);
                this.logger.LogInformation($"Task completed at :{DateTime.Now} with CheckInterval {checkInterval} and checkcount is {checkCount}");
            }
            while (checkCount < 10);

            this.logger.LogInformation($"Azure Synapse Runner completed at :{DateTime.Now} and checkcount is {checkCount}");

            return pipelineStatus == nameof(PipelineStatus.Succeeded);
        }

        /// <summary>
        /// Build the JSON content of the status update API call.
        /// </summary>
        public static StringContent GetStatusUpdateMessage(int calculatorRunId, bool pipelineSucceeded)
            => new StringContent(
                JsonSerializer.Serialize(new
                {
                    runId = calculatorRunId,
                    updatedBy = "string",
                    isSuccessful = pipelineSucceeded,
                }),
                Encoding.UTF8,
                "application/json");

        private static async Task<Guid> StartPipelineRun(
            IPipelineClientFactory factory,
            Uri pipelineUrl,
            string pipelineName,
            string year)
        {
#if DEBUG
            var credentials = new DefaultAzureCredential();
#else
            var credentials = new ManagedIdentityCredential();
#endif

            var pipelineClient = factory.GetPipelineClient(pipelineUrl, credentials);

            var result = await pipelineClient.CreatePipelineRunAsync(
            pipelineName,
            parameters: new Dictionary<string, object>
            {
                { "date", year },
            });

            return Guid.Parse(result.Value.RunId);
        }

        private static async Task<string> GetPipelineRunStatus(IPipelineClientFactory factory, Uri pipelineUrl, Guid runId)
        {
#if DEBUG
            var credentials = new DefaultAzureCredential();
#else
            var credentials = new ManagedIdentityCredential();
#endif

            var pipelineClient = factory.GetPipelineRunClient(pipelineUrl, credentials);

            var result = await pipelineClient.GetPipelineRunAsync(runId.ToString());
            return result.Value.Status;
        }
    }
}