namespace EPR.Calculator.Service.Common.UnitTests.AzureSynapse
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Threading.Tasks;
    using AutoFixture;
    using Azure;
    using Azure.Analytics.Synapse.Artifacts;
    using Azure.Analytics.Synapse.Artifacts.Models;
    using Azure.Core;
    using EPR.Calculator.Service.Common.AzureSynapse;
    using EPR.Calculator.Service.Common.UnitTests.AutoFixtureCustomisations;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Moq.Protected;

    /// <summary>
    /// Unit tests for the <see cref="AzureSynapseRunner"/> class.
    /// </summary>
    [TestClass]
    public class AzureSynapseRunnerTests
    {
        private const string TestPipelineUrl = "http://not.a.real.address";

        private const string TestPipelineName = "Test Pipeline";

        private const string FakeRunNumber = "33a07ce6-c34e-410f-b878-d0c89fd4f893";

        private const int MaxCheckCount = 3;

        private const int CheckInterval = 1;

        private const int CalculatorRunId = 7;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureSynapseRunnerTests"/> class.
        /// </summary>
        public AzureSynapseRunnerTests()
        {
            this.Fixture = new Fixture();
            this.Fixture.Customizations.Add(new CalendarYearCustomisation());
            this.Fixture.Customizations.Add(new FinancialYearCustomisation());

            // Create a mock client factory to inject the mock pipeline clients into the test class.
            this.MockPipelineClient = new Mock<PipelineClient>();
            this.MockPipelineRunClient = new Mock<PipelineRunClient>();
            this.MockStatusUpdateHandler = new Mock<HttpMessageHandler>();
            this.Mocklogger = new Mock<ILogger<AzureSynapseRunner>>();

            var pipelineClientFactory = new Mock<PipelineClientFactory>();
            pipelineClientFactory.Setup(factory => factory.GetPipelineRunClient(
                It.IsAny<Uri>(),
                It.IsAny<TokenCredential>()))
                .Returns(this.MockPipelineRunClient.Object);
            pipelineClientFactory.Setup(factory => factory.GetPipelineClient(
                It.IsAny<Uri>(),
                It.IsAny<TokenCredential>()))
                .Returns(this.MockPipelineClient.Object);

            // Set up the status update handler to return a default response
            // rather than actualy trying to contact the endpoint.
            this.MockStatusUpdateHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage());

            // Create the class to test.
            this.TestClass = new AzureSynapseRunner(
                pipelineClientFactory.Object, this.Mocklogger.Object);
        }

        private AzureSynapseRunnerParameters Parameters
           => new AzureSynapseRunnerParameters
           {
               CalculatorRunId = CalculatorRunId,
               CalendarYear = this.Fixture.Create<CalendarYear>(),
               CheckInterval = CheckInterval,
               MaxCheckCount = MaxCheckCount,
               PipelineUrl = new Uri(TestPipelineUrl),
               PipelineName = TestPipelineName,
           };

        private Fixture Fixture { get; init; }

        private AzureSynapseRunner TestClass { get; set; }

        private Mock<PipelineClient> MockPipelineClient { get; set; }

        private Mock<PipelineRunClient> MockPipelineRunClient { get; set; }

        private Mock<HttpMessageHandler> MockStatusUpdateHandler { get; set; }

        private Mock<ILogger<AzureSynapseRunner>> Mocklogger { get; set; }

        /// <summary>
        /// Check that the non-test constructor
        /// (The one that automatically initialises a <see cref="PipelineClientFactory"/>)
        /// can be run.
        /// </summary>
        [TestMethod]
        public void CanConstructWithPipelineFactory()
        {
            // Arrange
            // Act
            var result = new AzureSynapseRunner(this.Mocklogger.Object);

            // Assert
            Assert.IsInstanceOfType<AzureSynapseRunner>(result);
        }

        /// <summary>
        /// Check that <see cref="AzureSynapseRunner.Process(string, string, string)"/> returns true when the
        /// pipeline run immediately succeeds.
        /// </summary>
        /// <param name="statusReturned">The status of the pipeline.</param>
        /// <param name="expectedPipelineResult">
        /// The result the method is expected to return for the given pipeline status.
        /// </param>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        [DataRow(nameof(PipelineStatus.Succeeded), true)]
        [DataRow(nameof(PipelineStatus.Failed), false)]
        public async Task CallProcessSucceeds(
            string statusReturned,
            bool expectedPipelineResult)
        {
            // Arrange
            this.MockCreateRunResponse();

            this.MockPipelineRunClient.Setup(client => client.GetPipelineRunAsync(
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(MockPipelineRunResponse(statusReturned)));            
            // Act
            var testdat = new AzureSynapseRunnerParameters
            {
                CalculatorRunId = CalculatorRunId,
                CalendarYear = this.Fixture.Create<CalendarYear>(),
                CheckInterval = CheckInterval,
                MaxCheckCount = MaxCheckCount,
                PipelineUrl = new Uri(TestPipelineUrl),
                PipelineName = TestPipelineName,
            };

            

            var pipelineSucceeded = await this.TestClass.Process(testdat);

            // Assert
            Assert.AreEqual(expectedPipelineResult, pipelineSucceeded);
        }

        /// <summary>
        /// Check that <see cref="AzureSynapseRunner.Process(string, string, string)"/> returns true when
        /// the pipeline takes a long time to complete, and we have to check its status multiple times.
        /// </summary>
        /// <param name="statusReturned">The status of the pipeline.</param>
        /// <param name="expectedPipelineResult">
        /// The result the method is expected to return for the given pipeline status.
        /// </param>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        [DataRow(nameof(PipelineStatus.Succeeded), true)]
        [DataRow(nameof(PipelineStatus.Failed), false)]
        public async Task CallProcessSucceedsWhenPipelineDelayed(
            string statusReturned,
            bool expectedPipelineResult)
        {
            // Arrange
            this.MockCreateRunResponse();

            this.MockPipelineRunClient.SetupSequence(client => client.GetPipelineRunAsync(
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(MockPipelineRunResponse(nameof(PipelineStatus.InProgress))))
                .Returns(Task.FromResult(MockPipelineRunResponse(nameof(PipelineStatus.InProgress))))
                .Returns(Task.FromResult(MockPipelineRunResponse(statusReturned)));

            // Act
            var pipelineSucceeded = await this.TestClass.Process(this.Parameters);

            // Assert
            Assert.AreEqual(expectedPipelineResult, pipelineSucceeded);
        }

        /// <summary>
        /// Check that <see cref="AzureSynapseRunner.Process(string, string, string)"/> returns false when
        /// the pipeline is still running after it's been checked more than the maximum number of times,
        /// even if the pipeline eventually does succeed after that.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task CallProcessFailsWhenPipelineDelayedTooLong()
        {
            // Arrange
            this.MockCreateRunResponse();

            this.MockPipelineRunClient.SetupSequence(client => client.GetPipelineRunAsync(
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(MockPipelineRunResponse(nameof(PipelineStatus.InProgress))))
                .Returns(Task.FromResult(MockPipelineRunResponse(nameof(PipelineStatus.InProgress))))
                .Returns(Task.FromResult(MockPipelineRunResponse(nameof(PipelineStatus.InProgress))))
                .Returns(Task.FromResult(MockPipelineRunResponse(nameof(PipelineStatus.InProgress))));

            // Act
            var pipelineSucceeded = await this.TestClass.Process(this.Parameters);

            // Assert
            Assert.IsFalse(pipelineSucceeded);
        }

        /// <summary>
        /// Check that <see cref="AzureSynapseRunner.Process(string, string, string)"/> returns false when the
        /// pipeline run immediately failed.
        /// </summary>
        /// <param name="statusReturned">The status of the pipeline.</param>
        /// <param name="expectedPipelineResult">
        /// The result the method is expected to return for the given pipeline status.
        /// </param>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        [DataRow(nameof(PipelineStatus.Succeeded), false)]
        [DataRow(nameof(PipelineStatus.Failed), true)]
        public async Task CallProcessFailedReturnFalse(
            string statusReturned,
            bool expectedPipelineResult)
        {
            // Arrange
            this.MockPipelineRunClient.Setup(client => client.GetPipelineRunAsync(
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(MockPipelineRunResponse(statusReturned)));

            // Act
            var pipelineFailed = await this.TestClass.Process(this.Parameters);

            // Assert
            Assert.AreEqual(false, pipelineFailed);
        }

        /// <summary>
        /// Check that <see cref="AzureSynapseRunner.Process(string, string, string)"/> returns the appropriate
        /// response when retrieving the pipeline status initialy throws an exception,
        /// but successfully retrieves the status when retried.
        /// </summary>
        /// <param name="statusReturned">The status of the pipeline.</param>
        /// <param name="expectedResult">
        /// The result the method is expected to return for the given pipeline status.
        /// </param>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        [DataRow(nameof(PipelineStatus.Succeeded), true)]
        [DataRow(nameof(PipelineStatus.Failed), false)]
        public async Task CallProcessSucceedsAfterException(
            string statusReturned,
            bool expectedResult)
        {
            // Arrange
            this.MockCreateRunResponse();

            this.MockPipelineRunClient.SetupSequence(client => client.GetPipelineRunAsync(
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException())
                .Returns(Task.FromResult(MockPipelineRunResponse(statusReturned)));

            // Act
            var pipelineSucceeded = await this.TestClass.Process(this.Parameters);

            // Assert
            Assert.AreEqual(expectedResult, pipelineSucceeded);
        }

        /// <summary>
        /// Check that when <see cref="AzureSynapseRunner.Process(string, string, string)"/> encounters repeated
        /// exceptions retrieving the pipeline status, it rethrows the exception after running out of retries.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task CallProcessFailsAfterRepeatedExceptions()
        {
            // Arrange
            this.MockCreateRunResponse();

            this.MockPipelineRunClient.SetupSequence(client => client.GetPipelineRunAsync(
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException())
                .Throws(new InvalidOperationException())
                .Throws(new InvalidOperationException());

            // Act & Assert
            var pipelineSucceeded = await this.TestClass.Process(this.Parameters);

            // Assert
            Assert.IsFalse(pipelineSucceeded);
        }

        /// <summary>
        /// Builds a mock response to a Synapse request for a pipeline run's status.
        /// </summary>
        private static Response<PipelineRun> MockPipelineRunResponse(string status)
        {
            // We have to use reflection to create the response object, as the class's constructor
            // is internal, and it doesnt use an inverface we could mock instead.
            var ctor = typeof(PipelineRun)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[1];
            var pipelineResult = (PipelineRun)ctor.Invoke(
                [
                FakeRunNumber, // runId
                string.Empty, // runGroupId
                true, // isLatest
                TestPipelineName, // pipelineName
                new Dictionary<string, string>().AsReadOnly(), // parameters
                default(PipelineRunInvokedBy), // invokedBy
                default(DateTimeOffset?), // lastUpdated
                default(DateTimeOffset?), // runStart
                default(DateTimeOffset?), // runEnd
                default(int?), // durationInMs,
                status, // status
                string.Empty, // message
                new Dictionary<string, object>().AsReadOnly(), // additionalProperties
                ]);
            var pipelineRunResponse = new Mock<Response<PipelineRun>>();
            pipelineRunResponse.Setup(r => r.Value).Returns(pipelineResult);
            return pipelineRunResponse.Object;
        }

        /// <summary>
        /// Builds a mock response to a Synapse request for a new pipeline run.
        /// </summary>
        private void MockCreateRunResponse()
        {
            var ctor = typeof(CreateRunResponse)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0];
            var pipelineResult = (CreateRunResponse)ctor.Invoke([FakeRunNumber]);
            var createRunResponse = new Mock<Response<CreateRunResponse>>();
            createRunResponse.Setup(r => r.Value).Returns(pipelineResult);

            this.MockPipelineClient.Setup(client => client.CreatePipelineRunAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<string?>(),
                It.IsAny<IDictionary<string, object>?>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(createRunResponse.Object));
        }
    }
}