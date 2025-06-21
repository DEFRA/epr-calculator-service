using AutoFixture;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    [TestClass]
    public class ParametersOtherMapperTests
    {
        private ParametersOtherMapper TestClass { get; init; }

        public ParametersOtherMapperTests()
        {
            this.TestClass = new ParametersOtherMapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var otherCost = fixture.Create<CalcResultParameterOtherCost>();

            // Act
            var result = ((IParametersOtherMapper)TestClass).Map(otherCost);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ParametersOther);
        }

        [TestMethod]
        public void Map_ReturnsDefaultCountryAmountJson_WhenSaOperatingCostIsEmpty()
        {
            var otherCost = new CalcResultParameterOtherCost
            {
                Name = ParametersOtherMapper.EightTonnageChangeHeader,
                SaOperatingCost = Enumerable.Empty<CalcResultParameterOtherCostDetail>(),
                Details = new List<CalcResultParameterOtherCostDetail>(),
                SchemeSetupCost = new CalcResultParameterOtherCostDetail(),
                BadDebtProvision = new KeyValuePair<string, string>("key", "value"),
                Materiality = new List<CalcResultMateriality>()
            };

            var result = TestClass.Map(otherCost);
            Assert.IsNotNull(result.ParametersOther.ThreeSAOperatingCost);
            Assert.AreEqual(string.Empty, result.ParametersOther.ThreeSAOperatingCost.England);
        }

        [TestMethod]
        public void Map_ReturnsDefaultCountryAmountJson_WhenDetailsIsEmpty()
        {
            var otherCost = new CalcResultParameterOtherCost
            {
                Name = ParametersOtherMapper.EightTonnageChangeHeader,
                SaOperatingCost = new List<CalcResultParameterOtherCostDetail>(),
                Details = new List<CalcResultParameterOtherCostDetail>(),
                SchemeSetupCost = new CalcResultParameterOtherCostDetail(),
                BadDebtProvision = new KeyValuePair<string, string>("key", "value"),
                Materiality = new List<CalcResultMateriality>()
            };

            var result = TestClass.Map(otherCost);
            Assert.IsNotNull(result.ParametersOther.FourLADataPrepCosts);
            Assert.AreEqual(string.Empty, result.ParametersOther.FourLADataPrepCosts.England);
        }

        [TestMethod]
        public void Map_ReturnsDefaultCountryAmountJson_WhenApportionmentDetailNotFound()
        {
            var otherCost = new CalcResultParameterOtherCost
            {
                Name = ParametersOtherMapper.EightTonnageChangeHeader,
                SaOperatingCost = new List<CalcResultParameterOtherCostDetail>(),
                Details = new List<CalcResultParameterOtherCostDetail>(),
                SchemeSetupCost = new CalcResultParameterOtherCostDetail(),
                BadDebtProvision = new KeyValuePair<string, string>("key", "value"),
                Materiality = new List<CalcResultMateriality>()
            };

            var result = TestClass.Map(otherCost);
            Assert.IsNotNull(result.ParametersOther.FourCountryApportionmentPercentages);
            Assert.AreEqual(string.Empty, result.ParametersOther.FourCountryApportionmentPercentages.England);
        }

        [TestMethod]
        public void Map_SplitsMaterialityAndTonnageChangeCorrectly()
        {
            var materiality = new List<CalcResultMateriality>
            {
                new() { SevenMateriality = "Increase", Amount = "£1", Percentage = "10%" },
                new() { SevenMateriality = "Decrease", Amount = "£2", Percentage = "20%" },
                new() { SevenMateriality = "8 Tonnage Change", Amount = "", Percentage = "" },
                new() { SevenMateriality = "Increase", Amount = "£3", Percentage = "30%" },
                new() { SevenMateriality = "Decrease", Amount = "£4", Percentage = "40%" }
            };

            var otherCost = new CalcResultParameterOtherCost
            {
                Name = ParametersOtherMapper.EightTonnageChangeHeader,
                SaOperatingCost = new List<CalcResultParameterOtherCostDetail>(),
                Details = new List<CalcResultParameterOtherCostDetail>(),
                SchemeSetupCost = new CalcResultParameterOtherCostDetail(),
                BadDebtProvision = new KeyValuePair<string, string>("key", "value"),
                Materiality = materiality
            };

            var result = TestClass.Map(otherCost);

            Assert.AreEqual("£1", result.ParametersOther.SevenMateriality.Increase.Amount);
            Assert.AreEqual("£2", result.ParametersOther.SevenMateriality.Decrease.Amount);
            Assert.AreEqual("£3", result.ParametersOther.EightTonnageChange.Increase.Amount);
            Assert.AreEqual("£4", result.ParametersOther.EightTonnageChange.Decrease.Amount);
        }

        [TestMethod]
        public void MapChangeSection_ReturnsDefault_WhenNoIncreaseOrDecrease()
        {
            var section = new List<CalcResultMateriality>
            {
                new() { SevenMateriality = "Other", Amount = "£0", Percentage = "0%" },
                new() { SevenMateriality = "Other", Amount = "£0", Percentage = "0%" }
            };

            var result = typeof(ParametersOtherMapper)
                .GetMethod("MapChangeSection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { section }) as ChangeJson;

            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result.Increase.Amount);
            Assert.AreEqual(string.Empty, result.Increase.Percentage);
            Assert.AreEqual(string.Empty, result.Decrease.Amount);
        }

        [TestMethod]
        public void MapCountryAmount_ReturnsDefault_WhenSourceIsNull()
        {
            var result = typeof(ParametersOtherMapper)
                .GetMethod("MapCountryAmount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { null }) as CountryAmountJson;

            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result.England);
        }

        [TestMethod]
        public void MapChangeDetail_ReturnsDefault_WhenSourceIsNull()
        {
            var result = typeof(ParametersOtherMapper)
                .GetMethod("MapChangeDetail", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { null }) as ChangeDetailJson;

            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result.Amount);
            Assert.AreEqual(string.Empty, result.Percentage);
        }
    }
}