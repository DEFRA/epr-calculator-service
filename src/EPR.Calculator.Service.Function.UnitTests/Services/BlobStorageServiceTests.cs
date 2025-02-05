using Castle.Core.Logging;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    using System.Configuration;
    using AutoFixture;
    using Azure;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
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
            this.Fixture = new Fixture();

            this.MockBlobServiceClient = new Mock<BlobServiceClient>();
            this.MockBlobContainerClient = new Mock<BlobContainerClient>();
            this.MockBlobClient = new Mock<BlobClient>();

            this.ConfigurationService = new Mock<IConfigurationService>();
            this.ConfigurationService.Setup(s => s.BlobContainerName)
                .Returns(this.Fixture.Create<string>());

            this.MockBlobServiceClient.Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(this.MockBlobContainerClient.Object);

            this.MockBlobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(this.MockBlobClient.Object);

            this.Logger = new Mock<ILogger<BlobStorageService>>();
            ;
            this.BlobStorageService = new BlobStorageService(
                this.MockBlobServiceClient.Object,
                this.ConfigurationService.Object,
                this.Logger.Object);
        }

        private Fixture Fixture { get; init; }

        private Mock<BlobServiceClient> MockBlobServiceClient { get; init; }

        private Mock<BlobContainerClient> MockBlobContainerClient { get; init; }

        private Mock<BlobClient> MockBlobClient { get; init; }

        private BlobStorageService BlobStorageService { get; init; }

        private Mock<IConfigurationService> ConfigurationService { get; init; }

        private Mock<ILogger<BlobStorageService>> Logger { get; init; }


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