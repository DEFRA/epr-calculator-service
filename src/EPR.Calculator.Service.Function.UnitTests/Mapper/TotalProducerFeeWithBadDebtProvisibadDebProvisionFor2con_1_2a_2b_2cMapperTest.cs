namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using AutoFixture;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapperTests
    {
        private TotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper? _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new TotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResultSummaryProducerDisposalFees = fixture.Create<CalcResultSummaryProducerDisposalFees>();

            // Act
            var result = _testClass?.Map(calcResultSummaryProducerDisposalFees);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(calcResultSummaryProducerDisposalFees.ProducerTotalOnePlus2A2B2CWithBadDeptProvision, result.TotalFeeWithBadDebtProvision);
            Assert.AreEqual(calcResultSummaryProducerDisposalFees.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, result.ProducerPercentageOfOverallProducerCost);
        }
    }
}