﻿namespace EPR.Calculator.Service.Common.AzureSynapse
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
        private const string DateParameter = "date";
        private readonly ILogger<AzureSynapseRunner> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureSynapseRunner"/> class.
        /// </summary>
        /// <param name="logger">The logger instance to use for logging.</param>
        public AzureSynapseRunner(ILogger<AzureSynapseRunner> logger)
            : this(new PipelineClientFactory(), logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureSynapseRunner"/> class.
        /// </summary>
        /// <param name="pipelineClientFactory">A factory that initializes pipeline clients
        /// (or generates mock clients when unit testing).</param>
        /// <param name="logger">The logger instance to use for logging.</param>
        internal AzureSynapseRunner(
            IPipelineClientFactory pipelineClientFactory, ILogger<AzureSynapseRunner> logger)
        {
            this.PipelineClientFactory = pipelineClientFactory;
            this.logger = logger;
        }

        private IPipelineClientFactory PipelineClientFactory { get; }

        /// <summary>
        /// Processes the Azure Synapse Runner with the specified parameters.
        /// </summary>
        /// <param name="args">The parameters required to run the Azure Synapse Runner.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the pipeline run succeeded.</returns>
        public async Task<bool> Process(AzureSynapseRunnerParameters args)
        {
            this.logger.LogInformation("Azure synapse trigger process started");

            // Instead of year, get financial year and map financial year to calendar year.
            // Trigger the pipeline.
            Guid pipelineRunId;
            pipelineRunId = await StartPipelineRun(
                this.PipelineClientFactory,
                args.PipelineUrl,
                args.PipelineName,
                args.CalendarYear);

            this.logger.LogInformation($"pipelineRunId: {pipelineRunId}");

            // Periodically check the status of the pipeline until it's completed.
            var checkCount = 0;
            var pipelineStatus = nameof(PipelineStatus.NotStarted);
            var maxCheckCount = args.MaxCheckCount;
            do
            {
                checkCount++;
                try
                {
                    this.logger.LogInformation($"pipelineRunId checkCount: {checkCount}");
                    pipelineStatus = await GetPipelineRunStatus(
                        this.PipelineClientFactory,
                        args.PipelineUrl,
                        pipelineRunId);

                    this.logger.LogInformation($"pipelineStatus for pipelineRunId {pipelineRunId}: {pipelineStatus}");
                    if (pipelineStatus.Contains(nameof(PipelineStatus.Succeeded)))
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"Error occurred in Azure Synapse Runner: {ex.StackTrace}");
                    this.logger.LogInformation($"Check count inside exception: {checkCount}");

                    // Something went wrong retrieving the status, but we're going to try again,
                    // so ignore it unless this is the last try.
                    if (checkCount >= maxCheckCount)
                    {
                        break;
                    }
                }

                var checkInterval = args.CheckInterval;

                this.logger.LogInformation($"CheckInterval: {checkInterval}");
                this.logger.LogInformation($"Task started at: {DateTime.Now}");

                await Task.Delay(TimeSpan.FromMilliseconds(checkInterval));
                this.logger.LogInformation($"Task completed at: {DateTime.Now} with CheckInterval {checkInterval} and checkCount is {checkCount}");
            }
            while (checkCount < maxCheckCount);

            this.logger.LogInformation($"Azure Synapse Runner completed at: {DateTime.Now} and checkCount is {checkCount}");

            return pipelineStatus == nameof(PipelineStatus.Succeeded);
        }

        /// <summary>
        /// Starts a pipeline run.
        /// </summary>
        /// <param name="factory">The factory to create pipeline clients.</param>
        /// <param name="pipelineUrl">The URL of the pipeline.</param>
        /// <param name="pipelineName">The name of the pipeline.</param>
        /// <param name="year">The year parameter for the pipeline.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the pipeline run ID.</returns>
        private static async Task<Guid> StartPipelineRun(
            IPipelineClientFactory factory,
            Uri pipelineUrl,
            string pipelineName,
            string year)
        {
            var credentials = new ManagedIdentityCredential();

            var pipelineClient = factory.GetPipelineClient(pipelineUrl, credentials);

            var result = await pipelineClient.CreatePipelineRunAsync(
                pipelineName,
                parameters: new Dictionary<string, object>
                {
            { DateParameter, year },
                });

            return Guid.Parse(result.Value.RunId);
        }

        /// <summary>
        /// Retrieves the status of a pipeline run.
        /// </summary>
        /// <param name="factory">The factory to create pipeline clients.</param>
        /// <param name="pipelineUrl">The URL of the pipeline.</param>
        /// <param name="runId">The ID of the pipeline run.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the status of the pipeline run.</returns>
        private static async Task<string> GetPipelineRunStatus(IPipelineClientFactory factory, Uri pipelineUrl, Guid runId)
        {
            var credentials = new ManagedIdentityCredential();

            var pipelineClient = factory.GetPipelineRunClient(pipelineUrl, credentials);

            var result = await pipelineClient.GetPipelineRunAsync(runId.ToString());
            return result.Value.Status;
        }
    }
}