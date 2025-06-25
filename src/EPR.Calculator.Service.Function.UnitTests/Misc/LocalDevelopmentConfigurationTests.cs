namespace EPR.Calculator.Service.Function.UnitTests.Misc
{
    using System;
    using System.Configuration;
    using System.Runtime.CompilerServices;
    using AutoFixture;
    using Castle.Core.Configuration;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Misc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class LocalDevelopmentConfigurationTests
    {
        private LocalDevelopmentConfiguration TestClass { get; init; }

        private IFixture Fixture { get; init; }

        private Mock<Microsoft.Extensions.Configuration.IConfiguration> Configuration { get; init; }

        public LocalDevelopmentConfigurationTests()
        {
            this.Fixture = new Fixture();
            this.Configuration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            this.TestClass = new LocalDevelopmentConfiguration(this.Configuration.Object);
        }

        [TestMethod]
        public void CanGetCheckInterval()
        {
            // Arrange
            var checkIntervalValue = this.Fixture.Create<int>();
            var checkIntervalSection = new Mock<IConfigurationSection>();
            checkIntervalSection.Setup(v => v.Value).Returns(checkIntervalValue.ToString());
            this.Configuration.Setup(c => c.GetSection(nameof(LocalDevelopmentConfiguration.CheckInterval)))
                .Returns(checkIntervalSection.Object);

            // Assert
            Assert.IsInstanceOfType(this.TestClass.CheckInterval, typeof(int));
        }

        [TestMethod]
        public void CanGetDbConnectionString()
        {
            // Arrange
            var blobConnectionValue = this.Fixture.Create<string>();
            var blobConnectionSection = new Mock<IConfigurationSection>();
            blobConnectionSection.Setup(v => v.Value).Returns(blobConnectionValue);
            this.Configuration.Setup(c => c.GetSection("DbConnectionString"))
                .Returns(blobConnectionSection.Object);

            // Assert
            Assert.IsInstanceOfType(this.TestClass.DbConnectionString, typeof(string));
        }

        [TestMethod]
        public void CanGetExecuteRPDPipeline()
        {
            // Arrange
            var executeRPDPipelineValue = this.Fixture.Create<bool>();
            var rPDPipelineSection = new Mock<IConfigurationSection>();
            rPDPipelineSection.Setup(v => v.Value).Returns(executeRPDPipelineValue.ToString());
            this.Configuration.Setup(c => c.GetSection(nameof(LocalDevelopmentConfiguration.ExecuteRPDPipeline)))
                .Returns(rPDPipelineSection.Object);

            // Assert
            Assert.IsInstanceOfType(this.TestClass.ExecuteRPDPipeline, typeof(bool));
        }

        [TestMethod]
        public void CanGetMaxCheckCount()
        {
            // Arrange
            var maxCheckCountValue = this.Fixture.Create<int>();
            var maxCheckCountSection = new Mock<IConfigurationSection>();
            maxCheckCountSection.Setup(v => v.Value).Returns(maxCheckCountValue.ToString());
            this.Configuration.Setup(c => c.GetSection(nameof(LocalDevelopmentConfiguration.MaxCheckCount)))
                .Returns(maxCheckCountSection.Object);

            // Assert
            Assert.IsInstanceOfType(this.TestClass.MaxCheckCount, typeof(int));
        }

        [TestMethod]
        public void CanGetOrgDataPipelineName()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.OrgDataPipelineName, typeof(string));
        }

        [TestMethod]
        public void CanGetPipelineUrl()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.PipelineUrl, typeof(string));
        }

        [TestMethod]
        public void CanGetPomDataPipelineName()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.PomDataPipelineName, typeof(string));
        }

        [TestMethod]
        public void CanGetPrepareCalcResultEndPoint()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.PrepareCalcResultEndPoint, typeof(Uri));
        }

        [TestMethod]
        public void CanGetPrepareCalcResultsTimeout()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.PrepareCalcResultsTimeout, typeof(TimeSpan));
        }

        [TestMethod]
        public void CanGetRpdStatusTimeout()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.RpdStatusTimeout, typeof(TimeSpan));
        }

        [TestMethod]
        public void CanGetStatusEndpoint()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.StatusEndpoint, typeof(Uri));
        }

        [TestMethod]
        public void CanGetTransposeEndpoint()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.TransposeEndpoint, typeof(Uri));
        }

        [TestMethod]
        public void CanGetTransposeTimeout()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.TransposeTimeout, typeof(TimeSpan));
        }

        [TestMethod]
        public void CanGetBlobConnectionString()
        {
            // Arrange
            var blobConnectionValue = this.Fixture.Create<string>();
            var blobConnectionSection = new Mock<IConfigurationSection>();
            blobConnectionSection.Setup(v => v.Value).Returns(blobConnectionValue);
            this.Configuration.Setup(c => c.GetSection("BlobConnectionString"))
                .Returns(blobConnectionSection.Object);

            // Assert
            Assert.IsInstanceOfType(this.TestClass.BlobConnectionString, typeof(string));
        }

        [TestMethod]
        public void CanGetDbLoadingChunkSize()
        {
            // Arrange
            var dbLoadingChunkSize = this.Fixture.Create<int>();
            var blobConnectionSection = new Mock<IConfigurationSection>();
            blobConnectionSection.Setup(v => v.Value).Returns(dbLoadingChunkSize.ToString());
            this.Configuration.Setup(c => c.GetSection(EnvironmentVariableKeys.DbLoadingChunkSize))
                .Returns(blobConnectionSection.Object);

            // Assert
            Assert.IsInstanceOfType(this.TestClass.DbLoadingChunkSize, typeof(int));
        }

        [TestMethod]
        public void ResultFileCSVContainerName()
        {
            // Arrange
            var expectedResult = this.Fixture.Create<string>();

            var resultFileNameSection = new Mock<IConfigurationSection>();
            resultFileNameSection.Setup(v => v.Value).Returns(expectedResult);

            this.Configuration
                .Setup(c => c.GetSection(nameof(LocalDevelopmentConfiguration.BillingFileCSVBlobContainerName)))
                .Returns(resultFileNameSection.Object);

            // Assert
            Assert.IsInstanceOfType(this.TestClass.BillingFileCSVBlobContainerName, typeof(string));
        }
    }
}