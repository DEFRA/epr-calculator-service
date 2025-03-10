namespace EPR.Calculator.Service.Function.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using EPR.Calculator.API.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LocalFileStorageServiceTests
    {
        private LocalFileStorageService _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new LocalFileStorageService();
        }

        [TestMethod]
        public async Task CanCallUploadResultFileContentAsync()
        {
            // Arrange
            var fixture = new Fixture();
            var fileName = fixture.Create<string>();
            var content = fixture.Create<string>();
            var runName = fixture.Create<string>();

            // Act
            var result = await _testClass.UploadResultFileContentAsync(fileName, content, runName);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}