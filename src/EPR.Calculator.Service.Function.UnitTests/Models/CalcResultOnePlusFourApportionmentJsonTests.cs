namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultOnePlusFourApportionmentJsonTests
    {
        private CalcResultOnePlusFourApportionmentJson _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new CalcResultOnePlusFourApportionmentJson();
        }

        [TestMethod]
        public void CanSetAndGetOnePlusFourApportionment()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<CalcResultOnePlusFourApportionmentDetailsJson>();

            // Act
            _testClass.OnePlusFourApportionment = testValue;

            // Assert
            Assert.AreSame(testValue, _testClass.OnePlusFourApportionment);
        }
    }

    [TestClass]
    public class CalcResultOnePlusFourApportionmentDetailsJsonTests
    {
        private CalcResultOnePlusFourApportionmentDetailsJson _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new CalcResultOnePlusFourApportionmentDetailsJson();
        }

        [TestMethod]
        public void CanSetAndGetOneFeeForLADisposalCosts()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<CalcResultOnePlusFourApportionmentDetailJson>();

            // Act
            _testClass.OneFeeForLADisposalCosts = testValue;

            // Assert
            Assert.AreSame(testValue, _testClass.OneFeeForLADisposalCosts);
        }

        [TestMethod]
        public void CanSetAndGetFourLADataPrepCharge()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<CalcResultOnePlusFourApportionmentDetailJson>();

            // Act
            _testClass.FourLADataPrepCharge = testValue;

            // Assert
            Assert.AreSame(testValue, _testClass.FourLADataPrepCharge);
        }

        [TestMethod]
        public void CanSetAndGetTotalOfonePlusFour()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<CalcResultOnePlusFourApportionmentDetailJson>();

            // Act
            _testClass.TotalOfonePlusFour = testValue;

            // Assert
            Assert.AreSame(testValue, _testClass.TotalOfonePlusFour);
        }

        [TestMethod]
        public void CanSetAndGetOnePlusFourApportionmentPercentages()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<CalcResultOnePlusFourApportionmentDetailJson>();

            // Act
            _testClass.OnePlusFourApportionmentPercentages = testValue;

            // Assert
            Assert.AreSame(testValue, _testClass.OnePlusFourApportionmentPercentages);
        }
    }
}