namespace EPR.Calculator.Service.Function.UnitTests
{
    using EPR.Calculator.API.Data.Models;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Function;
    using EPR.Calculator.Service.Function.Constants;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="CreateResultFileMessageTests"/> class.
    /// </summary>
    [TestClass]
    public class CreateResultFileMessageTests
    {
        /// <summary>
        /// Tests initialisation of the class and it's properties.
        /// </summary>
        [TestMethod]
        public void CanInitialise()
        {
            // Arrange
            var testCalculatorRunId = 1868588173;
            var testRelativeYear = new RelativeYear(2024);
            var testCreatedBy = "TestValue873880154";

            // Act
            var testClass = new CreateResultFileMessage
            {
                CalculatorRunId = testCalculatorRunId,
                RelativeYear = testRelativeYear,
                CreatedBy = testCreatedBy,
                MessageType = MessageTypes.Result
            };

            // Assert
            Assert.AreEqual(testCalculatorRunId, testClass.CalculatorRunId);

            Assert.AreEqual(testRelativeYear, testClass.RelativeYear);
            Assert.AreEqual(testCreatedBy, testClass.CreatedBy);
        }
    }
}