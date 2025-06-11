namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter.CalcResult
{
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Exporter.JsonExporter.CalcResult;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Globalization;

    [TestClass]
    public class SummaryExporterTests
    {
        private CalculationResultsExporter TestClass { get; init; }

        private IFixture Fixture { get; init; }

        public SummaryExporterTests()
        {
            Fixture = new Fixture();
            this.TestClass = new CalculationResultsExporter();
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var data = Fixture.Create<CalcResultSummary>();

            // Act
            var result = this.TestClass.Export(data, new List<object>());


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

            // Act
            var json = this.TestClass.Export(data, null);

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
                expected.ToString("C", CultureInfo.CurrentCulture),
                actual.GetValue<string>(),
                $"Expected {expected} to be equal to {actual}");
        }
    }
}