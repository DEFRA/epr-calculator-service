namespace EPR.Calculator.Service.Common.UnitTests.AzureSynapse
{
    using System;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Analytics.Synapse.Artifacts;
    using Azure.Analytics.Synapse.Artifacts.Models;
    using Azure.Core;
    using EPR.Calculator.Service.Common.AzureSynapse;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Unit tests for the <see cref="AzureSynapseRunner"/> class.
    /// </summary>
    [TestClass]
    public class AzureSynapseRunnerTests
    {
        private const string TestPipelineUrl = "http://not.a.real.address";

        private const string TestPipelineName = "Test Pipeline";

        private const string FakeRunNumber = "33a07ce6-c34e-410f-b878-d0c89fd4f893";

        private const int MaxChecks = 3;

        private const int CheckInterval = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureSynapseRunnerTests"/> class.
        /// </summary>
        public AzureSynapseRunnerTests()
        {
            // Create a mock client factory to inject the mock pipeline clients into the test class.
            this.MockPipelineClient = new Mock<PipelineClient>();
            this.MockPipelineRunClient = new Mock<PipelineRunClient>();
            var pipelineClientFactory = new Mock<PipelineClientFactory>();
            pipelineClientFactory.Setup(factory => factory.GetPipelineRunClient(
                It.IsAny<Uri>(),
                It.IsAny<TokenCredential>()))
                .Returns(this.MockPipelineRunClient.Object);
            pipelineClientFactory.Setup(factory => factory.GetPipelineClient(
                It.IsAny<Uri>(),
                It.IsAny<TokenCredential>()))
                .Returns(this.MockPipelineClient.Object);

            // Create the class to test.
            this.TestClass = new AzureSynapseRunner(
                pipelineClientFactory.Object,
                new Uri(TestPipelineUrl),
                TestPipelineName,
                MaxChecks,
                CheckInterval);
        }

        private AzureSynapseRunner TestClass { get; set; }

        private Mock<PipelineClient> MockPipelineClient { get; set; }

        private Mock<PipelineRunClient> MockPipelineRunClient { get; set; }

        private PipelineParameters PipelineParameters { get; } = new PipelineParameters
        {
            CalculationId = "TestValue1255453645",
            FinancialYear = "TestValue166468872",
            UserId = "TestValue213567527",
        };

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
            var result = new AzureSynapseRunner(new Uri(TestPipelineUrl), TestPipelineName, MaxChecks, CheckInterval);

            // Assert
            Assert.IsInstanceOfType<AzureSynapseRunner>(result);
        }

        /// <summary>
        /// Check that <see cref="AzureSynapseRunner.Process(string, string, string)"/> returns true when the
        /// pipeline run immediately succeeds.
        /// </summary>
        /// <param name="statusReturned">The status of the pipeline.</param>
        /// <param name="expectedResult">
        /// The result the method is expected to return for the given pipeline status.
        /// </param>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        [DataRow(nameof(PipelineStatus.Succeeded), true)]
        [DataRow(nameof(PipelineStatus.Failed), false)]
        public async Task CallProcessSucceeds(string statusReturned, bool expectedResult)
        {
            // Arrange
            this.MockCreateRunResponse();

            this.MockPipelineRunClient.Setup(client => client.GetPipelineRunAsync(
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(MockPipelineRunResponse(statusReturned)));

            // Act
            var result = await this.TestClass.Process(this.PipelineParameters);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        /// <summary>
        /// Check that <see cref="AzureSynapseRunner.Process(string, string, string)"/> returns true when
        /// the pipeline takes a long time to complete, and we have to check its status multiple times.
        /// </summary>
        /// <param name="statusReturned">The status of the pipeline.</param>
        /// <param name="expectedResult">
        /// The result the method is expected to return for the given pipeline status.
        /// </param>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        [DataRow(nameof(PipelineStatus.Succeeded), true)]
        [DataRow(nameof(PipelineStatus.Failed), false)]
        public async Task CallProcessSucceedsWhenPipelineDelayed(string statusReturned, bool expectedResult)
        {
            // Arrange
            this.MockCreateRunResponse();

            this.MockPipelineRunClient.SetupSequence(client => client.GetPipelineRunAsync(
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(MockPipelineRunResponse(nameof(PipelineStatus.Running))))
                .Returns(Task.FromResult(MockPipelineRunResponse(nameof(PipelineStatus.Running))))
                .Returns(Task.FromResult(MockPipelineRunResponse(statusReturned)));

            // Act
            var result = await this.TestClass.Process(this.PipelineParameters);

            // Assert
            Assert.AreEqual(expectedResult, result);
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
                .Returns(Task.FromResult(MockPipelineRunResponse(nameof(PipelineStatus.Running))))
                .Returns(Task.FromResult(MockPipelineRunResponse(nameof(PipelineStatus.Running))))
                .Returns(Task.FromResult(MockPipelineRunResponse(nameof(PipelineStatus.Running))))
                .Returns(Task.FromResult(MockPipelineRunResponse(nameof(PipelineStatus.Succeeded))));

            // Act
            var result = await this.TestClass.Process(this.PipelineParameters);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Check that <see cref="AzureSynapseRunner.Process(string, string, string)"/> returns true when
        /// retrieving the pipeline status initialy throws an exception, but succeeds when retried.
        /// </summary>
        /// <param name="statusReturned">The status of the pipeline.</param>
        /// <param name="expectedResult">
        /// The result the method is expected to return for the given pipeline status.
        /// </param>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        [DataRow(nameof(PipelineStatus.Succeeded), true)]
        [DataRow(nameof(PipelineStatus.Failed), false)]
        public async Task CallProcessSucceedsAfterException(string statusReturned, bool expectedResult)
        {
            // Arrange
            this.MockCreateRunResponse();

            this.MockPipelineRunClient.SetupSequence(client => client.GetPipelineRunAsync(
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException())
                .Returns(Task.FromResult(MockPipelineRunResponse(statusReturned)));

            // Act
            var result = await this.TestClass.Process(this.PipelineParameters);

            // Assert
            Assert.AreEqual(expectedResult, result);
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
            Exception? result = null;

            this.MockPipelineRunClient.SetupSequence(client => client.GetPipelineRunAsync(
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException())
                .Throws(new InvalidOperationException())
                .Throws(new InvalidOperationException());

            // Act
            try
            {
                await this.TestClass.Process(this.PipelineParameters);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            Assert.IsInstanceOfType<InvalidOperationException>(result);
        }

        /// <summary>
        /// Check that <see cref="AzureSynapseRunner.Process(string, string, string)"/>
        /// can't be called without a valid calculation ID.
        /// </summary>
        /// <param name="value">The calculation ID to test.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public async Task CannotCallProcessWithInvalidCalculationId(string value)
        {
            Exception? exception = null;
            try
            {
                await this.TestClass.Process(new PipelineParameters
                {
                   CalculationId = value,
                   FinancialYear = "TestValue556375345",
                   UserId = "TestValue58318999",
                });
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.IsTrue(exception is ArgumentException);
        }

        /// <summary>
        /// Check that <see cref="AzureSynapseRunner.Process(string, string, string)"/>
        /// can't be called without a valid financial year.
        /// </summary>
        /// <param name="value">The financial ID to test.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public async Task CannotCallProcessWithInvalidFinancialYear(string value)
        {
            Exception? exception = null;
            try
            {
                await this.TestClass.Process(new PipelineParameters
                {
                    CalculationId = "TestValue313680772",
                    FinancialYear = value,
                    UserId = "TestValue1282357944",
                });
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.IsTrue(exception is ArgumentException);
        }

        /// <summary>
        /// Check that <see cref="AzureSynapseRunner.Process(string, string, string)"/>
        /// can't be called without a valid user ID.
        /// </summary>
        /// <param name="value">The user ID to test.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public async Task CannotCallProcessWithInvalidUserId(string value)
        {
            Exception? exception = null;
            try
            {
                await this.TestClass.Process(new PipelineParameters
                {
                    CalculationId = "TestValue142251981",
                    FinancialYear = "TestValue1761091532",
                    UserId = value,
                });
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.IsTrue(exception is ArgumentException);
        }

        /// <summary>
        /// Builds a mock response to a Synapse request for a pipeline run's status.
        /// </summary>
        private static Response<PipelineRun> MockPipelineRunResponse(string status)
        {
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