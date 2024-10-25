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
    }
}