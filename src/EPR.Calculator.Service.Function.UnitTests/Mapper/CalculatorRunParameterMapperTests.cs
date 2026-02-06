// <copyright file="CalculatorRunParameterMapperTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Function;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Mapper;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Contains unit tests for the CalculatorRunParameterMapper class.
    /// </summary>
    [TestClass]
    public class CalculatorRunParameterMapperTests
    {
        private ICalculatorRunParameterMapper? calculatorRunParameterMapper;

        /// <summary>
        /// Initializes the test class by setting up the CalculatorRunParameterMapper instance.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            this.calculatorRunParameterMapper = new CalculatorRunParameterMapper();
        }

        /// <summary>
        /// Tests the Map method of the CalculatorRunParameterMapper class.
        /// </summary>
        [TestMethod]
        public void Calculator_Run_Mapper_Map_Test()
        {
            // Arrange
            var calculatorParameter = new CreateResultFileMessage
            {
                FinancialYear = new FinancialYear("2024-25"),
                CreatedBy = "Test user",
                CalculatorRunId = 678767,
                MessageType = MessageTypes.Result
            };

            // Act
            var result = this.calculatorRunParameterMapper?.Map(calculatorParameter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(calculatorParameter.FinancialYear, result?.FinancialYear);
            Assert.AreEqual(calculatorParameter.CreatedBy, result?.User);
            Assert.AreEqual(calculatorParameter.CalculatorRunId, result?.Id);
        }

        /// <summary>
        /// Tests the Map method of the CalculatorRunParameterMapper class for billing file.
        /// </summary>
        [TestMethod]
        public void Calculator_Run_Mapper_Map_Billing_Test()
        {
            // Arrange
            var billingFileMessage = new CreateBillingFileMessage
            {
                CalculatorRunId = 123,
                ApprovedBy = "Test user",
                MessageType = "Billing",
            };

            // Act
            var result = this.calculatorRunParameterMapper?.Map(billingFileMessage);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(billingFileMessage.CalculatorRunId, result?.Id);
            Assert.AreEqual(billingFileMessage.ApprovedBy, result?.ApprovedBy);
            Assert.AreEqual(billingFileMessage.MessageType, result?.MessageType);
        }
    }
}