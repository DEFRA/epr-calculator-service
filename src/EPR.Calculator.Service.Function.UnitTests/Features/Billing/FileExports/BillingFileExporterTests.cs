using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Billing.FileExports;
using EPR.Calculator.Service.Function.Features.Billing.FileExports.Json;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Options;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing.FileExports;

[TestCategory(TestCategories.BillingRuns)]
[TestClass]
public class BillingFileExporterTests
{
    private Mock<IOptions<BlobStorageOptions>> _blobOptions = null!;
    private CalcResult _calcResult = null!;
    private Mock<IBillingFileCsvWriter> _csvWriter = null!;
    private Mock<IBillingFileJsonWriter> _jsonWriter = null!;
    private BillingRunContext _runContext = null!;
    private Mock<IStorageService> _storageService = null!;
    private BillingFileExporter _sut = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        var fixture = TestFixtures.New();
        _blobOptions = fixture.Freeze<Mock<IOptions<BlobStorageOptions>>>();
        _blobOptions.Setup(m => m.Value).Returns(new BlobStorageOptions
        {
            BillingFileCsvContainer = "csv-container",
            BillingFileJsonContainer = "json-container"
        });

        _csvWriter = fixture.Freeze<Mock<IBillingFileCsvWriter>>();
        _csvWriter.Setup(m => m.WriteToString(
                It.IsAny<BillingRunContext>(), It.IsAny<CalcResult>()))
            .Returns("csv-content");

        _jsonWriter = fixture.Freeze<Mock<IBillingFileJsonWriter>>();
        _jsonWriter.Setup(m => m.WriteToString(
                It.IsAny<BillingRunContext>(), It.IsAny<CalcResult>()))
            .ReturnsAsync("json-content");

        _storageService = fixture.Freeze<Mock<IStorageService>>();
        _storageService.Setup(m => m.UploadFileAsync(
                It.Is<(string, string, string, string, bool)>(a => a.Item4 == "csv-container"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://csv.uri");
        _storageService.Setup(m => m.UploadFileAsync(
                It.Is<(string, string, string, string, bool)>(a => a.Item4 == "json-container"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://json.uri");

        _runContext = fixture.Create<BillingRunContext>();
        _calcResult = fixture.Create<CalcResult>();

        _sut = fixture.Create<BillingFileExporter>();
    }

    [TestMethod]
    public async Task Should_upload_csv_to_correct_container()
    {
        // Act
        await _sut.Export(_runContext, _calcResult, CancellationToken.None);

        // Assert
        _storageService.Verify(x => x.UploadFileAsync(
            It.Is<(string FileName, string Content, string RunName, string ContainerName, bool Overwrite)>(args =>
                args.Content == "csv-content"
                && args.RunName == _runContext.RunName
                && args.ContainerName == "csv-container"
                && args.Overwrite),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Should_upload_json_to_correct_container()
    {
        // Act
        await _sut.Export(_runContext, _calcResult, CancellationToken.None);

        // Assert
        _storageService.Verify(x => x.UploadFileAsync(
            It.Is<(string FileName, string Content, string RunName, string ContainerName, bool Overwrite)>(args =>
                args.Content == "json-content"
                && args.RunName == _runContext.RunName
                && args.ContainerName == "json-container"
                && args.Overwrite),
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
        result.CsvMetadata.BlobUri.ShouldBe("https://csv.uri");
        result.CsvMetadata.FileName.ShouldContain("Billing");
        result.CsvMetadata.FileName.ShouldEndWith(".csv");
    }

    [TestMethod]
    public async Task Should_return_json_metadata_with_correct_values()
    {
        // Act
        var result = await _sut.Export(_runContext, _calcResult, CancellationToken.None);

        // Assert
        result.JsonMetadata.CalculatorRunId.ShouldBe(_runContext.RunId);
        result.JsonMetadata.BillingFileCreatedBy.ShouldBe(_runContext.User);
        result.JsonMetadata.BillingFileCreatedDate.ShouldBe(_runContext.ProcessingStartedAt.UtcDateTime);
        result.JsonMetadata.BillingCsvFileName.ShouldBe(result.CsvMetadata.FileName);
        result.JsonMetadata.BillingJsonFileName.ShouldNotBeNull();
        result.JsonMetadata.BillingJsonFileName.ShouldContain("Billing");
        result.JsonMetadata.BillingJsonFileName.ShouldEndWith(".json");
    }
}