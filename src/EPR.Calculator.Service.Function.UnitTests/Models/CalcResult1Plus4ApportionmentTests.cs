namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CalcResult1Plus4ApportionmentTests
    {
        private CalcResult1Plus4Apportionment TestClass;
        private IFixture Fixture;

        [TestInitialize]
        public void SetUp()
        {
            Fixture = new Fixture();
            this.TestClass = new CalcResult1Plus4Apportionment();
        }

        [TestMethod]
        public void CanSetAndGetCalcResultParameterCommunicationCostDetails()
        {
            // Arrange
            var testValue = new Mock<IEnumerable<CalcResultParameterCostDetail>>().Object;

            // Act
            this.TestClass.CalcResultParameterCommunicationCostDetails = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.CalcResultParameterCommunicationCostDetails);
        }
    }
}