namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter.CalcResult
{
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Exporter.JsonExporter.CalcResult;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Globalization;
    using Moq;
    using EPR.Calculator.Service.Function.Mapper;

    [TestClass]
    public class CalculationResultsExporterTests
    {
        private CalculationResultsExporter TestClass { get; init; }

        private IFixture Fixture { get; init; }

        public CalculationResultsExporterTests()
        {
            Fixture = new Fixture();
            this.TestClass = new CalculationResultsExporter(new CommsCostsByMaterialFeesSummary2aMapper());
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var data = Fixture.Create<CalcResultSummary>();

            // Act
            var result = this.TestClass.Export(data, new List<object>(), new List<int>());


            // Assert
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Serialises a <see cref="CalcResultSummary"/>, then parses the resulting JSON
        /// and checks that the values still match up with the original.
        /// </summary>
        [TestMethod]
        public void Export_ValuesAreValid()
        {
            // Arrange
            var data = Fixture.Create<CalcResultSummary>();

            var acceptIds = Fixture.Create<List<int>>();

            // Act
            var json = this.TestClass.Export(data, null, acceptIds);

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["calculationResults"]!
                ["producerCalculationResultsSummary"];


            // Assert
            Assert.IsNotNull(roundTrippedData);

            // 1
            AssertAreEqual(data.TotalFeeforLADisposalCostswoBadDebtprovision1,
                roundTrippedData["feeForLaDisposalCostsWithoutBadDebtprovision1"]);
            AssertAreEqual(data.BadDebtProvisionFor1,
                roundTrippedData["badDebtProvision1"]);
            AssertAreEqual(data.TotalFeeforLADisposalCostswithBadDebtprovision1,
                roundTrippedData["feeForLaDisposalCostsWithBadDebtprovision1"]);

            // 2a
            AssertAreEqual(data.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A,
                roundTrippedData["feeForCommsCostsByMaterialWithoutBadDebtprovision2a"]);
            AssertAreEqual(data.BadDebtProvisionFor2A,
                roundTrippedData["badDebtProvision2a"]);
            AssertAreEqual(data.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A,
                roundTrippedData["feeForCommsCostsByMaterialWitBadDebtprovision2a"]);

            // 2b
            AssertAreEqual(data.CommsCostHeaderWithoutBadDebtFor2bTitle,
                roundTrippedData["feeForCommsCostsUkWideWithoutBadDebtprovision2b"]);
            AssertAreEqual(data.CommsCostHeaderBadDebtProvisionFor2bTitle,
                roundTrippedData["badDebtProvision2b"]);
            AssertAreEqual(data.CommsCostHeaderWithBadDebtFor2bTitle,
                roundTrippedData["feeForCommsCostsUkWideWithBadDebtprovision2b"]);

            // 2c
            AssertAreEqual(data.TwoCCommsCostsByCountryWithoutBadDebtProvision,
                roundTrippedData["feeForCommsCostsByCountryWithoutBadDebtprovision2c"]);
            AssertAreEqual(data.TwoCBadDebtProvision,
                roundTrippedData["badDebtProvision2c"]);
            AssertAreEqual(data.TwoCCommsCostsByCountryWithBadDebtProvision,
                roundTrippedData["feeForCommsCostsByCountryWideWithBadDebtprovision2c"]);

            // 1+2a+2b+2c
            AssertAreEqual(data.TotalOnePlus2A2B2CFeeWithBadDebtProvision,
                roundTrippedData["total12a2b2cWithBadDebt"]);

            // 3
            AssertAreEqual(data.SaOperatingCostsWoTitleSection3,
                roundTrippedData["saOperatingCostsWithoutBadDebtProvision3"]);
            AssertAreEqual(data.BadDebtProvisionTitleSection3,
                roundTrippedData["badDebtProvision3"]);
            AssertAreEqual(data.SaOperatingCostsWithTitleSection3,
                roundTrippedData["saOperatingCostsWithBadDebtProvision3"]);

            // 4
            AssertAreEqual(data.LaDataPrepCostsTitleSection4,
                roundTrippedData["laDataPrepCostsWithoutBadDebtProvision4"]);
            AssertAreEqual(data.LaDataPrepCostsBadDebtProvisionTitleSection4,
                roundTrippedData["badDebtProvision4"]);
            AssertAreEqual(data.LaDataPrepCostsWithBadDebtProvisionTitleSection4,
                roundTrippedData["laDataPrepCostsWithbadDebtProvision4"]);

            // 5
            AssertAreEqual(data.SaSetupCostsTitleSection5,
                roundTrippedData["oneOffFeeSaSetuCostsWithbadDebtProvision5"]);
            AssertAreEqual(data.SaSetupCostsBadDebtProvisionTitleSection5,
                roundTrippedData["badDebtProvision5"]);
            AssertAreEqual(data.SaSetupCostsWithBadDebtProvisionTitleSection5,
                roundTrippedData["oneOffFeeSaSetuCostsWithoutbadDebtProvision5"]);
        }

        private void AssertAreEqual(decimal expected, JsonNode? actual)
        {
            Assert.IsNotNull(actual, "Actual value should not be null.");
            Assert.AreEqual(
                expected.ToString("C", CultureInfo.CreateSpecificCulture("en-GB")),
                actual.GetValue<string>(),
                $"Expected {expected} to be equal to {actual}");
        }


        /// <summary>
        /// Serialises a <see cref="CalcResultSummary"/>, then parses the resulting JSON
        /// and checks that the values still match up with the original.
        /// </summary>
        [TestMethod]
        public void Export_CommsCost2AValues_AreValid()
        {
            var data = Fixture.Create<CalcResultSummary>();

            var acceptIds = new List<int> { 1, 2, 3 };

            for (int i = 1; i <= data.ProducerDisposalFees.Count(); i++)
            {
                data.ProducerDisposalFees.ToList()[i - 1].ProducerId = i.ToString();
            }

            // Act
            var json = this.TestClass.Export(data, null, acceptIds);

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                     ["calculationResults"]!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);
            var twoACosts = roundTrippedData[0]["commsCostsByMaterialFeesSummary2a"];
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !t.isTotalRow && !string.IsNullOrEmpty(t.Level));
            AssertAreEqual(producer.NorthernIrelandTotalWithBadDebtProvision2A, twoACosts["northernIrelandTotalWithBadDebtProvision"]);
            AssertAreEqual(producer.ScotlandTotalWithBadDebtProvision2A, twoACosts["scotlandTotalWithBadDebtProvision"]);
            AssertAreEqual(producer.WalesTotalWithBadDebtProvision2A, twoACosts["walesTotalWithBadDebtProvision"]);
            AssertAreEqual(producer.EnglandTotalWithBadDebtProvision2A, twoACosts["englandTotalWithBadDebtProvision"]);
            AssertAreEqual(producer.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision, twoACosts["totalProducerFeeForCommsCostsWithoutBadDebtProvision2a"]);
            AssertAreEqual(producer.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision, twoACosts["totalProducerFeeForCommsCostsWithBadDebtProvision2a"]);
            AssertAreEqual(producer.BadDebtProvisionFor2A, twoACosts["totalBadDebtProvision"]);

        }
    }
}