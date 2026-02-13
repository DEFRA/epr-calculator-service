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
    public class CalcResultCommsCostOnePlusFourApportionmentUKWideTests
    {
        private IFixture Fixture { get; init; }

        public CalcResultCommsCostOnePlusFourApportionmentUKWideTests()
        {
            Fixture = new Fixture();
        }

        [TestMethod]
        public void CanCallFrom_WithValidData()
        {
            // Arrange
            var ukWideData = Fixture.Create<CalcResultCommsCostOnePlusFourApportionment>();
            ukWideData.Name = CalcResultCommsCostBuilder.TwoBCommsCostUkWide;

            // Act
            var result = CalcResultCommsCostOnePlusFourApportionmentUKWide.From(ukWideData);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void From_ValuesAreValid()
        {
            // Arrange
            var ukWideData = Fixture.Create<CalcResultCommsCostOnePlusFourApportionment>();
            ukWideData.Name = CalcResultCommsCostBuilder.TwoBCommsCostUkWide;

            // Act
            var result = CalcResultCommsCostOnePlusFourApportionmentUKWide.From(ukWideData);
            var json = JsonSerializer.Serialize(result);
            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json);

            // Assert
            Assert.IsNotNull(roundTrippedData);

            AssertAreEqual(ukWideData.Name,
                roundTrippedData["name"]);

            AssertAreEqual(ukWideData.England,
                roundTrippedData["englandCommsCostUKWide"]);
            AssertAreEqual(ukWideData.Wales,
                roundTrippedData["walesCommsCostUKWide"]);
            AssertAreEqual(ukWideData.Scotland,
                roundTrippedData["scotlandCommsCostUKWide"]);
            AssertAreEqual(ukWideData.NorthernIreland,
                roundTrippedData["northernIrelandCommsCostUKWide"]);

            AssertAreEqual(ukWideData.Total,
                roundTrippedData["totalCommsCostUKWide"]);
        }

        [TestMethod]
        public void From_WithNullData_ReturnsNull()
        {
            // Act
            var result = CalcResultCommsCostOnePlusFourApportionmentUKWide.From(null);

            // Assert
            Assert.IsNull(result);
        }
    }
}
