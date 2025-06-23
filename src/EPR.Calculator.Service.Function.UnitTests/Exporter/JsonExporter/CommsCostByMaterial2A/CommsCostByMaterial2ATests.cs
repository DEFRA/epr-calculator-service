namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter.CommsCostByMaterial2A
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2A;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Models.JsonExporter;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static EPR.Calculator.Service.Common.UnitTests.Utils.JsonNodeComparer;
    using Moq;

    [TestClass]
    public class CommsCostByMaterial2ATests
    {
        private CommsCostByMaterial2AExporter? _testClass;
        private ICalcResult2aCommsDataByMaterialMapper? _mapper;

        [TestInitialize]
        public void SetUp()
        {
            _mapper =  new CalcResult2aCommsDataByMaterialMapper();
            _testClass = new CommsCostByMaterial2AExporter(_mapper);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CommsCostByMaterial2AExporter(_mapper);

            // Assert
            Assert.IsNotNull(instance);
        }

        
        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var fixture = new Fixture();
            var commsCostByMaterial = fixture.Create<List<CalcResultCommsCostCommsCostByMaterial>>();

            // Act
            var result = _testClass.Export(commsCostByMaterial);

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(result)!
                ["calcResult2aCommsDataDetails"];

            var expected = commsCostByMaterial.First();
            var actual = roundTrippedData![0]!;

            Assert.IsNotNull(result);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.ProducerReportedHouseholdPackagingWasteTonnageValue, actual?["producerHouseholdPackagingWasteTonnage"]?.GetValue<decimal>());
            Assert.AreEqual(expected.ReportedPublicBinTonnageValue, actual?["publicBinTonnage"]?.GetValue<decimal>());
            Assert.AreEqual(expected.ProducerReportedTotalTonnage, actual?["totalTonnage"]?.GetValue<decimal>());
            Assert.AreEqual(expected.HouseholdDrinksContainersValue, actual?["householdDrinksContainersTonnage"]?.GetValue<decimal>());
            Assert.AreEqual(expected.CommsCostByMaterialPricePerTonneValue, actual?["commsCostByMaterialPricePerTonne"]?.GetValue<decimal>());
            Assert.AreEqual(expected.EnglandValue, actual?["englandCommsCost"]?.GetValue<decimal>());
            Assert.AreEqual(expected.WalesValue, actual?["walesCommsCost"]?.GetValue<decimal>());
            Assert.AreEqual(expected.ScotlandValue, actual?["scotlandCommsCost"]?.GetValue<decimal>());
            Assert.AreEqual(expected.NorthernIrelandValue, actual?["northernIrelandCommsCost"]?.GetValue<decimal>());
            Assert.AreEqual(expected.TotalValue, actual?["totalCommsCost"]?.GetValue<decimal>());
            Assert.AreEqual(expected.LateReportingTonnageValue, actual?["lateReportingTonnage"]?.GetValue<decimal>());
        }       
    }
}