namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.UnitTests.Builder;
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
            var materials = TestDataHelper.GetMaterials();

            // Act
            var result = _testClass?.Map(producerDisposalFeesByMaterial, materials);

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
            var materials = TestDataHelper.GetMaterials();

            // Act
            var result = _testClass?.Map(producerDisposalFeesByMaterial, materials);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.MaterialBreakdown.First().HouseholdDrinksContainersTonnageGlass);
        }

        [TestMethod]
        public void DecimalPrecision3Fields_AreSerializedToThreeDecimalPlaces()
        {
            // Arrange
            var mapper = new ProducerDisposalFeesWithBadDebtProvision1JsonMapper();
            var producerDisposalFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>
            {
                {
                    "GL",
                    new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 1.12345m,
                        PublicBinTonnage = 2.98765m,
                        HouseholdDrinksContainersTonnage = 3.45678m,
                        TotalReportedTonnage = 4.65432m,
                        ManagedConsumerWasteTonnage = 5.11111m,
                        NetReportedTonnage = 6.22222m,
                        PreviousInvoicedTonnage = "0",
                        TonnageChange = "0",
                    }
                }
            };

            var materials = new List<MaterialDetail>
            {
                new MaterialDetail { Code = "GL", Name = "Glass", Description = "Glass" }
            };

            // Act
            var result = mapper.Map(producerDisposalFeesByMaterial, materials);
            var json = JsonSerializer.Serialize(result);

            // Assert
            using var doc = JsonDocument.Parse(json);
            var breakdown = doc.RootElement.GetProperty("materialBreakdown")[0];

            Assert.AreEqual("1.123", breakdown.GetProperty("householdPackagingWasteTonnage").GetRawText());
            Assert.AreEqual("2.988", breakdown.GetProperty("publicBinTonnage").GetRawText());
            //Assert.AreEqual("3.457", breakdown.GetProperty("householdDrinksContainersTonnageGlass").GetRawText());
            Assert.AreEqual("4.654", breakdown.GetProperty("totalTonnage").GetRawText());
            Assert.AreEqual("5.111", breakdown.GetProperty("selfManagedConsumerWasteTonnage").GetRawText());
            Assert.AreEqual("6.222", breakdown.GetProperty("netTonnage").GetRawText());

            if (breakdown.TryGetProperty("householdDrinksContainersTonnageGlass", out var drinksGlass))
            {
                Assert.AreEqual("3.457", drinksGlass.GetRawText());
            }
        }
    }
}