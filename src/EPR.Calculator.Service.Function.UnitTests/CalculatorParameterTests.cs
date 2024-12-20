namespace EPR.Calculator.Service.Function.UnitTests
{
    using EPR.Calculator.Service.Function;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="CalculatorParameterTests"/> class.
    /// </summary>
    [TestClass]
    public class CalculatorParameterTests
    {
        /// <summary>
        /// Tests initialisation of the class and it's properties.
        /// </summary>
        [TestMethod]
        public void CanInitialise()
        {
            // Arrange
            var testCalculatorRunId = 1868588173;
            var testFinancialYear = "TestValue1739994975";
            var testCreatedBy = "TestValue873880154";

            // Act
            var testClass = new CalculatorParameter
            {
                CalculatorRunId = testCalculatorRunId,
                FinancialYear = testFinancialYear,
                CreatedBy = testCreatedBy,
            };

            // Assert
            Assert.AreEqual(testCalculatorRunId, testClass.CalculatorRunId);

            Assert.AreEqual(testFinancialYear, testClass.FinancialYear);
            Assert.AreEqual(testCreatedBy, testClass.CreatedBy);
        }
    }
}