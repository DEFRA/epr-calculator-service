// <copyright file="CalculatorRunServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
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