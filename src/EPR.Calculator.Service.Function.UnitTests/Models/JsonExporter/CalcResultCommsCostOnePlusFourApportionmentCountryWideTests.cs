namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter.CommsCostByMaterial2A
{
    using AutoFixture;
    using EPR.Calculator.Service.Function.Builder.CommsCost;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Models.JsonExporter;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using static EPR.Calculator.Service.Common.UnitTests.Utils.JsonNodeComparer;

    [TestClass]
    public class CalcResultCommsCostOnePlusFourApportionmentCountryWideTests
    {
        private IFixture Fixture { get; init; }

        public CalcResultCommsCostOnePlusFourApportionmentCountryWideTests()
        {
            Fixture = new Fixture();
        }

        [TestMethod]
        public void CanCallFrom_WithValidData()
        {
            // Arrange
            var countryData = Fixture.Create<CalcResultCommsCostOnePlusFourApportionment>();
            countryData.Name = CalcResultCommsCostBuilder.TwoCCommsCostByCountry;

            // Act
            var result = CalcResultCommsCostOnePlusFourApportionmentCountryWide.From(countryData);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void From_ValuesAreValid()
        {
            // Arrange
            var countryData = Fixture.Create<CalcResultCommsCostOnePlusFourApportionment>();
            countryData.Name = CalcResultCommsCostBuilder.TwoCCommsCostByCountry;

            // Act
            var result = CalcResultCommsCostOnePlusFourApportionmentCountryWide.From(countryData);
            var json = JsonSerializer.Serialize(result);
            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json);

            // Assert
            Assert.IsNotNull(roundTrippedData);

            AssertAreEqual(countryData.Name,
                roundTrippedData["name"]);

            AssertAreEqual(countryData.England,
                roundTrippedData["englandCommsCostByCountry"]);
            AssertAreEqual(countryData.Wales,
                roundTrippedData["walesCommsCostByCountry"]);
            AssertAreEqual(countryData.Scotland,
                roundTrippedData["scotlandCommsCostByCountry"]);
            AssertAreEqual(countryData.NorthernIreland,
                roundTrippedData["northernIrelandCommsCostByCountry"]);

            AssertAreEqual(countryData.Total,
                roundTrippedData["totalCommsCostByCountry"]);
        }

        [TestMethod]
        public void From_WithNullData_ReturnsNull()
        {
            // Act
            var result = CalcResultCommsCostOnePlusFourApportionmentCountryWide.From(null);

            // Assert
            Assert.IsNull(result);
        }
    }
}
