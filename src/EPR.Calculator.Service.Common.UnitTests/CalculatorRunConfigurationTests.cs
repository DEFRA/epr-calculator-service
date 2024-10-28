namespace EPR.Calculator.Service.Common.UnitTests
{
    using EPR.Calculator.Service.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalculatorRunConfigurationTests
    {
        /// <summary>
        /// Tests initialisation of the class and it's properties.
        /// </summary>
        [TestMethod]
        public void CanInitialise()
        {
            // Arrange
            var pipelineUrl = "www.test.com";
            var pipeLineName = "TestName";
            var maxCheckCount = "5";
            var checkInterval = "30";

            // Act
            var calculatorRunConfiguration = new CalculatorRunConfiguration
            {
                PipelineUrl = pipelineUrl,
                PipelineName = pipeLineName,
                MaxCheckCount = maxCheckCount,
                CheckInterval = checkInterval,
            };

            // Assert
            Assert.AreEqual(pipelineUrl, calculatorRunConfiguration.PipelineUrl);
            Assert.AreEqual(pipeLineName, calculatorRunConfiguration.PipelineName);
            Assert.AreEqual(maxCheckCount, calculatorRunConfiguration.MaxCheckCount);
            Assert.AreEqual(checkInterval, calculatorRunConfiguration.CheckInterval);
        }
    }
}