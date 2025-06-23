namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter.CommsCostByMaterial2A
{
    using AutoFixture;
    using EPR.Calculator.Service.Function.Builder.CommsCost;
    using EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2A;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using static EPR.Calculator.Service.Common.UnitTests.Utils.JsonNodeComparer;
    using System.Linq;

    [TestClass]
    public class CalcResultCommsCostOnePlusFourApportionmentExporterTests
    {
        private CalcResultCommsCostOnePlusFourApportionmentExporter TestClass { get; init; }

        private IFixture Fixture { get; init; }

        public CalcResultCommsCostOnePlusFourApportionmentExporterTests()
        {
            Fixture = new Fixture();
            this.TestClass = new CalcResultCommsCostOnePlusFourApportionmentExporter();
        }

        [TestMethod]
        public void CanCallConvertToJson()
        {
            // Arrange
            var (countryData, ukWideData) = BuildTestData();
            var commsCostData = BuildTestCommsCostData(countryData, ukWideData);

            // Act
            var result = this.TestClass.ConvertToJson(commsCostData);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Export_ValuesAreValid()
        {
            // Arrange
            var (countryData, ukWideData) = BuildTestData();
            var commsCostData = BuildTestCommsCostData(countryData, ukWideData);

            // Act
            var json = this.TestClass.ConvertToJson(commsCostData);

            var roundTrippedCountryData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["calcResult2cCommsDataByCountry"];
            var roundTrippedUkWideData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["calcResult2bCommsDataByUkWide"];

            // Assert
            Assert.IsNotNull(roundTrippedCountryData);

            // Disposal Fee
            AssertAreEqual(countryData.Name,
                roundTrippedCountryData["name"]);
            
            AssertAreEqual(countryData.England,
                roundTrippedCountryData["englandCommsCostByCountry"]);
            AssertAreEqual(countryData.Wales,
                roundTrippedCountryData["walesCommsCostByCountry"]);
            AssertAreEqual(countryData.Scotland,
                roundTrippedCountryData["scotlandCommsCostByCountry"]);
            AssertAreEqual(countryData.NorthernIreland,
                roundTrippedCountryData["northernIrelandCommsCostByCountry"]);

            AssertAreEqual(countryData.Total,
                roundTrippedCountryData["totalCommsCostByCountry"]);

            // Assert
            Assert.IsNotNull(roundTrippedUkWideData);

            // Disposal Fee
            AssertAreEqual(ukWideData.Name,
                roundTrippedUkWideData["name"]);

            AssertAreEqual(ukWideData.England,
                roundTrippedUkWideData["englandCommsCostUKWide"]);
            AssertAreEqual(ukWideData.Wales,
                roundTrippedUkWideData["walesCommsCostUKWide"]);
            AssertAreEqual(ukWideData.Scotland,
                roundTrippedUkWideData["scotlandCommsCostUKWide"]);
            AssertAreEqual(ukWideData.NorthernIreland,
                roundTrippedUkWideData["northernIrelandCommsCostUKWide"]);

            AssertAreEqual(ukWideData.Total,
                roundTrippedUkWideData["totalCommsCostUKWide"]);
        }

        private (CalcResultCommsCostOnePlusFourApportionment countryData,
            CalcResultCommsCostOnePlusFourApportionment ukWideData) BuildTestData()
        {
            var byCountryData = Fixture.Create<CalcResultCommsCostOnePlusFourApportionment>();
            byCountryData.Name = CalcResultCommsCostBuilder.TwoCCommsCostByCountry;
            var ukWideData = Fixture.Create<CalcResultCommsCostOnePlusFourApportionment>();
            ukWideData.Name = CalcResultCommsCostBuilder.TwoBCommsCostUkWide;
            return (byCountryData, ukWideData);
        }

        private CalcResultCommsCost BuildTestCommsCostData(
            CalcResultCommsCostOnePlusFourApportionment countryData,
            CalcResultCommsCostOnePlusFourApportionment ukWideData)
        {
            var records = Fixture.CreateMany<CalcResultCommsCostOnePlusFourApportionment>(5).ToList();
            records.Add(countryData);
            records.Add(ukWideData);

            var commsCostData = Fixture.Create<CalcResultCommsCost>();
            commsCostData.CalcResultCommsCostOnePlusFourApportionment = records.OrderBy(x => new Random().Next());
            return commsCostData;
        }
    }
}