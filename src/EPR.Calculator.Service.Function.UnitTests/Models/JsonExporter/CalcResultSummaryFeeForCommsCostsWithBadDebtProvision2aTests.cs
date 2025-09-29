namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    using AutoFixture;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Enums;
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
            var totalProducerFeeForCommsCostsWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(testValue);
            // Act
            _testClass.TotalProducerFeeForCommsCostsWithoutBadDebtProvision = totalProducerFeeForCommsCostsWithoutBadDebtProvision;

            // Assert
            Assert.AreEqual(totalProducerFeeForCommsCostsWithoutBadDebtProvision, _testClass.TotalProducerFeeForCommsCostsWithoutBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetBadDebtProvisionFor2a()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();
            var badDebtProvisionFor2a = CurrencyConverter.ConvertToCurrency(testValue);

            // Act
            _testClass.BadDebtProvisionFor2a = badDebtProvisionFor2a;

            // Assert
            Assert.AreEqual(badDebtProvisionFor2a, _testClass.BadDebtProvisionFor2a);
        }

        [TestMethod]
        public void CanSetAndGetTotalProducerFeeForCommsCostsWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();
            var totalProducerFeeForCommsCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(testValue);

            // Act
            _testClass.TotalProducerFeeForCommsCostsWithBadDebtProvision = totalProducerFeeForCommsCostsWithBadDebtProvision;

            // Assert
            Assert.AreEqual(totalProducerFeeForCommsCostsWithBadDebtProvision, _testClass.TotalProducerFeeForCommsCostsWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetEnglandTotalWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();
            var englandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(testValue);

            // Act
            _testClass.EnglandTotalWithBadDebtProvision = englandTotalWithBadDebtProvision;

            // Assert
            Assert.AreEqual(englandTotalWithBadDebtProvision, _testClass.EnglandTotalWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetWalesTotalWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();
            var walesTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(testValue);

            // Act
            _testClass.WalesTotalWithBadDebtProvision = walesTotalWithBadDebtProvision;

            // Assert
            Assert.AreEqual(walesTotalWithBadDebtProvision, _testClass.WalesTotalWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetScotlandTotalWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();
            var scotlandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(testValue);

            // Act
            _testClass.ScotlandTotalWithBadDebtProvision = scotlandTotalWithBadDebtProvision;

            // Assert
            Assert.AreEqual(scotlandTotalWithBadDebtProvision, _testClass.ScotlandTotalWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetNorthernIrelandTotalWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();
            var northernIrelandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(testValue);

            // Act
            _testClass.NorthernIrelandTotalWithBadDebtProvision = northernIrelandTotalWithBadDebtProvision;

            // Assert
            Assert.AreEqual(northernIrelandTotalWithBadDebtProvision, _testClass.NorthernIrelandTotalWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetPercentageOfProducerTonnageVsAllProducers()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<decimal>();
            var percentageOfProducerTonnageVsAllProducers = CsvSanitiser.SanitiseData(testValue, DecimalPlaces.Eight, null, false, true);

            // Act
            _testClass.PercentageOfProducerTonnageVsAllProducers = percentageOfProducerTonnageVsAllProducers;

            // Assert
            Assert.AreEqual(percentageOfProducerTonnageVsAllProducers, _testClass.PercentageOfProducerTonnageVsAllProducers);
        }
    }
}