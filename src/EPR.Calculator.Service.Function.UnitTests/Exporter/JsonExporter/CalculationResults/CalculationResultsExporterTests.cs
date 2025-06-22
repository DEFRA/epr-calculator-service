namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter.CalcResult
{
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Exporter.JsonExporter.CalcResult;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static EPR.Calculator.Service.Common.UnitTests.Utils.JsonNodeComparer;

    [TestClass]
    public class CalculationResultsExporterTests
    {
        private CalculationResultsExporter TestClass { get; init; }

        private IFixture Fixture { get; init; }

        public CalculationResultsExporterTests()
        {
            this.Fixture = new Fixture();
            this.TestClass = new CalculationResultsExporter(
                new ProducerDisposalFeesWithBadDebtProvision1JsonMapper(),
                new CommsCostsByMaterialFeesSummary2aMapper(),
                new CalcResultCommsCostByMaterial2AJsonMapper(),
                new SAOperatingCostsWithBadDebtProvisionMapper(),
                new FeeForCommsCostsWithBadDebtProvision2aMapper(),
                new FeeForCommsCostsWithBadDebtProvision2bMapper(),
                new TotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper(),
                new FeeForSASetUpCostsWithBadDebtProvision_5Mapper(),
                new CalcResultCommsCostsWithBadDebtProvision2cMapper(),
                new CalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper(),
                new TotalProducerBillWithBadDebtProvisionMapper(),
                new CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1Mapper()
            );
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var data = SetCalcResultSummayData();

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
            var data = SetCalcResultSummayData();

            // Act
            var json = this.TestClass.Export(data, null, new List<int> { 1, 2, 3 });

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

        [TestMethod]
        public void Export_ProducerDisposalFeesWithBadDebtProvision1_ReturnsValidValues()
        {
            // Arrange
            var data = SetCalcResultSummayData();

            // Act
            var json = this.TestClass.Export(data, null, new List<int> { 1, 2, 3 });

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["calculationResults"]!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);

            var actual = roundTrippedData[0]!["producerDisposalFeesWithBadDebtProvision1"]!["materialBreakdown"]![0]!;
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !t.isTotalRow && !string.IsNullOrEmpty(t.Level))!;
            var expected = producer.ProducerDisposalFeesByMaterial.First();

            Assert.AreEqual(expected.Value.PreviousInvoicedTonnage, actual["previousInvoicedTonnage"]!.ToString());
            AssertAreEqual(expected.Value.HouseholdPackagingWasteTonnage, actual["householdPackagingWasteTonnage"]);
            AssertAreEqual(expected.Value.PublicBinTonnage, actual["publicBinTonnage"]);
            AssertAreEqual(expected.Value.TotalReportedTonnage, actual["totalTonnage"]);
            AssertAreEqual(expected.Value.ManagedConsumerWasteTonnage, actual["selfManagedConsumerWasteTonnage"]);
            AssertAreEqual(expected.Value.NetReportedTonnage, actual["netTonnage"]);
            Assert.AreEqual(expected.Value.TonnageChange, actual["tonnageChange"]!.ToString());
            AssertAreEqual(expected.Value.PricePerTonne, actual["pricePerTonne"]);
            AssertAreEqual(expected.Value.ProducerDisposalFee, actual["producerDisposalFeeWithoutBadDebtProvision"]);
            AssertAreEqual(expected.Value.BadDebtProvision, actual["badDebtProvision"]);
            AssertAreEqual(expected.Value.ProducerDisposalFeeWithBadDebtProvision, actual["producerDisposalFeeWithBadDebtProvision"]);
            AssertAreEqual(expected.Value.EnglandWithBadDebtProvision, actual["englandWithBadDebtProvision"]);
            AssertAreEqual(expected.Value.WalesWithBadDebtProvision, actual["walesWithBadDebtProvision"]);
            AssertAreEqual(expected.Value.ScotlandWithBadDebtProvision, actual["scotlandWithBadDebtProvision"]);
            AssertAreEqual(expected.Value.NorthernIrelandWithBadDebtProvision, actual["northernIrelandWithBadDebtProvision"]);
        }

        [TestMethod]
        public void Export_TotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c_ReturnsValidValues()
        {
            // Arrange
            var data = SetCalcResultSummayData();

            // Act
            var json = this.TestClass.Export(data, null, new List<int> { 1, 2, 3 });

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["calculationResults"]!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);
            var actual = roundTrippedData[0]!["totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c"]!;
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !t.isTotalRow && !string.IsNullOrEmpty(t.Level))!;

            AssertAreEqual(producer.ProducerTotalOnePlus2A2B2CWithBadDeptProvision, actual["totalFeeWithBadDebtProvision"]);
            AssertAreEqual(producer.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, actual["producerPercentageOfOverallProducerCost"]);
        }

        /// <summary>
        /// Serialises a <see cref="CalcResultSummary"/>, then parses the resulting JSON
        /// and checks that the values still match up with the original.
        /// </summary>
        [TestMethod]
        public void Export_CommsCost2AValues_AreValid()
        {
            // Arrange
            var data = SetCalcResultSummayData();

            // Act
            var json = this.TestClass.Export(data, null, new List<int> { 1, 2, 3 });

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

        [TestMethod]
        public void Export_FeeForSASetUpCostsWithBadDebtProvision_5_ReturnsValidValues()
        {
            // Arrange
            var data = SetCalcResultSummayData();

            // Act
            var json = this.TestClass.Export(data, null, new List<int> { 1, 2, 3 });

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["calculationResults"]!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);
            var actual = roundTrippedData[0]!["feeForSASetUpCostsWithBadDebtProvision_5"]!;
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !t.isTotalRow && !string.IsNullOrEmpty(t.Level))!;

            AssertAreEqual(producer.TotalProducerFeeWithoutBadDebtProvisionSection5, actual["totalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision"]);
            AssertAreEqual(producer.BadDebtProvisionSection5, actual["badDebtProvisionFor5"]);
            AssertAreEqual(producer.TotalProducerFeeWithBadDebtProvisionSection5, actual["totalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision"]);
            AssertAreEqual(producer.EnglandTotalWithBadDebtProvisionSection5, actual["englandTotalForSASetUpCostsWithBadDebtProvision"]);
            AssertAreEqual(producer.WalesTotalWithBadDebtProvisionSection5, actual["walesTotalForSASetUpCostsWithBadDebtProvision"]);
            AssertAreEqual(producer.ScotlandTotalWithBadDebtProvisionSection5, actual["scotlandTotalForSASetUpCostsWithBadDebtProvision"]);
            AssertAreEqual(producer.NorthernIrelandTotalWithBadDebtProvisionSection5, actual["northernIrelandTotalForSASetUpCostsWithBadDebtProvision"]);
        }

        /// <summary>
        /// Serialises a <see cref="CalcResultSummary"/>, then parses the resulting JSON
        /// and checks that the values still match up with the original.
        /// </summary>
        [TestMethod]
        public void Export_CommsCost3SA_Operating_Costs_AreValid()
        {
            // Arrange
            var data = SetCalcResultSummayData();

            // Act
            var json = this.TestClass.Export(data, null, new List<int> { 1, 2, 3 });

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                     ["calculationResults"]!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);
            var threeSACosts = roundTrippedData[0]["calcResultSAOperatingCostsWithBadDebtProvision"];
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !t.isTotalRow && !string.IsNullOrEmpty(t.Level));
            AssertAreEqual(producer.NorthernIrelandTotalWithBadDebtProvision3, threeSACosts["northernIrelandTotalForSAOperatingCostsWithBadDebtProvision"]);
            AssertAreEqual(producer.ScotlandTotalWithBadDebtProvision3, threeSACosts["scotlandTotalForSAOperatingCostsWithBadDebtProvision"]);
            AssertAreEqual(producer.WalesTotalWithBadDebtProvision3, threeSACosts["walesTotalForSAOperatingCostsWithBadDebtProvision"]);
            AssertAreEqual(producer.EnglandTotalWithBadDebtProvision3, threeSACosts["englandTotalForSAOperatingCostsWithBadDebtProvision"]);
            AssertAreEqual(producer.Total3SAOperatingCostswithBadDebtprovision, threeSACosts["totalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision"]);
            AssertAreEqual(producer.Total3SAOperatingCostwoBadDebtprovision, threeSACosts["totalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision"]);
            AssertAreEqual(producer.BadDebtProvisionFor3, threeSACosts["badDebProvisionFor3"]);
        }

        /// <summary>
        /// Serialises a <see cref="CalcResultSummary"/>, then parses the resulting JSON
        /// and checks that the values still match up with the original.
        /// </summary>
        [TestMethod]
        public void Export_FeeForCommsCostsWithBadDebtProvision2a_AreValid()
        {
            // Arrange
            var data = SetCalcResultSummayData();

            // Act
            var json = this.TestClass.Export(data, null, new List<int> { 1, 2, 3 });

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                     ["calculationResults"]!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);
            var twoACosts = roundTrippedData[0]["feeForCommsCostsWithBadDebtProvision2a"];
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !t.isTotalRow && !string.IsNullOrEmpty(t.Level));
            AssertAreEqual(producer.NorthernIrelandTotalWithBadDebtProvision2A, twoACosts["northernIrelandTotalWithBadDebtProvision"]);
            AssertAreEqual(producer.ScotlandTotalWithBadDebtProvision2A, twoACosts["scotlandTotalWithBadDebtProvision"]);
            AssertAreEqual(producer.WalesTotalWithBadDebtProvision2A, twoACosts["walesTotalWithBadDebtProvision"]);
            AssertAreEqual(producer.EnglandTotalWithBadDebtProvision2A, twoACosts["englandTotalWithBadDebtProvision"]);
            AssertAreEqual(producer.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision, twoACosts["totalProducerFeeForCommsCostsWithoutBadDebtProvision"]);
            AssertAreEqual(producer.BadDebtProvisionFor2A, twoACosts["badDebProvisionFor2a"]);
            AssertAreEqual(producer.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision, twoACosts["totalProducerFeeForCommsCostsWithBadDebtProvision"]);
        }

        [TestMethod]
        public void Export_FeeForCommsCostsWithBadDebtProvision2b_AreValid()
        {
            // Arrange
            var data = SetCalcResultSummayData();

            // Act
            var json = this.TestClass.Export(data, null, new List<int> { 1, 2, 3 });

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["calculationResults"]!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);
            var twoBCosts = roundTrippedData[0]["feeForCommsCostsWithBadDebtProvision2b"];
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !t.isTotalRow && !string.IsNullOrEmpty(t.Level));
            AssertAreEqual(producer.NorthernIrelandTotalWithBadDebtFor2bComms, twoBCosts["northernIrelandTotalWithBadDebtProvision"]);
            AssertAreEqual(producer.ScotlandTotalWithBadDebtFor2bComms, twoBCosts["scotlandTotalWithBadDebtProvision"]);
            AssertAreEqual(producer.WalesTotalWithBadDebtFor2bComms, twoBCosts["walesTotalWithBadDebtProvision"]);
            AssertAreEqual(producer.EnglandTotalWithBadDebtFor2bComms, twoBCosts["englandTotalWithBadDebtProvision"]);
            AssertAreEqual(producer.TotalProducerFeeWithoutBadDebtFor2bComms, twoBCosts["totalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision"]);
            AssertAreEqual(producer.BadDebtProvisionFor2bComms, twoBCosts["badDebtProvisionFor2bComms"]);
            AssertAreEqual(producer.TotalProducerFeeWithBadDebtFor2bComms, twoBCosts["totalProducerFeeForCommsCostsUKWideWithBadDebtProvision"]);
        }

        /// <summary>
        /// Serialises a <see cref="CalcResultSummary"/>, then parses the resulting JSON
        /// and checks that the values still match up with the original.
        /// </summary>
        [TestMethod]
        public void Export_CommsCost2CValues_AreValid()
        {
            // Arrange
            var data = SetCalcResultSummayData();

            // Act
            var json = this.TestClass.Export(data, null, new List<int> { 1, 2, 3 });

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                     ["calculationResults"]!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);
            var twoCCosts = roundTrippedData[0]["feeForCommsCostsWithBadDebtProvision2c"];
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !t.isTotalRow && !string.IsNullOrEmpty(t.Level));
            AssertAreEqual(producer.TwoCNorthernIrelandTotalWithBadDebt, twoCCosts["northernIrelandTotalWithBadDebtProvision"]);
            AssertAreEqual(producer.TwoCScotlandTotalWithBadDebt, twoCCosts["scotlandTotalWithBadDebtProvision"]);
            AssertAreEqual(producer.TwoCWalesTotalWithBadDebt, twoCCosts["walesTotalWithBadDebtProvision"]);
            AssertAreEqual(producer.TwoCEnglandTotalWithBadDebt, twoCCosts["englandTotalWithBadDebtProvision"]);
            AssertAreEqual(producer.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt, twoCCosts["totalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision"]);
            AssertAreEqual(producer.TwoCTotalProducerFeeForCommsCostsWithBadDebt, twoCCosts["totalProducerFeeForCommsCostsByCountryWithBadDebtProvision"]);
            AssertAreEqual(producer.TwoCBadDebtProvision, twoCCosts["badDebProvisionFor2c"]);
        }

        [TestMethod]
        public void Export_BillingInstructions_AreValid()
        {
            // Arrange
            var data = SetCalcResultSummayData();

            // Act
            var json = this.TestClass.Export(data, null, new List<int> { 1, 2, 3 });

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                     ["calculationResults"]!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);
            var billingInstructions = roundTrippedData[0]["calculationOfSuggestedBillingInstructionsAndInvoiceAmounts"];
            Assert.IsNotNull(billingInstructions);
        }

        /// <summary>
        /// Serialises a <see cref="CalcResultSummary"/>, then parses the resulting JSON
        /// and checks that the values still match up with the original.
        /// </summary>
        [TestMethod]
        public void Export_FeeForLADisposalCost1_AreValid()
        {
            // Arrange
            var data = SetCalcResultSummayData();

            // Act
            var json = this.TestClass.Export(data, null, new List<int> { 1, 2, 3 });

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                     ["calculationResults"]!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);
            var feeForLADisposalCosts1 = roundTrippedData[0]["feeForLADisposalCosts1"];
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !t.isTotalRow && !string.IsNullOrEmpty(t.Level));
            AssertAreEqual(producer.NorthernIrelandTotalWithBadDebtProvision, feeForLADisposalCosts1["northernIrelandTotalForLADisposalCostsWithBadDebtProvision"]);
            AssertAreEqual(producer.ScotlandTotalWithBadDebtProvision, feeForLADisposalCosts1["scotlandTotalForLADisposalCostsWithBadDebtProvision"]);
            AssertAreEqual(producer.WalesTotalWithBadDebtProvision, feeForLADisposalCosts1["walesTotalForLADisposalCostsWithBadDebtProvision"]);
            AssertAreEqual(producer.EnglandTotalWithBadDebtProvision, feeForLADisposalCosts1["englandTotalForLADisposalCostsWithBadDebtProvision"]);
            AssertAreEqual(producer.TotalProducerFeeforLADisposalCostswithBadDebtprovision, feeForLADisposalCosts1["totalProducerFeeForLADisposalCostsWithBadDebtProvision"]);
            AssertAreEqual(producer.BadDebtProvisionFor1, feeForLADisposalCosts1["badDebtProvisionForLADisposalCosts"]);
            AssertAreEqual(producer.TotalProducerFeeforLADisposalCostswoBadDebtprovision, feeForLADisposalCosts1["totalProducerFeeForLADisposalCostsWithoutBadDebtProvision"]);
        }

        private CalcResultSummary SetCalcResultSummayData()
        {
            var data = Fixture.Create<CalcResultSummary>();

            var acceptIds = new List<int> { 1, 2, 3 };

            for (var i = 1; i <= data.ProducerDisposalFees.Count(); i++)
            {
                data.ProducerDisposalFees.ToList()[i - 1].ProducerId = i.ToString();
            }

            return data;
        }
    }
}