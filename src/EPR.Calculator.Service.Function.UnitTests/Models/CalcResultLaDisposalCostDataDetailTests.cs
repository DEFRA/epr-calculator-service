namespace EPR.Calculator.Service.Function.UnitTests.Models
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultLaDisposalCostDataDetailTests
    {
        private CalcResultLaDisposalCostDataDetail _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new CalcResultLaDisposalCostDataDetail
            {
                Name = "Test",
                England = "England",
                Wales = "Wales",
                Scotland = "Scotland",
                NorthernIreland = "NorthernIreland",
                Total = "Total",
                ProducerReportedHouseholdPackagingWasteTonnage = "Total",
                ReportedPublicBinTonnage = "ReportedPublicBinTonnage",
            };
        }

        [TestMethod]
        public void CanSetAndGetName()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.Name = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.Name);
        }

        [TestMethod]
        public void CanSetAndGetMaterial()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.Material = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.Material);
        }

        [TestMethod]
        public void CanSetAndGetEngland()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.England = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.England);
        }

        [TestMethod]
        public void CanSetAndGetWales()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.Wales = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.Wales);
        }

        [TestMethod]
        public void CanSetAndGetScotland()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.Scotland = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.Scotland);
        }

        [TestMethod]
        public void CanSetAndGetNorthernIreland()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.NorthernIreland = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.NorthernIreland);
        }

        [TestMethod]
        public void CanSetAndGetTotal()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.Total = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.Total);
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
        public void CanSetAndGetHouseholdDrinkContainers()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.HouseholdDrinkContainers = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.HouseholdDrinkContainers);
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
        public void CanSetAndGetProducerReportedTotalTonnage()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.ProducerReportedTotalTonnage = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ProducerReportedTotalTonnage);
        }

        [TestMethod]
        public void CanSetAndGetDisposalCostPricePerTonne()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.DisposalCostPricePerTonne = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.DisposalCostPricePerTonne);
        }

        [TestMethod]
        public void CanSetAndGetOrderId()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int>();

            // Act
            _testClass.OrderId = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.OrderId);
        }
    }
}