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
        /// Tests initialisation of the class.
        /// </summary>
        [TestMethod]
        public void CanInitialise()
        {
            // Arrange
            var testId = 60754953;
            var testUser = "TestValue2015204409";
            var testFinancialYear = "TestValue1106807557";

            // Act
            var testClass = new CalculatorRunParameter
            {
                FinancialYear = testFinancialYear,
                Id = testId,
                User = testUser,
            };

            // Assert
            Assert.AreEqual(testId, testClass.Id);
            Assert.AreEqual(testUser, testClass.User);
            Assert.AreEqual(testFinancialYear, testClass.FinancialYear);
        }
    }
}