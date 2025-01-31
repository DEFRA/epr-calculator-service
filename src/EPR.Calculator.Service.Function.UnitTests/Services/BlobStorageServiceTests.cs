namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    using System.Configuration;
    using Azure;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.Extensions.Configuration;
    using Moq;

    /// <summary>Unit tests for the <see cref="BlobStorageService"/> class.</summary>
    [TestClass]
    public class BlobStorageServiceTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStorageServiceTests"/> class.
        /// </summary>
        public BlobStorageServiceTests()
        {
            this.MockBlobServiceClient = new Mock<BlobServiceClient>();
            this.MockBlobContainerClient = new Mock<BlobContainerClient>();
            this.MockBlobClient = new Mock<BlobClient>();

            var config = new ConfigurationBuilder().AddInMemoryCollection(
                [
                    new KeyValuePair<string, string>(
                        $"{BlobStorageService.BlobStorageSection}:ConnectionString",
                        string.Empty),
                    new KeyValuePair<string, string>(
                        $"{BlobStorageService.BlobStorageSection}:ContainerName",
                        string.Empty),
                    new KeyValuePair<string, string>(
                        $"{BlobStorageService.BlobStorageSection}:CsvFileName",
                        string.Empty),
                ]).Build();

            this.MockBlobServiceClient.Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(this.MockBlobContainerClient.Object);

            this.MockBlobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(this.MockBlobClient.Object);

            this.BlobStorageService = new BlobStorageService(this.MockBlobServiceClient.Object, config);
        }

        private Mock<BlobServiceClient> MockBlobServiceClient { get; init; }

        private Mock<BlobContainerClient> MockBlobContainerClient { get; init; }

        private Mock<BlobClient> MockBlobClient { get; init; }

        private BlobStorageService BlobStorageService { get; init; }

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenBlobStorageSettingsMissing()
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var blobStorageSettings = new BlobStorageSettings { ContainerName = "test-container" };
            configurationSectionMock.Setup(x => x.Value).Returns(blobStorageSettings.ContainerName);
            configurationMock.Setup(x => x.GetSection(BlobStorageService.BlobStorageSection))
                .Returns(configurationSectionMock.Object);

            // Act & Assert is handled by ExpectedException
            Assert.ThrowsException<ConfigurationErrorsException>(() => new BlobStorageService(MockBlobServiceClient.Object, configurationMock.Object));
        }

        [TestMethod]
        public async Task UploadResultFileContentAsync_ReturnsTrue_WhenUploadSucceeds()
        {
            // Arrange
            var fileName = "test.txt";
            var content = "test content";

            var responseMock = new Mock<Response<BlobContentInfo>>();
            this.MockBlobClient.Setup(x => x.UploadAsync(It.IsAny<BinaryData>()))
                          .ReturnsAsync(responseMock.Object);

            // Act
            var result = await this.BlobStorageService.UploadResultFileContentAsync(fileName, content);

            // Assert
            Assert.IsTrue(result);
            this.MockBlobClient.Verify(x => x.UploadAsync(It.IsAny<BinaryData>()), Times.Once);
        }

        [TestMethod]
        public async Task UploadResultFileContentAsync_ShouldReturnFalse_WhenUploadFails()
        {
            // Arrange
            var fileName = "test.txt";
            var content = "test content";

            this.MockBlobClient.Setup(x => x.UploadAsync(It.IsAny<BinaryData>()))
                .ThrowsAsync(new Exception("Upload failed"));

            // Act
            var result = await this.BlobStorageService.UploadResultFileContentAsync(fileName, content);

            // Assert
            Assert.IsFalse(result);
            this.MockBlobClient.Verify(x => x.UploadAsync(It.IsAny<BinaryData>()), Times.Once);
        }
    }
}