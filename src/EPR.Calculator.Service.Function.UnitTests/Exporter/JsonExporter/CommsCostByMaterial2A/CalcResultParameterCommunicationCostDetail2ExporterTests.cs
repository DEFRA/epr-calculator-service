namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter.CommsCostByMaterial2A
{
    using AutoFixture;
    using EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2A;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using static EPR.Calculator.Service.Common.UnitTests.Utils.JsonNodeComparer;

    [TestClass]
    public class CalcResultParameterCommunicationCostDetail2ExporterTests
    {
        private CalcResultParameterCommunicationCostDetail2Exporter TestClass { get; init; }
        private IFixture Fixture { get; init; }

        public CalcResultParameterCommunicationCostDetail2ExporterTests()
        {
            Fixture = new Fixture();
            this.TestClass = new CalcResultParameterCommunicationCostDetail2Exporter();
        }

        [TestMethod]
        public void CanCallConvertToJson()
        {
            // Arrange
            var data = Fixture.Create<CalcResultParameterCommunicationCostDetail2>();

            // Act
            var result = this.TestClass.ConvertToJson(data);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Export_ValuesAreValid()
        {
            // Arrange
            var data = Fixture.Create<CalcResultParameterCommunicationCostDetail2>();

            // Act
            var json = this.TestClass.ConvertToJson(data);

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["calcResult2cCommsDataByCountry"];

            // Assert
            Assert.IsNotNull(roundTrippedData);

            // Disposal Fee
            AssertAreEqual(data.Name,
                roundTrippedData["name"]);
            
            AssertAreEqual(data.England,
                roundTrippedData["englandCommsCostByCountry"]);
            AssertAreEqual(data.Wales,
                roundTrippedData["walesCommsCostByCountry"]);
            AssertAreEqual(data.Scotland,
                roundTrippedData["scotlandCommsCostByCountry"]);
            AssertAreEqual(data.NorthernIreland,
                roundTrippedData["northernIrelandCommsCostByCountry"]);

            AssertAreEqual(data.Total,
                roundTrippedData["totalCommsCostByCountry"]);
        }
    }
}