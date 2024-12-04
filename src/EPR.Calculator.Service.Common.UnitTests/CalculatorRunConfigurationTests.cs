namespace EPR.Calculator.Service.Common.UnitTests
{
    using EPR.Calculator.Service.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Contains unit tests for the CalculatorRunConfiguration class.
    /// </summary>
    [TestClass]
    public class CalculatorRunConfigurationTests
    {
        /// <summary>
        /// Tests the initialization of the CalculatorRunConfiguration class and its properties.
        /// </summary>
        [TestMethod]
        public void CanInitialise()
        {
            // Arrange
            var pipelineUrl = "www.test.com";
            var pipelineName = "TestName";
            var maxCheckCount = "5";
            var checkInterval = "30";

            // Act
            var calculatorRunConfiguration = new CalculatorRunConfiguration
            {
                PipelineUrl = pipelineUrl,
                PipelineName = pipelineName,
                MaxCheckCount = maxCheckCount,
                CheckInterval = checkInterval,
            };

            // Assert
            Assert.AreEqual(pipelineUrl, calculatorRunConfiguration.PipelineUrl);
            Assert.AreEqual(pipelineName, calculatorRunConfiguration.PipelineName);
            Assert.AreEqual(maxCheckCount, calculatorRunConfiguration.MaxCheckCount);
            Assert.AreEqual(checkInterval, calculatorRunConfiguration.CheckInterval);
        }
    }
}