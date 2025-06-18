namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using AutoFixture;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProducerDisposalFeesWithBadDebtProvision1JsonMapperTest
    {
        private ProducerDisposalFeesWithBadDebtProvision1JsonMapper? _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new ProducerDisposalFeesWithBadDebtProvision1JsonMapper();
        }

        [TestMethod]
        public void Set_HouseholdDrinksContainersTonnageGlass_ForGlass()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResultSummaryProducerDisposalFeesByMaterial = fixture.Create<CalcResultSummaryProducerDisposalFeesByMaterial>();
            calcResultSummaryProducerDisposalFeesByMaterial.HouseholdDrinksContainersTonnage = 100m;
            var producerDisposalFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>()
                {
                    { MaterialCodes.Glass, calcResultSummaryProducerDisposalFeesByMaterial }
                };

            // Act
            var result = _testClass?.Map(producerDisposalFeesByMaterial);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(producerDisposalFeesByMaterial.First().Value.HouseholdDrinksContainersTonnage, result.MaterialBreakdown.First().HouseholdDrinksContainersTonnageGlass);
        }

        [TestMethod]
        public void DoNotSet_HouseholdDrinksContainersTonnageGlass_ForNonGlass()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResultSummaryProducerDisposalFeesByMaterial = fixture.Create<CalcResultSummaryProducerDisposalFeesByMaterial>();
            var producerDisposalFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>()
                {
                    { MaterialCodes.Aluminium, calcResultSummaryProducerDisposalFeesByMaterial }
                };

            // Act
            var result = _testClass?.Map(producerDisposalFeesByMaterial);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.MaterialBreakdown.First().HouseholdDrinksContainersTonnageGlass);
        }
    }
}