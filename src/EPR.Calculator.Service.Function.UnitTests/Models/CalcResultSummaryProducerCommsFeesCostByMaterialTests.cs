namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultSummaryProducerCommsFeesCostByMaterialTests
    {
        private CalcResultSummaryProducerCommsFeesCostByMaterial _testClass;

        CalcResultSummaryProducerCommsFeesCostByMaterialTests()
        {
            _testClass = new CalcResultSummaryProducerCommsFeesCostByMaterial();
        }

        [TestMethod]
        public void CanSetAndGetHouseholdPackagingWasteTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.HouseholdPackagingWasteTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.HouseholdPackagingWasteTonnage);
        }

        [TestMethod]
        public void CanSetAndGetReportedPublicBinTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.ReportedPublicBinTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ReportedPublicBinTonnage);
        }

        [TestMethod]
        public void CanSetAndGetTotalReportedTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.TotalReportedTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.TotalReportedTonnage);
        }

        [TestMethod]
        public void CanSetAndGetHouseholdDrinksContainers()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.HouseholdDrinksContainers = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.HouseholdDrinksContainers);
        }

        [TestMethod]
        public void CanSetAndGetPriceperTonne()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.PriceperTonne = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.PriceperTonne);
        }

        [TestMethod]
        public void CanSetAndGetProducerTotalCostWithoutBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.ProducerTotalCostWithoutBadDebtProvision = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ProducerTotalCostWithoutBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.BadDebtProvision = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.BadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetProducerTotalCostwithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.ProducerTotalCostwithBadDebtProvision = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ProducerTotalCostwithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetEnglandWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.EnglandWithBadDebtProvision = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.EnglandWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetWalesWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.WalesWithBadDebtProvision = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.WalesWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetScotlandWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.ScotlandWithBadDebtProvision = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ScotlandWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetNorthernIrelandWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.NorthernIrelandWithBadDebtProvision = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.NorthernIrelandWithBadDebtProvision);
        }
    }
}