namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Models.JsonExporter;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2aTests
    {
        private CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2a _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2a();
        }

        [TestMethod]
        public void CanSetAndGetTotalProducerFeeForCommsCostsWithoutBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.TotalProducerFeeForCommsCostsWithoutBadDebtProvision = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.TotalProducerFeeForCommsCostsWithoutBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetBadDebProvisionFor2a()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.BadDebProvisionFor2a = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.BadDebProvisionFor2a);
        }

        [TestMethod]
        public void CanSetAndGetTotalProducerFeeForCommsCostsWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.TotalProducerFeeForCommsCostsWithBadDebtProvision = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.TotalProducerFeeForCommsCostsWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetEnglandTotalWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.EnglandTotalWithBadDebtProvision = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.EnglandTotalWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetWalesTotalWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.WalesTotalWithBadDebtProvision = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.WalesTotalWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetScotlandTotalWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.ScotlandTotalWithBadDebtProvision = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ScotlandTotalWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetNorthernIrelandTotalWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.NorthernIrelandTotalWithBadDebtProvision = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.NorthernIrelandTotalWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetPercentageOfProducerTonnageVsAllProducers()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.PercentageOfProducerTonnageVsAllProducers = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.PercentageOfProducerTonnageVsAllProducers);
        }
    }
}