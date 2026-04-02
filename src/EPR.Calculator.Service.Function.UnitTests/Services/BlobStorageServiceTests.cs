using AutoFixture;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Services;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    /// <summary>Unit tests for the <see cref="BlobStorageService"/> class.</summary>
    [TestClass]
    public class BlobStorageServiceTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStorageServiceTests"/> class.
        /// </summary>
        public BlobStorageServiceTests()
        {
            Fixture = new Fixture();

            MockBlobServiceClient = new Mock<BlobServiceClient>();
            MockBlobContainerClient = new Mock<BlobContainerClient>();
            MockBlobClient = new Mock<BlobClient>();

            ConfigurationService = new Mock<IConfigurationService>();
            ConfigurationService.Setup(s => s.ResultFileCSVContainerName)
                .Returns(Fixture.Create<string>());

            MockBlobServiceClient.Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(MockBlobContainerClient.Object);

            MockBlobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(MockBlobClient.Object);

            TelemetryLogger = new Mock<ICalculatorTelemetryLogger>();

            BlobStorageService = new BlobStorageService(
                MockBlobServiceClient.Object,
                TelemetryLogger.Object);
        }

        private Fixture Fixture { get; init; }

        private Mock<BlobServiceClient> MockBlobServiceClient { get; init; }

        private Mock<BlobContainerClient> MockBlobContainerClient { get; init; }

        private Mock<BlobClient> MockBlobClient { get; init; }

        private BlobStorageService BlobStorageService { get; init; }

        private Mock<IConfigurationService> ConfigurationService { get; init; }

        private Mock<ICalculatorTelemetryLogger> TelemetryLogger { get; init; }

        [TestMethod]
        public async Task UploadResultFileContentAsync_ReturnsTrue_WhenUploadSucceeds()
        {
            // Arrange
            var fileName = "test.txt";
            var content = "test content";
            var runName = "test";
            var containerName = Fixture.Create<string>();
            var expectedUri = new Uri("https://example.com/test.txt");

            var responseMock = new Mock<Response<BlobContentInfo>>();
            MockBlobClient.Setup(x => x.UploadAsync(It.IsAny<BinaryData>(), true, default))
                          .ReturnsAsync(responseMock.Object);
            MockBlobClient.Setup(x => x.Uri).Returns(expectedUri);

            // Act
            var result = await BlobStorageService.UploadFileContentAsync(
                (FileName: fileName, 
                Content: content, 
                RunName: runName,
                ContainerName: containerName,
                Overwrite: true));

            // Assert
            Assert.AreEqual(result, expectedUri.ToString());
            MockBlobClient.Verify(x => x.UploadAsync(It.IsAny<BinaryData>(), true, default), Times.Once);
        }

        [TestMethod]
        public async Task UploadResultFileContentAsync_ShouldReturnFalse_WhenUploadFails()
        {
            // Arrange
            var fileName = "test.txt";
            var content = "test content";
            var runName = "test";
            var containerName = Fixture.Create<string>();

            MockBlobClient.Setup(x => x.UploadAsync(It.IsAny<BinaryData>()))
                .ThrowsAsync(new Exception("Upload failed"));

            // Act
            var result = await BlobStorageService.UploadFileContentAsync(
                (FileName: fileName, 
                Content: content,
                RunName: runName,
                ContainerName: containerName,
                Overwrite: true));

            // Assert
            Assert.AreEqual(result, string.Empty);
            MockBlobClient.Verify(x => x.UploadAsync(It.IsAny<BinaryData>(), true, default), Times.Once);
        }
    }
}