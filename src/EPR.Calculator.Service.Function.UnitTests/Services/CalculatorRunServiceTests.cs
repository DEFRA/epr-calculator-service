namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using AutoFixture;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.AzureSynapse;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.Service.Common.UnitTests.AutoFixtureCustomisations;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Services;
    using Moq;
    using Moq.Protected;

    /// <summary>
    /// Contains unit tests for the CalculatorRunService class.
    /// </summary>
    [TestClass]
    public class CalculatorRunServiceTests
    {
        private FinancialYear FinancialYear { get; init; } = "2024-25";

        private CalendarYear CalendarYear { get; init; } = "2023";

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorRunServiceTests"/> class.
        /// </summary>
        public CalculatorRunServiceTests()
        {
            this.Fixture = new Fixture();
            this.Fixture.Customizations.Add(new FinancialYearCustomisation());
            this.AzureSynapseRunner = new Mock<IAzureSynapseRunner>();
            this.MockLogger = new Mock<ICalculatorTelemetryLogger>();
            this.TransposeService = new Mock<ITransposePomAndOrgDataService>();
            this.RunNameService = new Mock<IRunNameService>();
            this.Configuration = new Mock<IConfigurationService>();
            this.TelemetryLogger = new Mock<ICalculatorTelemetryLogger>();

            this.MockStatusUpdateHandler = new Mock<HttpMessageHandler>();
            this.MockStatusUpdateHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            this.PrepareCalcService = new Mock<IPrepareCalcService>();
            this.PrepareCalcService.Setup(s => s.PrepareCalcResults(
                It.IsAny<CalcResultsRequestDto>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            this.StatusService = new Mock<IRpdStatusService>();
            this.StatusService.Setup(s => s.UpdateRpdStatus(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(RunClassification.RUNNING);

            var httpClient = new HttpClient(this.MockStatusUpdateHandler.Object)
            {
                BaseAddress = new Uri("http://test.com"),
            };

            this.PipelineClientFactory = new Mock<PipelineClientFactory>();
            this.PipelineClientFactory.Setup(factory => factory.GetHttpClient(It.IsAny<Uri>()))
                .Returns(httpClient);

            this.CalculatorRunService = new CalculatorRunService(
                this.AzureSynapseRunner.Object,
                this.MockLogger.Object,
                this.TransposeService.Object,
                new Configuration(),
                this.PrepareCalcService.Object,
                this.StatusService.Object);

            this.TransposeService.Setup(t => t.TransposeBeforeCalcResults(
                It.IsAny<CalcResultsRequestDto>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PomDataPipelineName, this.Fixture.Create<string>());
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.OrgDataPipelineName, this.Fixture.Create<string>());
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.StatusUpdateEndpoint, this.Fixture.Create<string>());
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PrepareCalcResultEndPoint, this.Fixture.Create<Uri>().ToString());
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.TransposeEndpoint, this.Fixture.Create<Uri>().ToString());
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.RpdStatusTimeout, "0");
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.StatusUpdateEndpoint, this.Fixture.Create<Uri>().ToString());
        }

        private CalculatorRunService CalculatorRunService { get; }

        private Mock<IAzureSynapseRunner> AzureSynapseRunner { get; }

        private Mock<ICalculatorTelemetryLogger> MockLogger { get; }

        private Fixture Fixture { get; }

        private Mock<HttpMessageHandler> MockStatusUpdateHandler { get; set; }

        private Mock<PipelineClientFactory> PipelineClientFactory { get; }

        private Mock<ITransposePomAndOrgDataService> TransposeService { get; }

        private Mock<IPrepareCalcService> PrepareCalcService { get; init; }

        private Mock<IRunNameService> RunNameService { get; }

        private Mock<IConfigurationService> Configuration { get; }

        private Mock<ICalculatorTelemetryLogger> TelemetryLogger { get; }

        private Mock<IRpdStatusService> StatusService { get; }

        /// <summary>
        /// Checks that the service calls the Azure Synapse runner and passes the correct parameters to it.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [TestMethod]
        public async Task StartProcessCallsAzureSynapseRunnerIfOrgPipelineIsUnsuccessful()
        {
            // Arrange
            var id = this.Fixture.Create<int>();
            var user = this.Fixture.Create<string>();
            var runName = "Test Run Name";

            var checkInterval = this.Fixture.Create<int>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.CheckInterval,
                checkInterval.ToString());

            var maxCheckCount = this.Fixture.Create<int>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.MaxCheckCount,
                maxCheckCount.ToString());

            var pipelineUrl = this.Fixture.Create<Uri>();
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PipelineUrl, pipelineUrl.ToString());

            var orgPipelineName = "test";
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.OrgDataPipelineName, orgPipelineName);

            var pomPipelineName = "pomtest";
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PomDataPipelineName, pomPipelineName);

            var statusUpdateEndpoint = this.Fixture.Create<Uri>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.StatusUpdateEndpoint,
                statusUpdateEndpoint.ToString());

            var prepareCalcResultEndPoint = this.Fixture.Create<Uri>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.PrepareCalcResultEndPoint,
                prepareCalcResultEndPoint.ToString());

            var runRPDPipeline = true; // Ensure this is set to true to test the org pipeline
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.ExecuteRPDPipeline,
                runRPDPipeline.ToString());

            var calculatorRunParameters = new CalculatorRunParameter
            {
                Id = id,
                FinancialYear = this.FinancialYear,
                User = user,
                MessageType = MessageTypes.Result
            };

            // The values that the service is expected to pass to the pipeline runner.
            var expectedOrgParameters = new AzureSynapseRunnerParameters
            {
                CalculatorRunId = id,
                CheckInterval = checkInterval,
                CalendarYear = this.CalendarYear,
                MaxCheckCount = maxCheckCount,
                PipelineUrl = pipelineUrl,
                PipelineName = orgPipelineName,
            };

            // Mock the AzureSynapseRunner to return false for the org pipeline
            this.AzureSynapseRunner.Setup(t => t.Process(It.Is<AzureSynapseRunnerParameters>(p =>
                p.CalculatorRunId == id &&
                p.CheckInterval == checkInterval &&
                p.CalendarYear == this.CalendarYear &&
                p.MaxCheckCount == maxCheckCount &&
                p.PipelineUrl == pipelineUrl &&
                p.PipelineName == orgPipelineName)))
                .ReturnsAsync(false);

            this.RunNameService.Setup(t => t.GetRunNameAsync(It.IsAny<int>())).ReturnsAsync(runName);

            // Act
            await this.CalculatorRunService.StartProcess(calculatorRunParameters, runName);

            // Assert

            // Verify that the org pipeline was called
            this.AzureSynapseRunner.Verify(
                t => t.Process(It.Is<AzureSynapseRunnerParameters>(p =>
                p.CalculatorRunId == id &&
                p.CheckInterval == checkInterval &&
                p.CalendarYear == this.CalendarYear &&
                p.MaxCheckCount == maxCheckCount &&
                p.PipelineUrl == pipelineUrl &&
                p.PipelineName == orgPipelineName)),
                Times.Once);

            // Verify that the pom pipeline was not called
            this.AzureSynapseRunner.Verify(
                t => t.Process(It.Is<AzureSynapseRunnerParameters>(p =>
                p.CalculatorRunId == id &&
                p.CheckInterval == checkInterval &&
                p.CalendarYear == this.CalendarYear &&
                p.MaxCheckCount == maxCheckCount &&
                p.PipelineUrl == pipelineUrl &&
                p.PipelineName == pomPipelineName)),
                Times.Never);
        }

        /// <summary>
        /// Checks that the service calls the Azure Synapse runner and passes the correct parameters to it
        /// when the RPD pipeline is disabled.
        /// </summary>
        /// <param name="pipelineNameKey">
        /// Which pipeline to test - The service should call the synapse runner twice,
        /// once for each pipeline, so we run this test for each pipeline we want to check is being called.
        /// </param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [TestMethod]
        [DataRow(EnvironmentVariableKeys.OrgDataPipelineName)]
        [DataRow(EnvironmentVariableKeys.PomDataPipelineName)]
        public async Task StartProcessCallsAzureSynapseRunnerWithRPDPipelineDisabled(string pipelineNameKey)
        {
            // Arrange
            var id = this.Fixture.Create<int>();
            var user = this.Fixture.Create<string>();
            var runName = "Test Run Name";

            var checkInterval = this.Fixture.Create<int>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.CheckInterval,
                checkInterval.ToString());

            var maxCheckCount = this.Fixture.Create<int>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.MaxCheckCount,
                maxCheckCount.ToString());

            var pipelineUrl = this.Fixture.Create<Uri>();
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PipelineUrl, pipelineUrl.ToString());

            var orgPipelineName = "test";
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.OrgDataPipelineName, orgPipelineName);

            var pomPipelineName = "pomtest";
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PomDataPipelineName, pomPipelineName);

            var statusUpdateEndpoint = this.Fixture.Create<Uri>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.StatusUpdateEndpoint,
                statusUpdateEndpoint.ToString());

            var runRPDPipeline = false;
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.ExecuteRPDPipeline,
                runRPDPipeline.ToString());

            var calculatorRunParameters = new CalculatorRunParameter
            {
                Id = id,
                FinancialYear = this.FinancialYear,
                User = user,
                MessageType = MessageTypes.Result
            };

            // The values that the service is expected to pass to the pipeline runner.
            var expectedParameters = new AzureSynapseRunnerParameters
            {
                CalculatorRunId = id,
                CheckInterval = checkInterval,
                CalendarYear = this.CalendarYear,
                MaxCheckCount = maxCheckCount,
                PipelineUrl = pipelineUrl,
                PipelineName = orgPipelineName,
            };

            // Mock the AzureSynapseRunner to ensure it is not called
            this.AzureSynapseRunner.Setup(t => t.Process(It.IsAny<AzureSynapseRunnerParameters>()))
                .ReturnsAsync(true);

            this.RunNameService.Setup(t => t.GetRunNameAsync(It.IsAny<int>())).ReturnsAsync(runName);

            // Act
            await this.CalculatorRunService.StartProcess(calculatorRunParameters, runName);

            // Assert
            this.AzureSynapseRunner.Verify(
                runner => runner.Process(
                    It.IsAny<AzureSynapseRunnerParameters>()),
                Times.Never);
        }

        /// <summary>
        /// Verifies that the service calls the Azure Synapse runner and passes the correct parameters to it,
        /// ensuring both the org and pom pipelines are processed successfully.
        /// </summary>
        /// <param name="pipelineNameKey">The key for the pipeline name environment variable.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [TestMethod]
        [DataRow(EnvironmentVariableKeys.OrgDataPipelineName)]
        [DataRow(EnvironmentVariableKeys.PomDataPipelineName)]
        public async Task StartProcessCallsAzureSynapseRunnerSuccessfully(string pipelineNameKey)
        {
            // Arrange
            var id = this.Fixture.Create<int>();
            var user = this.Fixture.Create<string>();
            var runName = "Test Run Name";

            var checkInterval = 5;
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.CheckInterval,
                checkInterval.ToString());

            var maxCheckCount = 10;
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.MaxCheckCount,
                maxCheckCount.ToString());

            var pipelineUrl = this.Fixture.Create<Uri>();
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PipelineUrl, pipelineUrl.ToString());

            var orgPipelineName = "test";
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.OrgDataPipelineName, orgPipelineName);

            var pomPipelineName = "pomtest";
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PomDataPipelineName, pomPipelineName);

            var runRPDPipeline = true;
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.ExecuteRPDPipeline,
                runRPDPipeline.ToString());

            var statusUpdateEndpoint = this.Fixture.Create<Uri>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.StatusUpdateEndpoint,
                statusUpdateEndpoint.ToString());

            var calculatorRunParameters = new CalculatorRunParameter
            {
                Id = id,
                FinancialYear = this.FinancialYear,
                User = user,
                MessageType = MessageTypes.Result
            };

            // The values that the service is expected to pass to the pipeline runner.
            var expectedOrgParameters = new AzureSynapseRunnerParameters
            {
                CalculatorRunId = id,
                CheckInterval = checkInterval,
                CalendarYear = this.CalendarYear,
                MaxCheckCount = maxCheckCount,
                PipelineUrl = pipelineUrl,
                PipelineName = orgPipelineName,
            };

            var expectedPomParameters = new AzureSynapseRunnerParameters
            {
                CalculatorRunId = id,
                CheckInterval = checkInterval,
                CalendarYear = this.CalendarYear,
                MaxCheckCount = maxCheckCount,
                PipelineUrl = pipelineUrl,
                PipelineName = pomPipelineName,
            };

            // Mock the AzureSynapseRunner to return true for both pipelines
            this.AzureSynapseRunner.Setup(t => t.Process(It.Is<AzureSynapseRunnerParameters>(p =>
                p.PipelineName == orgPipelineName))).ReturnsAsync(true);

            this.AzureSynapseRunner.Setup(t => t.Process(It.Is<AzureSynapseRunnerParameters>(p =>
                p.PipelineName == pomPipelineName))).ReturnsAsync(true);

            this.RunNameService.Setup(t => t.GetRunNameAsync(It.IsAny<int>())).ReturnsAsync(runName);

            // Act
            var result = await this.CalculatorRunService.StartProcess(calculatorRunParameters, runName);

            // Assert
            Assert.IsTrue(result);

            // Verify that the org pipeline was called once
            this.AzureSynapseRunner.Verify(
                t => t.Process(It.Is<AzureSynapseRunnerParameters>(p =>
                p.PipelineName == orgPipelineName)), Times.Once);

            // Verify that the pom pipeline was called once
            this.AzureSynapseRunner.Verify(
                t => t.Process(It.Is<AzureSynapseRunnerParameters>(p =>
                p.PipelineName == pomPipelineName)), Times.Once);
        }

        /// <summary>
        /// Verifies that the service returns false if the pom pipeline fails after the org pipeline succeeds.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task StartProcessReturnsFalseIfPomPipelineFails()
        {
            // Arrange
            var id = this.Fixture.Create<int>();
            var user = this.Fixture.Create<string>();
            var runName = "Test Run Name";

            var checkInterval = 5;
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.CheckInterval,
                checkInterval.ToString());

            var maxCheckCount = 10;
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.MaxCheckCount,
                maxCheckCount.ToString());

            var pipelineUrl = this.Fixture.Create<Uri>();
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PipelineUrl, pipelineUrl.ToString());

            var orgPipelineName = "test";
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.OrgDataPipelineName, orgPipelineName);

            var pomPipelineName = "pomtest";
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PomDataPipelineName, pomPipelineName);

            var runRPDPipeline = true;
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.ExecuteRPDPipeline,
                runRPDPipeline.ToString());

            var statusUpdateEndpoint = this.Fixture.Create<Uri>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.StatusUpdateEndpoint,
                statusUpdateEndpoint.ToString());

            var prepareCalcResultEndPoint = this.Fixture.Create<Uri>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.PrepareCalcResultEndPoint,
                prepareCalcResultEndPoint.ToString());

            var calculatorRunParameters = new CalculatorRunParameter
            {
                Id = id,
                FinancialYear = this.FinancialYear,
                User = user,
                MessageType = MessageTypes.Result
            };

            // Mock the AzureSynapseRunner to return true for the org pipeline and false for the pom pipeline
            this.AzureSynapseRunner.Setup(t => t.Process(It.Is<AzureSynapseRunnerParameters>(p =>
                p.PipelineName == orgPipelineName))).ReturnsAsync(true);

            this.AzureSynapseRunner.Setup(t => t.Process(It.Is<AzureSynapseRunnerParameters>(p =>
                p.PipelineName == pomPipelineName))).ReturnsAsync(false);

            this.RunNameService.Setup(t => t.GetRunNameAsync(It.IsAny<int>())).ReturnsAsync(runName);

            // Act
            var result = await this.CalculatorRunService.StartProcess(calculatorRunParameters, runName);

            // Assert
            Assert.IsFalse(result);

            // Verify that the org pipeline was called once
            this.AzureSynapseRunner.Verify(
                t => t.Process(It.Is<AzureSynapseRunnerParameters>(p =>
                p.PipelineName == orgPipelineName)), Times.Once);

            // Verify that the pom pipeline was called once
            this.AzureSynapseRunner.Verify(
                t => t.Process(It.Is<AzureSynapseRunnerParameters>(p =>
                p.PipelineName == pomPipelineName)), Times.Once);
        }

        /// <summary>
        /// Checks that <see cref="CalculatorRunService.StartProcess(CalculatorRunParameter)"/> returns false
        /// when the calculator timed out.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task StartProcessReturnsFalseWhenCalculatorTimesOut()
        {
            // Arrange
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.StatusUpdateEndpoint,
                this.Fixture.Create<Uri>().ToString());

            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.PrepareCalcResultEndPoint,
                this.Fixture.Create<Uri>().ToString());

            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.PrepareCalcResultsTimeout,
                "0.00000001");

            this.PipelineClientFactory.Setup(factory => factory.GetHttpClient(It.IsAny<Uri>()))
                .Returns(new HttpClient(new DelayedMessageHandler())
                {
                    BaseAddress = this.Fixture.Create<Uri>(),
                });

            var calculatorRunParameters = this.Fixture.Create<CalculatorRunParameter>();
            calculatorRunParameters.FinancialYear = "2024-25";
            var runName = "Test Run Name";

            this.StatusService.Setup(s => s.UpdateRpdStatus(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TaskCanceledException("Timed out!"));

            this.RunNameService.Setup(t => t.GetRunNameAsync(It.IsAny<int>())).ReturnsAsync(runName);

            // Act
            var result = await this.CalculatorRunService.StartProcess(calculatorRunParameters, runName);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task StartProcess_ShouldReturnFalseOn_TaskCanceledException()
        {
            // Arrange
            var calculatorRunParameter = new CalculatorRunParameter { Id = 1, User = "TestUser", FinancialYear = new FinancialYear("2024-25"), MessageType = MessageTypes.Result };
            var runName = "TestRun";
            var mockHttpClient = new Mock<HttpClient>();

            this.PipelineClientFactory.Setup(p => p.GetHttpClient(It.IsAny<Uri>())).Returns(mockHttpClient.Object);
            this.TransposeService.Setup(t => t.TransposeBeforeCalcResults(It.IsAny<CalcResultsRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TaskCanceledException());

            // Act
            var result = await this.CalculatorRunService.StartProcess(calculatorRunParameter, runName);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task StartProcess_ShouldReturnFalseOn_Exception()
        {
            // Arrange
            var calculatorRunParameter = new CalculatorRunParameter { Id = 1, User = "TestUser", FinancialYear = new FinancialYear("2024-25") , MessageType = MessageTypes.Result };
            var runName = "TestRun";
            var mockHttpClient = new Mock<HttpClient>();

            this.PipelineClientFactory.Setup(p => p.GetHttpClient(It.IsAny<Uri>())).Returns(mockHttpClient.Object);
            this.TransposeService.Setup(t => t.TransposeBeforeCalcResults(It.IsAny<CalcResultsRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await this.CalculatorRunService.StartProcess(calculatorRunParameter, runName);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetCalcResultMessage_ShouldReturnCorrect_StringContent()
        {
            // Arrange
            int calculatorRunId = 123;
            var expectedJson = JsonSerializer.Serialize(new { runId = calculatorRunId });
            var expectedContent = new StringContent(expectedJson, Encoding.UTF8, "application/json");

            // Act
            var result = CalculatorRunService.GetCalcResultMessage(calculatorRunId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedContent.Headers.ContentType?.MediaType, result.Headers.ContentType?.MediaType);
            Assert.AreEqual(expectedContent.Headers.ContentType?.CharSet, result.Headers.ContentType?.CharSet);
            Assert.AreEqual(expectedJson, result.ReadAsStringAsync().Result);
        }

        [TestMethod]
        [DataRow("org-data-pipeline", true)]
        [DataRow("ORG_DATA_PIPELINE", true)]
        [DataRow("organization", true)]
        [DataRow("pom-data-pipeline", false)]
        [DataRow("porg-data-pipeline", true)]
        [DataRow("dataorg", true)]
        [DataRow("data-org", true)]
        [DataRow("dataorgy", true)]
        [DataRow("data", false)]
        public void IsOrganisationPipeline_ShouldMatchExpected(string pipelineName, bool expected)
        {
            var service = this.CalculatorRunService;
            var result = service.IsOrganisationPipeline(pipelineName);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetAzureSynapseConfiguration_ShouldUseCorrectCalendarYearMethod_ForOrgPipeline()
        {
            // Arrange
            var service = this.CalculatorRunService;
            var financialYear = new FinancialYear("2025-26");
            var args = new CalculatorRunParameter
            {
                Id = 1,
                FinancialYear = financialYear,
                User = "user",
                MessageType = "Result"
            };
            var orgPipelineName = "org-data-pipeline";
            var configMock = new Mock<IConfigurationService>();
            configMock.SetupGet(c => c.PipelineUrl).Returns("http://test.com");
            configMock.SetupGet(c => c.CheckInterval).Returns(1);
            configMock.SetupGet(c => c.MaxCheckCount).Returns(1);

            var testService = new CalculatorRunService(
                this.AzureSynapseRunner.Object,
                this.MockLogger.Object,
                this.TransposeService.Object,
                configMock.Object,
                this.PrepareCalcService.Object,
                this.StatusService.Object);

            // Act
            var result = testService.GetAzureSynapseConfiguration(args, orgPipelineName);

            // Assert
            Assert.AreEqual((CalendarYear)"2025", result.CalendarYear);
        }

        [TestMethod]
        public void GetAzureSynapseConfiguration_ShouldUseCorrectCalendarYearMethod_ForNonOrgPipeline()
        {
            // Arrange
            var service = this.CalculatorRunService;
            var financialYear = new FinancialYear("2025-26");
            var args = new CalculatorRunParameter
            {
                Id = 1,
                FinancialYear = financialYear,
                User = "user",
                MessageType = "Result"
            };
            var pomPipelineName = "pom-data-pipeline";
            var configMock = new Mock<IConfigurationService>();
            configMock.SetupGet(c => c.PipelineUrl).Returns("http://test.com");
            configMock.SetupGet(c => c.CheckInterval).Returns(1);
            configMock.SetupGet(c => c.MaxCheckCount).Returns(1);

            var testService = new CalculatorRunService(
                this.AzureSynapseRunner.Object,
                this.MockLogger.Object,
                this.TransposeService.Object,
                configMock.Object,
                this.PrepareCalcService.Object,
                this.StatusService.Object);

            // Act
            var result = testService.GetAzureSynapseConfiguration(args, pomPipelineName);

            // Assert
            Assert.AreEqual((CalendarYear)"2024", result.CalendarYear);
        }

        /// <summary>
        /// A message handler that delays the response in order to test timeouts.
        /// </summary>
        private class DelayedMessageHandler : HttpMessageHandler
        {
            /// <inheritdoc/>
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Thread.Sleep(10);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }
    }
}