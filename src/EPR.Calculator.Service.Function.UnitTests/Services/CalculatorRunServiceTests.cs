namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    using AutoFixture;
    using Azure.Analytics.Synapse.Artifacts.Models;  
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.AzureSynapse;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.Extensions.Logging;
    using Moq;

    [TestClass]
    public class CalculatorRunServiceTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorRunServiceTests"/> class.
        /// </summary>
        public CalculatorRunServiceTests()
        {
            this.Fixture = new Fixture();
            this.AzureSynapseRunner = new Mock<IAzureSynapseRunner>();
            this.MockLogger = new Mock<ILogger<CalculatorRunService>>();
            this.CalculatorRunService = new CalculatorRunService(this.AzureSynapseRunner.Object, this.MockLogger.Object );
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PomDataPipelineName, this.Fixture.Create<string>());
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PomDataPipelineName, this.Fixture.Create<string>());
        }

        private CalculatorRunService CalculatorRunService { get; }

        private Mock<IAzureSynapseRunner> AzureSynapseRunner { get; }

        private Mock<ILogger<CalculatorRunService>> MockLogger { get; }

        private Fixture Fixture { get; }

        /// <summary>
        /// Checks that the service calls the Azure Synapse runner and passes the correct parameters to it.
        /// </summary>
        /// <param name="pipelineNameKey">Which pipeline to test - The service should call the synapse runner twice
        /// - once for each pipeline, so we run this test for each pipeline we want to check is being called.</param>
        [TestMethod]
        [DataRow(EnvironmentVariableKeys.PomDataPipelineName)]
        [DataRow(EnvironmentVariableKeys.OrgDataPipelineName)]
        public void StartProcessCallsAzureSynapseRunner(string pipelineNameKey)
        {
            // Arrange
            var id = this.Fixture.Create<int>();
            var financialYear = "2024-25";
            var user = this.Fixture.Create<string>();

            var checkInterval = this.Fixture.Create<int>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.CheckInterval,
                checkInterval.ToString());

            var maxChecks = this.Fixture.Create<int>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.MaxCheckCount,
                maxChecks.ToString());

            var pipelineUrl = this.Fixture.Create<Uri>();
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PipelineUrl, pipelineUrl.ToString());

            var pipelineName = this.Fixture.Create<string>();
            Environment.SetEnvironmentVariable(pipelineNameKey, pipelineName.ToString());

            var statusUpdateEndpoint = this.Fixture.Create<Uri>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.StatusUpdateEndpoint,
                statusUpdateEndpoint.ToString());

            var calculatorRunParameters = new CalculatorRunParameter
            {
                Id = id,
                FinancialYear = financialYear,
                User = user,
            };

            // The values that the service is expected to pass to the pipeline runner.
            var expectedParameters = new AzureSynapseRunnerParameters
            {
                CalculatorRunId = id,
                CheckInterval = checkInterval,
                FinancialYear = financialYear,
                MaxChecks = maxChecks,
                PipelineUrl = pipelineUrl,
                PipelineName = pipelineName,
                StatusUpdateEndpoint = statusUpdateEndpoint,
            };

            // Act
            this.CalculatorRunService.StartProcess(calculatorRunParameters);

            // Assert
            this.AzureSynapseRunner.Verify(
                runner => runner.Process(
                    It.IsAny<AzureSynapseRunnerParameters>()),
                Times.Exactly(2));

            this.AzureSynapseRunner.Verify(
                runnner => runnner.Process(
                    It.Is<AzureSynapseRunnerParameters>(args =>
                    args == expectedParameters)),
                Times.Never);
        }
    }
}