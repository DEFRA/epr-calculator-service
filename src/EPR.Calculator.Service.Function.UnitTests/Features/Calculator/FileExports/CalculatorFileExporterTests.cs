using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Features.Calculator.FileExports;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Options;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Calculator.FileExports;

[TestCategory(TestCategories.CalculatorRuns)]
[TestClass]
public class CalculatorFileExporterTests
{
    private Mock<IOptions<BlobStorageOptions>> _blobOptions = null!;
    private CalcResult _calcResult = null!;
    private Mock<IResultsFileCsvWriter> _csvWriter = null!;
    private CalculatorRunContext _runContext = null!;
    private Mock<IStorageService> _storageService = null!;
    private CalculatorFileExporter _sut = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        var fixture = TestFixtures.New();
        _blobOptions = fixture.Freeze<Mock<IOptions<BlobStorageOptions>>>();
        _blobOptions.Setup(m => m.Value).Returns(new BlobStorageOptions
        {
            ResultFileCsvContainer = "results-container"
        });

        _csvWriter = fixture.Freeze<Mock<IResultsFileCsvWriter>>();
        _csvWriter.Setup(m => m.WriteToString(
                It.IsAny<CalculatorRunContext>(), It.IsAny<CalcResult>()))
            .Returns("results-content");

        _storageService = fixture.Freeze<Mock<IStorageService>>();
        _storageService.Setup(m => m.UploadFileAsync(
                It.Is<(string, string, string, string, bool)>(a => a.Item4 == "results-container"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://results.uri");

        _runContext = fixture.Create<CalculatorRunContext>();
        _calcResult = fixture.Create<CalcResult>();

        _sut = fixture.Create<CalculatorFileExporter>();
    }

    [TestMethod]
    public async Task Should_upload_csv_to_correct_container()
    {
        // Act
        await _sut.Export(_runContext, _calcResult, CancellationToken.None);

        // Assert
        _storageService.Verify(x => x.UploadFileAsync(
            It.Is<(string FileName, string Content, string RunName, string ContainerName, bool Overwrite)>(args =>
                args.Content == "results-content"
                && args.RunName == _runContext.RunName
                && args.ContainerName == "results-container"
                && !args.Overwrite),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Should_return_csv_metadata_with_correct_values()
    {
        // Act
        var result = await _sut.Export(_runContext, _calcResult, CancellationToken.None);

        // Assert
        result.CsvMetadata.CalculatorRunId.ShouldBe(_runContext.RunId);
        result.CsvMetadata.FileName.ShouldNotBeNull();
        result.CsvMetadata.BlobUri.ShouldBe("https://results.uri");
        result.CsvMetadata.FileName.ShouldContain("Results File");
        result.CsvMetadata.FileName.ShouldEndWith(".csv");
    }
}