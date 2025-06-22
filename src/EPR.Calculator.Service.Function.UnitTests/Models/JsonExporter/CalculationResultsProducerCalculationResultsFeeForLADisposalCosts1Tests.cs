namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Models.JsonExporter;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1Tests
    {
        private CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1 _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1();
        }

        [TestMethod]
        public void CanSetAndGetTotalProducerFeeForLADisposalCostsWithoutBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.TotalProducerFeeForLADisposalCostsWithoutBadDebtProvision = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.TotalProducerFeeForLADisposalCostsWithoutBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetBadDebtProvisionForLADisposalCosts()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.BadDebtProvisionForLADisposalCosts = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.BadDebtProvisionForLADisposalCosts);
        }

        [TestMethod]
        public void CanSetAndGetTotalProducerFeeForLADisposalCostsWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.TotalProducerFeeForLADisposalCostsWithBadDebtProvision = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.TotalProducerFeeForLADisposalCostsWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetEnglandTotalForLADisposalCostsWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.EnglandTotalForLADisposalCostsWithBadDebtProvision = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.EnglandTotalForLADisposalCostsWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetWalesTotalForLADisposalCostsWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.WalesTotalForLADisposalCostsWithBadDebtProvision = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.WalesTotalForLADisposalCostsWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetScotlandTotalForLADisposalCostsWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.ScotlandTotalForLADisposalCostsWithBadDebtProvision = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ScotlandTotalForLADisposalCostsWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetNorthernIrelandTotalForLADisposalCostsWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();

            // Act
            _testClass.NorthernIrelandTotalForLADisposalCostsWithBadDebtProvision = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.NorthernIrelandTotalForLADisposalCostsWithBadDebtProvision);
        }
    }
}