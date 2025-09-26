using AutoFixture;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Common.Utils;


namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2bTests
    {
        private CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2B _testClass;

        public CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2bTests()
        {
            _testClass = new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2B();
        }       

        [TestMethod]
        public void CanSetAndGetTotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision()
        {
            var fixture = new Fixture();
            var testValue = fixture.Create<decimal>();
            var totalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(testValue);
            _testClass.TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision = totalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision;
            Assert.AreEqual(totalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision, _testClass.TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetBadDebtProvisionFor2b()
        {
            var fixture = new Fixture();
            var testValue = fixture.Create<decimal>();
            var badDebtProvisionFor2bComms = CurrencyConverter.ConvertToCurrency(testValue);
            _testClass.BadDebtProvisionFor2b = badDebtProvisionFor2bComms;
            Assert.AreEqual(badDebtProvisionFor2bComms, _testClass.BadDebtProvisionFor2b);
        }

        [TestMethod]
        public void CanSetAndGetTotalProducerFeeForCommsCostsUKWideWithBadDebtProvision()
        {
            var fixture = new Fixture();
            var testValue = fixture.Create<decimal>();
            var totalProducerFeeForCommsCostsUKWideWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(testValue);
            _testClass.TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision = totalProducerFeeForCommsCostsUKWideWithBadDebtProvision;
            Assert.AreEqual(totalProducerFeeForCommsCostsUKWideWithBadDebtProvision, _testClass.TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetEnglandTotalWithBadDebtProvision()
        {
            var fixture = new Fixture();
            var testValue = fixture.Create<decimal>();
            var englandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(testValue);
            _testClass.EnglandTotalWithBadDebtProvision = englandTotalWithBadDebtProvision;
            Assert.AreEqual(englandTotalWithBadDebtProvision, _testClass.EnglandTotalWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetWalesTotalWithBadDebtProvision()
        {
            var fixture = new Fixture();
            var testValue = fixture.Create<decimal>();
            var walesTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(testValue);
            _testClass.WalesTotalWithBadDebtProvision = walesTotalWithBadDebtProvision;
            Assert.AreEqual(walesTotalWithBadDebtProvision, _testClass.WalesTotalWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetScotlandTotalWithBadDebtProvision()
        {
            var fixture = new Fixture();
            var testValue = fixture.Create<decimal>();
            var scotlandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(testValue);
            _testClass.ScotlandTotalWithBadDebtProvision = scotlandTotalWithBadDebtProvision;
            Assert.AreEqual(scotlandTotalWithBadDebtProvision, _testClass.ScotlandTotalWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetNorthernIrelandTotalWithBadDebtProvision()
        {
            var fixture = new Fixture();
            var testValue = fixture.Create<decimal>();
            var northernIrelandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(testValue);
            _testClass.NorthernIrelandTotalWithBadDebtProvision = northernIrelandTotalWithBadDebtProvision;
            Assert.AreEqual(northernIrelandTotalWithBadDebtProvision, _testClass.NorthernIrelandTotalWithBadDebtProvision);
        }
    }
}