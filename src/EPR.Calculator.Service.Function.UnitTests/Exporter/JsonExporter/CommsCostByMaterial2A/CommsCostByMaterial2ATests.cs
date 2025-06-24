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
    using EPR.Calculator.Service.Function.Constants;

    [TestClass]
    public class CommsCostByMaterial2ATests
    {
        private CommsCostByMaterial2AExporter? _testClass;

        [TestInitialize]
        public void SetUp()
        {
           
            _testClass = new CommsCostByMaterial2AExporter(new CalcResult2ACommsDataByMaterialMapper());
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CommsCostByMaterial2AExporter(new CalcResult2ACommsDataByMaterialMapper());

            // Assert
            Assert.IsNotNull(instance);
        }

        
        [TestMethod]
        public void CommsCostByMaterial()
        {
            List<CalcResultCommsCostCommsCostByMaterial> commsCostByMaterial = GetCommsCostMaterialData();
            // Act
            var result = _testClass?.Export(commsCostByMaterial) ?? string.Empty;

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
            AssertAreEqual(expected.CommsCostByMaterialPricePerTonneValue, actual?["commsCostByMaterialPricePerTonne"]);
            AssertAreEqual(expected.EnglandValue, actual?["englandCommsCost"]);
            AssertAreEqual(expected.WalesValue, actual?["walesCommsCost"]);
            AssertAreEqual(expected.ScotlandValue, actual?["scotlandCommsCost"]);
            AssertAreEqual(expected.NorthernIrelandValue, actual?["northernIrelandCommsCost"]);
            AssertAreEqual(expected.TotalValue, actual?["totalCommsCost"]);
            Assert.AreEqual(expected.LateReportingTonnageValue, actual?["lateReportingTonnage"]?.GetValue<decimal>());
        }


        [TestMethod]
        public void CommsCostByMaterialTotal()
        {
            List<CalcResultCommsCostCommsCostByMaterial> commsCostByMaterial = GetCommsCostMaterialData();
            // Act
            var result = _testClass?.Export(commsCostByMaterial) ?? string.Empty;

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(result)!
                ["calcResult2aCommsDataDetailsTotal"];

            var expected = commsCostByMaterial.Single(t=>t.Name== CommonConstants.Total);
            var actual = roundTrippedData!;

            Assert.IsNotNull(result);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.ProducerReportedHouseholdPackagingWasteTonnageValue, actual?["producerHouseholdPackagingWasteTonnageTotal"]?.GetValue<decimal>());
            Assert.AreEqual(expected.ReportedPublicBinTonnageValue, actual?["publicBinTonnage"]?.GetValue<decimal>());
            Assert.AreEqual(expected.ProducerReportedTotalTonnage, actual?["totalTonnageTotal"]?.GetValue<decimal>());
            Assert.AreEqual(expected.HouseholdDrinksContainersValue, actual?["householdDrinksContainersTonnageTotal"]?.GetValue<decimal>());
            AssertAreEqual(expected.EnglandValue, actual?["englandCommsCostTotal"]);
            AssertAreEqual(expected.WalesValue, actual?["walesCommsCostTotal"]);
            AssertAreEqual(expected.ScotlandValue, actual?["scotlandCommsCostTotal"]);
            AssertAreEqual(expected.NorthernIrelandValue, actual?["northernIrelandCommsCostTotal"]);
            AssertAreEqual(expected.TotalValue, actual?["totalCommsCostTotal"]);
            Assert.AreEqual(expected.LateReportingTonnageValue, actual?["lateReportingTonnageTotal"]?.GetValue<decimal>());
        }

        private static List<CalcResultCommsCostCommsCostByMaterial> GetCommsCostMaterialData()
        {
            // Arrange           
            return new List<CalcResultCommsCostCommsCostByMaterial>
                    {
                        new CalcResultCommsCostCommsCostByMaterial
                        {
                            CommsCostByMaterialPricePerTonne = "0.42",
                            Name = "Aluminium",
                            ProducerReportedHouseholdPackagingWasteTonnageValue = 100.25m,
                            CommsCostByMaterialPricePerTonneValue = 100,
                            EnglandValue = 100,
                            ScotlandValue = 200,
                            WalesValue = 300,
                            NorthernIrelandValue = 400,
                            HouseholdDrinksContainersValue = 100.85m,
                            ProducerReportedTotalTonnage = 100.67m,
                            ReportedPublicBinTonnageValue =100.55m,
                            TotalValue = 1000,
                            LateReportingTonnageValue =100
                        },
                        new CalcResultCommsCostCommsCostByMaterial
                        {
                            CommsCostByMaterialPricePerTonne = "0.3",
                            Name = "Glass",
                            ProducerReportedHouseholdPackagingWasteTonnageValue = 200,
                            CommsCostByMaterialPricePerTonneValue = 200,
                            EnglandValue = 100,
                            ScotlandValue = 200,
                            WalesValue = 300,
                            NorthernIrelandValue = 400,
                            HouseholdDrinksContainersValue = 100,
                            ProducerReportedTotalTonnage = 100,
                            ReportedPublicBinTonnageValue =100,
                            TotalValue = 1000,
                            LateReportingTonnageValue =100
                        },
                         new CalcResultCommsCostCommsCostByMaterial
                        {
                            CommsCostByMaterialPricePerTonne = "0.72",
                            Name = "Total",
                            ProducerReportedHouseholdPackagingWasteTonnageValue = 300,
                            CommsCostByMaterialPricePerTonneValue = 300,
                            EnglandValue = 200,
                            ScotlandValue = 400,
                            WalesValue = 600,
                            NorthernIrelandValue = 800,
                            HouseholdDrinksContainersValue = 200,
                            ProducerReportedTotalTonnage = 200,
                            ReportedPublicBinTonnageValue =200,
                            TotalValue = 2000,
                            LateReportingTonnageValue =200
                        },
            };
        }
    }
}