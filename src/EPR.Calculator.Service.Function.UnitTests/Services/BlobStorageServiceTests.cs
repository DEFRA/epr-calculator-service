using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using Microsoft.Extensions.Logging;

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
            MockBlobServiceClient = new Mock<BlobServiceClient>();
            MockBlobContainerClient = new Mock<BlobContainerClient>();
            MockBlobClient = new Mock<BlobClient>();

            MockBlobServiceClient.Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(MockBlobContainerClient.Object);

            MockBlobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(MockBlobClient.Object);

            Logger = new Mock<ILogger<BlobStorageService>>();

            BlobStorageService = new BlobStorageService(
                MockBlobServiceClient.Object,
                Logger.Object);
        }

        private Mock<BlobServiceClient> MockBlobServiceClient { get; init; }

        private Mock<BlobContainerClient> MockBlobContainerClient { get; init; }

        private Mock<BlobClient> MockBlobClient { get; init; }

        private BlobStorageService BlobStorageService { get; init; }

        private Mock<ILogger<BlobStorageService>> Logger { get; init; }

        [TestMethod]
        public async Task UploadResultFileContentAsync_ReturnsTrue_WhenUploadSucceeds()
        {
            // Arrange
            var fileName = "test.txt";
            var content = "test content";
            var runName = "test";
            var containerName = TestFixtures.Legacy.Create<string>();
            var expectedUri = new Uri("https://example.com/test.txt");

            var responseMock = new Mock<Response<BlobContentInfo>>();
            MockBlobClient.Setup(x => x.UploadAsync(It.IsAny<BinaryData>(), true, CancellationToken.None))
                          .ReturnsAsync(responseMock.Object);
            MockBlobClient.Setup(x => x.Uri).Returns(expectedUri);

            // Act
            var result = await BlobStorageService.UploadFileAsync(
                (FileName: fileName, 
                Content: content, 
                RunName: runName,
                ContainerName: containerName,
                Overwrite: true));

            // Assert
            Assert.AreEqual(result, expectedUri.ToString());
            MockBlobClient.Verify(x => x.UploadAsync(It.IsAny<BinaryData>(), true, CancellationToken.None), Times.Once);
        }

        [TestMethod]
        public async Task UploadResultFileContentAsync_ShouldThrow_WhenUploadFails()
        {
            // Arrange
            var fileName = "test.txt";
            var content = "test content";
            var runName = "test";
            var containerName = TestFixtures.Legacy.Create<string>();

            MockBlobClient
                .Setup(x => x.UploadAsync(It.IsAny<BinaryData>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Upload failed"));

            await Should.ThrowAsync<Exception>(() => BlobStorageService.UploadFileAsync(
                (FileName: fileName,
                    Content: content,
                    RunName: runName,
                    ContainerName: containerName,
                    Overwrite: true)));
        }
    }
}