namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    using System;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalculatorRunServiceTests
    {
        private CalculatorRunService? calculatorRunService;

        [TestInitialize]
        public void SetUp()
        {
            this.calculatorRunService = new CalculatorRunService();
        }

        [TestMethod]
        public void CanCallStartProcess()
        {
            // Arrange
            var calculatorRunParameter = new CalculatorRunParameter
            {
                Id = 19421039,
                User = "TestValue2066475176",
                FinancialYear = "TestValue974658342",
            };

            // Act
            this.calculatorRunService.StartProcess(calculatorRunParameter);

            // Assert
            Assert.Fail("Create or modify test");
        }
    }
}