namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultCommsCostCommsCostByMaterialTests
    {
        private CalcResultCommsCostCommsCostByMaterial? _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new CalcResultCommsCostCommsCostByMaterial();
        }

        [TestMethod]
        public void CanSetAndGetProducerReportedHouseholdPackagingWasteTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.ProducerReportedHouseholdPackagingWasteTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ProducerReportedHouseholdPackagingWasteTonnage);
        }

        [TestMethod]
        public void CanSetAndGetLateReportingTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.LateReportingTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.LateReportingTonnage);
        }

        [TestMethod]
        public void CanSetAndGetProducerReportedHouseholdPlusLateReportingTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.ProducerReportedHouseholdPlusLateReportingTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ProducerReportedHouseholdPlusLateReportingTonnage);
        }

        [TestMethod]
        public void CanSetAndGetCommsCostByMaterialPricePerTonne()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.CommsCostByMaterialPricePerTonne = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.CommsCostByMaterialPricePerTonne);
        }

        [TestMethod]
        public void CanSetAndGetProducerReportedHouseholdPackagingWasteTonnageValue()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.ProducerReportedHouseholdPackagingWasteTonnageValue = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ProducerReportedHouseholdPackagingWasteTonnageValue);
        }

        [TestMethod]
        public void CanSetAndGetLateReportingTonnageValue()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.LateReportingTonnageValue = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.LateReportingTonnageValue);
        }

        [TestMethod]
        public void CanSetAndGetReportedPublicBinTonnageValue()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.ReportedPublicBinTonnageValue = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ReportedPublicBinTonnageValue);
        }

        [TestMethod]
        public void CanSetAndGetHouseholdDrinksContainersValue()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.HouseholdDrinksContainersValue = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.HouseholdDrinksContainersValue);
        }

        [TestMethod]
        public void CanSetAndGetReportedPublicBinTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.ReportedPublicBinTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ReportedPublicBinTonnage);
        }

        [TestMethod]
        public void CanSetAndGetHouseholdDrinksContainers()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.HouseholdDrinksContainers = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.HouseholdDrinksContainers);
        }

        [TestMethod]
        public void CanSetAndGetProducerReportedTotalTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.ProducerReportedTotalTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ProducerReportedTotalTonnage);
        }

        [TestMethod]
        public void CanSetAndGetTotalReportedTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.TotalReportedTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.TotalReportedTonnage);
        }

        [TestMethod]
        public void CanSetAndGetCommsCostByMaterialPricePerTonneValue()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.CommsCostByMaterialPricePerTonneValue = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.CommsCostByMaterialPricePerTonneValue);
        }
    }
}