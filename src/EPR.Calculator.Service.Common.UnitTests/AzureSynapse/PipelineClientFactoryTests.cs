namespace EPR.Calculator.Service.Common.UnitTests.AzureSynapse
{
    using System;
    using Azure.Analytics.Synapse.Artifacts;
    using Azure.Core;
    using EPR.Calculator.Service.Common.AzureSynapse;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Unit tests for the <see cref="PipelineClientFactory"/> class.
    /// </summary>
    [TestClass]
    public class PipelineClientFactoryTests
    {
        private const string FakeAddress = "http://not.a.real.address";

        private PipelineClientFactory TestClass { get; set; } = new PipelineClientFactory();

        /// <summary>
        /// Perform required initialisation before each test.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            this.TestClass = new PipelineClientFactory();
        }

        /// <summary>
        /// Checks that a pipeline client is returned when calling
        /// <see cref="PipelineClientFactory.GetPipelineClient(TokenCredential)"/>.
        /// </summary>
        [TestMethod]
        public void CanCallGetPipelineClient()
        {
            // Arrange
            var tokenCredential = new Mock<TokenCredential>();

            // Act
            var result = this.TestClass.GetPipelineClient(new Uri(FakeAddress), tokenCredential.Object);

            // Assert
            Assert.IsInstanceOfType<PipelineClient>(result);
        }

        /// <summary>
        /// Checks that a pipeline run client is returned when calling
        /// <see cref="PipelineClientFactory.GetPipelineRunClient(TokenCredential)"/>.
        /// </summary>
        [TestMethod]
        public void CanCallGetPipelineRunClient()
        {
            // Arrange
            var tokenCredential = new Mock<TokenCredential>();

            // Act
            var result = this.TestClass.GetPipelineRunClient(new Uri(FakeAddress), tokenCredential.Object);

            // Assert
            Assert.IsInstanceOfType<PipelineRunClient>(result);
        }

        /// <summary>
        /// Checks that an HTTP client is returned when calling
        /// <see cref="PipelineClientFactory.GetHttpClient(Uri)"/>.
        /// </summary>
        [TestMethod]
        public void CanCallGetHttpClient()
        {
            // Arrange

            // Act
            var result = this.TestClass.GetHttpClient(new Uri(FakeAddress));

            // Assert
            Assert.IsInstanceOfType<HttpClient>(result);
        }
    }
}