using System.Text;
using System.Text.Json;
using AutoFixture;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Common;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Services.DataLoading;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    /// <summary>
    ///     Contains unit tests for the CalculatorRunService class.
    /// </summary>
    [TestClass]
    public class CalculatorRunServiceTests
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CalculatorRunServiceTests" /> class.
        /// </summary>
        public CalculatorRunServiceTests()
        {
            Fixture = new Fixture();
            MockLogger = new Mock<ICalculatorTelemetryLogger>();
            DataLoader = new Mock<IDataLoader>();
            TransposeService = new Mock<ITransposePomAndOrgDataService>();

            PrepareCalcService = new Mock<IPrepareCalcService>();
            PrepareCalcService.Setup(s => s.PrepareCalcResultsAsync(
                    It.IsAny<CalcResultsRequestDto>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            StatusService = new Mock<IRpdStatusService>();
            StatusService.Setup(s => s.UpdateRpdStatus(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(RunClassification.RUNNING);

            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.StatusUpdateEndpoint,
                Fixture.Create<Uri>().ToString());
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PrepareCalcResultEndPoint,
                Fixture.Create<Uri>().ToString());
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.TransposeEndpoint,
                Fixture.Create<Uri>().ToString());
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.RpdStatusTimeout, "0");

            CalculatorRunService = new CalculatorRunService(
                new Configuration(),
                DataLoader.Object,
                TransposeService.Object,
                PrepareCalcService.Object,
                StatusService.Object,
                MockLogger.Object);

            TransposeService.Setup(t => t.TransposeBeforeResultsFileAsync(
                    It.IsAny<CalcResultsRequestDto>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }

        private CalculatorRunService CalculatorRunService { get; }

        private Mock<ICalculatorTelemetryLogger> MockLogger { get; }

        private Fixture Fixture { get; }

        private Mock<IDataLoader> DataLoader { get; }

        private Mock<ITransposePomAndOrgDataService> TransposeService { get; }

        private Mock<IPrepareCalcService> PrepareCalcService { get; }

        private Mock<IRpdStatusService> StatusService { get; }

        /// <summary>
        ///     Checks that <see cref="CalculatorRunService.PrepareResultsFileAsync(CalculatorRunParameter, string)" /> returns
        ///     false
        ///     when the calculator timed out.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [TestMethod]
        public async Task StartProcessReturnsFalseWhenCalculatorTimesOut()
        {
            // Arrange
            var calculatorRunParameters = Fixture.Create<CalculatorRunParameter>();
            calculatorRunParameters.RelativeYear = new RelativeYear(2024);
            var runName = "Test Run Name";

            StatusService.Setup(s => s.UpdateRpdStatus(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TaskCanceledException("Timed out!"));

            // Act
            var result = await CalculatorRunService.PrepareResultsFileAsync(calculatorRunParameters, runName);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task StartProcess_ShouldReturnFalseOn_TaskCanceledException()
        {
            // Arrange
            var calculatorRunParameter = new CalculatorRunParameter
                { Id = 1, User = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = MessageTypes.Result };
            var runName = "TestRun";

            TransposeService.Setup(t => t.TransposeBeforeResultsFileAsync(It.IsAny<CalcResultsRequestDto>(),
                    It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TaskCanceledException());

            // Act
            var result = await CalculatorRunService.PrepareResultsFileAsync(calculatorRunParameter, runName);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task StartProcess_ShouldReturnFalseOn_Exception()
        {
            // Arrange
            var calculatorRunParameter = new CalculatorRunParameter
                { Id = 1, User = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = MessageTypes.Result };
            var runName = "TestRun";

            TransposeService.Setup(t => t.TransposeBeforeResultsFileAsync(It.IsAny<CalcResultsRequestDto>(),
                    It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await CalculatorRunService.PrepareResultsFileAsync(calculatorRunParameter, runName);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Verifies the full happy path returns true when all steps succeed.
        /// </summary>
        [TestMethod]
        public async Task PrepareResultsFileAsync_WhenAllStepsSucceed_ReturnsTrue()
        {
            // Arrange
            var runParams = new CalculatorRunParameter
            {
                Id = 1, User = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = MessageTypes.Result
            };

            // Act
            var result = await CalculatorRunService.PrepareResultsFileAsync(runParams, "TestRun");

            // Assert
            Assert.IsTrue(result);
            DataLoader.Verify(d => d.LoadData(runParams, "TestRun", It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        ///     Verifies that when the status is not RUNNING, transpose and prepare are not called.
        /// </summary>
        [TestMethod]
        public async Task PrepareResultsFileAsync_WhenStatusNotRunning_ReturnsFalse()
        {
            // Arrange
            StatusService.Setup(s => s.UpdateRpdStatus(
                    It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(RunClassification.UNCLASSIFIED);

            var runParams = new CalculatorRunParameter
            {
                Id = 1, User = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = MessageTypes.Result
            };

            // Act
            var result = await CalculatorRunService.PrepareResultsFileAsync(runParams, "TestRun");

            // Assert
            Assert.IsFalse(result);
            TransposeService.Verify(
                t => t.TransposeBeforeResultsFileAsync(
                    It.IsAny<CalcResultsRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        /// <summary>
        ///     Verifies that when transpose fails, prepare is not called and the result is false.
        /// </summary>
        [TestMethod]
        public async Task PrepareResultsFileAsync_WhenTransposeFails_ReturnsFalse()
        {
            // Arrange
            TransposeService.Setup(t => t.TransposeBeforeResultsFileAsync(
                    It.IsAny<CalcResultsRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var runParams = new CalculatorRunParameter
            {
                Id = 1, User = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = MessageTypes.Result
            };

            // Act
            var result = await CalculatorRunService.PrepareResultsFileAsync(runParams, "TestRun");

            // Assert
            Assert.IsFalse(result);
            PrepareCalcService.Verify(
                p => p.PrepareCalcResultsAsync(
                    It.IsAny<CalcResultsRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        /// <summary>
        ///     Verifies that when PrepareCalcResults fails, the result is false.
        /// </summary>
        [TestMethod]
        public async Task PrepareResultsFileAsync_WhenPrepareCalcResultsFails_ReturnsFalse()
        {
            // Arrange
            PrepareCalcService.Setup(s => s.PrepareCalcResultsAsync(
                    It.IsAny<CalcResultsRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var runParams = new CalculatorRunParameter
            {
                Id = 1, User = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = MessageTypes.Result
            };

            // Act
            var result = await CalculatorRunService.PrepareResultsFileAsync(runParams, "TestRun");

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Verifies that a data loader exception is caught and returns false.
        /// </summary>
        [TestMethod]
        public async Task PrepareResultsFileAsync_WhenDataLoaderThrows_ReturnsFalse()
        {
            // Arrange
            DataLoader.Setup(d => d.LoadData(
                    It.IsAny<CalculatorRunParameter>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("API failure"));

            var runParams = new CalculatorRunParameter
            {
                Id = 1, User = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = MessageTypes.Result
            };

            // Act
            var result = await CalculatorRunService.PrepareResultsFileAsync(runParams, "TestRun");

            // Assert
            Assert.IsFalse(result);
            StatusService.Verify(
                s => s.UpdateRpdStatus(
                    It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        /// <summary>
        ///     Verifies that a data loader TaskCanceledException is caught and returns false.
        /// </summary>
        [TestMethod]
        public async Task PrepareResultsFileAsync_WhenDataLoaderCancelled_ReturnsFalse()
        {
            // Arrange
            DataLoader.Setup(d => d.LoadData(
                    It.IsAny<CalculatorRunParameter>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TaskCanceledException("Cancelled"));

            var runParams = new CalculatorRunParameter
            {
                Id = 1, User = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = MessageTypes.Result
            };

            // Act
            var result = await CalculatorRunService.PrepareResultsFileAsync(runParams, "TestRun");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetCalcResultMessage_ShouldReturnCorrect_StringContent()
        {
            // Arrange
            var calculatorRunId = 123;
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
    }
}