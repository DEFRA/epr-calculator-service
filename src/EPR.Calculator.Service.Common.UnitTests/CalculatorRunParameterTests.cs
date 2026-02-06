namespace EPR.Calculator.Service.Common.UnitTests
{
    using EPR.Calculator.Service.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="CalculatorRunParameter"/> class.
    /// </summary>
    [TestClass]
    public class CalculatorRunParameterTests
    {
        /// <summary>
        /// Tests initialisation of the class and it's properties.
        /// </summary>
        [TestMethod]
        public void CanInitialise()
        {
            // Arrange
            var testId = 60754953;
            var testUser = "TestValue2015204409";
            var testFinancialYear = new FinancialYear("2024-25");
            var testMessageType = "Result";

            // Act
            var calculatorRunParameter = new CalculatorRunParameter
            {
                FinancialYear = testFinancialYear,
                Id = testId,
                User = testUser,
                MessageType = testMessageType
            };

            // Assert
            Assert.AreEqual(testId, calculatorRunParameter.Id);
            Assert.AreEqual(testUser, calculatorRunParameter.User);
            Assert.AreEqual(testFinancialYear, calculatorRunParameter.FinancialYear);
            Assert.AreEqual(testMessageType, calculatorRunParameter.MessageType);
        }
    }
}