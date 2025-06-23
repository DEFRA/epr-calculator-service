using AutoFixture;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2bTests
    {
        private CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2b _testClass;

        public CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2bTests()
        {
            _testClass = new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2b();
        }       

        [TestMethod]
        public void CanSetAndGetTotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision()
        {
            var fixture = new Fixture();
            var testValue = fixture.Create<decimal>();
            _testClass.TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision = testValue;
            Assert.AreEqual(testValue, _testClass.TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetBadDebProvisionFor2b()
        {
            var fixture = new Fixture();
            var testValue = fixture.Create<decimal>();
            _testClass.BadDebtProvisionFor2bComms = testValue;
            Assert.AreEqual(testValue, _testClass.BadDebtProvisionFor2bComms);
        }

        [TestMethod]
        public void CanSetAndGetTotalProducerFeeForCommsCostsUKWideWithBadDebtProvision()
        {
            var fixture = new Fixture();
            var testValue = fixture.Create<decimal>();
            _testClass.TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision = testValue;
            Assert.AreEqual(testValue, _testClass.TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetEnglandTotalWithBadDebtProvision()
        {
            var fixture = new Fixture();
            var testValue = fixture.Create<decimal>();
            _testClass.EnglandTotalWithBadDebtProvision = testValue;
            Assert.AreEqual(testValue, _testClass.EnglandTotalWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetWalesTotalWithBadDebtProvision()
        {
            var fixture = new Fixture();
            var testValue = fixture.Create<decimal>();
            _testClass.WalesTotalWithBadDebtProvision = testValue;
            Assert.AreEqual(testValue, _testClass.WalesTotalWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetScotlandTotalWithBadDebtProvision()
        {
            var fixture = new Fixture();
            var testValue = fixture.Create<decimal>();
            _testClass.ScotlandTotalWithBadDebtProvision = testValue;
            Assert.AreEqual(testValue, _testClass.ScotlandTotalWithBadDebtProvision);
        }

        [TestMethod]
        public void CanSetAndGetNorthernIrelandTotalWithBadDebtProvision()
        {
            var fixture = new Fixture();
            var testValue = fixture.Create<decimal>();
            _testClass.NorthernIrelandTotalWithBadDebtProvision = testValue;
            Assert.AreEqual(testValue, _testClass.NorthernIrelandTotalWithBadDebtProvision);
        }
    }
}