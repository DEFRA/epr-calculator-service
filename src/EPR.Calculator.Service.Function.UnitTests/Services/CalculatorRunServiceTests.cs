using System.Text;
using System.Text.Json;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Services.DataLoading;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Services;

[TestClass]
public class CalculatorRunServiceTests
{
    private IFixture fixture = null!;
    private Mock<IDataLoader> dataLoader = null!;
    private Mock<IPrepareCalcService> prepareCalcService = null!;
    private Mock<IRpdStatusService> statusService = null!;
    private Mock<IProducerDataTransposer> transposeService = null!;
    private CalculatorRunService sut = null!;

    [TestInitialize]
    public void Init()
    {
        fixture = TestFixtures.New();
        dataLoader = fixture.Freeze<Mock<IDataLoader>>();
        transposeService = fixture.Freeze<Mock<IProducerDataTransposer>>();

        prepareCalcService = fixture.Freeze<Mock<IPrepareCalcService>>();
        prepareCalcService.Setup(s => s.PrepareCalcResultsAsync(
                It.IsAny<CalcResultsRequestDto>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        statusService = fixture.Freeze<Mock<IRpdStatusService>>();
        statusService.Setup(s => s.UpdateRpdStatus(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(RunClassification.RUNNING);

        sut = fixture.Create<CalculatorRunService>();
    }

    [TestMethod]
    public async Task StartProcessReturnsFalseWhenCalculatorTimesOut()
    {
        // Arrange
        var calculatorRunParameters = fixture.Create<CalculatorRunParameter>();
        calculatorRunParameters.RelativeYear = new RelativeYear(2024);
        var runName = "Test Run Name";

        statusService.Setup(s => s.UpdateRpdStatus(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Timed out!"));

        // Act
        var result = await sut.PrepareResultsFileAsync(calculatorRunParameters, runName);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task StartProcess_ShouldReturnFalseOn_TaskCanceledException()
    {
        // Arrange
        var calculatorRunParameter = new CalculatorRunParameter
            { Id = 1, User = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = "ignored" };
        var runName = "TestRun";

        transposeService.Setup(t => t.Transpose(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException());

        // Act
        var result = await sut.PrepareResultsFileAsync(calculatorRunParameter, runName);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task StartProcess_ShouldReturnFalseOn_Exception()
    {
        // Arrange
        var calculatorRunParameter = new CalculatorRunParameter
            { Id = 1, User = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = "ignored" };
        var runName = "TestRun";

        transposeService.Setup(t => t.Transpose(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test Exception"));

        // Act
        var result = await sut.PrepareResultsFileAsync(calculatorRunParameter, runName);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task PrepareResultsFileAsync_WhenAllStepsSucceed_ReturnsTrue()
    {
        // Arrange
        var runParams = new CalculatorRunParameter
        {
            Id = 1, User = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = "ignored"
        };

        // Act
        var result = await sut.PrepareResultsFileAsync(runParams, "TestRun");

        // Assert
        Assert.IsTrue(result);
        dataLoader.Verify(d => d.LoadData(runParams, "TestRun", It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task PrepareResultsFileAsync_WhenStatusNotRunning_ReturnsFalse()
    {
        // Arrange
        statusService.Setup(s => s.UpdateRpdStatus(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RunClassification.UNCLASSIFIED);

        var runParams = new CalculatorRunParameter
        {
            Id = 1, User = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = "ignored"
        };

        // Act
        var result = await sut.PrepareResultsFileAsync(runParams, "TestRun");

        // Assert
        Assert.IsFalse(result);
        transposeService.Verify(
            t => t.Transpose(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task PrepareResultsFileAsync_WhenPrepareCalcResultsFails_ReturnsFalse()
    {
        // Arrange
        prepareCalcService.Setup(s => s.PrepareCalcResultsAsync(
                It.IsAny<CalcResultsRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var runParams = new CalculatorRunParameter
        {
            Id = 1, User = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = "ignored"
        };

        // Act
        var result = await sut.PrepareResultsFileAsync(runParams, "TestRun");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task PrepareResultsFileAsync_WhenDataLoaderThrows_ReturnsFalse()
    {
        // Arrange
        dataLoader.Setup(d => d.LoadData(
                It.IsAny<CalculatorRunParameter>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API failure"));

        var runParams = new CalculatorRunParameter
        {
            Id = 1, User = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = "ignored"
        };

        // Act
        var result = await sut.PrepareResultsFileAsync(runParams, "TestRun");

        // Assert
        Assert.IsFalse(result);
        statusService.Verify(
            s => s.UpdateRpdStatus(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task PrepareResultsFileAsync_WhenDataLoaderCancelled_ReturnsFalse()
    {
        // Arrange
        dataLoader.Setup(d => d.LoadData(
                It.IsAny<CalculatorRunParameter>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Cancelled"));

        var runParams = new CalculatorRunParameter
        {
            Id = 1, User = "TestUser", RelativeYear = new RelativeYear(2024), MessageType = "ignored"
        };

        // Act
        var result = await sut.PrepareResultsFileAsync(runParams, "TestRun");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task GetCalcResultMessage_ShouldReturnCorrect_StringContent()
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
        Assert.AreEqual(expectedJson, await result.ReadAsStringAsync());
    }
}
