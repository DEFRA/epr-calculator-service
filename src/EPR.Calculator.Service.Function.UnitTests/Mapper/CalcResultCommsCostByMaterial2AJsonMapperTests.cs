using AutoFixture;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.Builder;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class CalcResultCommsCostByMaterial2AJsonMapperTests
    {
        private CalcResultCommsCostByMaterial2AJsonMapper _testClass;

        [TestInitialize]
        public void Setup()
        {
            _testClass = new CalcResultCommsCostByMaterial2AJsonMapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            // var commsCostByMaterial = fixture.Create<Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>>();
            var commsCostByMaterial = TestDataHelper.GetProducerCommsFeesByMaterial();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var result = ((ICalcResultCommsCostByMaterial2AJsonMapper)_testClass).Map(commsCostByMaterial, materials);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
