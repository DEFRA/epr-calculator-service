namespace EPR.Calculator.Service.Function.UnitTests.Exporter.JsonExporter.CalcResult
{
    using AutoFixture;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Exporter.JsonExporter.CalculationResults;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.UnitTests.Builder;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Text.Json.Serialization;
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
                new CalcResultLADataPrepCostsWithBadDebtProvision4Mapper(),
                new FeeForCommsCostsWithBadDebtProvision2aMapper(),
                new FeeForCommsCostsWithBadDebtProvision2bMapper(),
                new TotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper(),
                new FeeForSASetUpCostsWithBadDebtProvision_5Mapper(),
                new CalcResultCommsCostsWithBadDebtProvision2cMapper(),
                new CalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper(),
                new TotalProducerBillWithBadDebtProvisionMapper(),
                new CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1Mapper(),
                new CalcResultProducerCalculationResultsTotalMapper(),
                new DisposalFeeSummary1Mapper()
            );
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var data = TestDataHelper.GetCalcResultSummary();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var result = this.TestClass.Export(data, new List<int>(), materials);

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
            var data = TestDataHelper.GetCalcResultSummary();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var obj = this.TestClass.Export(data, new List<int> { 1, 2, 3 }, materials);
            var json = JsonSerializer.Serialize(obj);

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["producerCalculationResultsSummary"];

            // Assert
            Assert.IsNotNull(roundTrippedData);

            // 1
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.TotalFeeforLADisposalCostswoBadDebtprovision1),
                roundTrippedData["FeeForLaDisposalCostsWithoutBadDebtprovision1"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.BadDebtProvisionFor1),
                roundTrippedData["BadDebtProvision1"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.TotalFeeforLADisposalCostswithBadDebtprovision1),
                roundTrippedData["FeeForLaDisposalCostsWithBadDebtprovision1"]);

            // 2a
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A),
                roundTrippedData["FeeForCommsCostsByMaterialWithoutBadDebtprovision2a"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.BadDebtProvisionFor2A),
                roundTrippedData["BadDebtProvision2a"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A),
                roundTrippedData["FeeForCommsCostsByMaterialWitBadDebtprovision2a"]);

            // 2b
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.CommsCostHeaderWithoutBadDebtFor2bTitle),
                roundTrippedData["FeeForCommsCostsUkWideWithoutBadDebtprovision2b"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.CommsCostHeaderBadDebtProvisionFor2bTitle),
                roundTrippedData["BadDebtProvision2b"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.CommsCostHeaderWithBadDebtFor2bTitle),
                roundTrippedData["FeeForCommsCostsUkWideWithBadDebtprovision2b"]);

            // 2c
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.TwoCCommsCostsByCountryWithoutBadDebtProvision),
                roundTrippedData["FeeForCommsCostsByCountryWithoutBadDebtprovision2c"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.TwoCBadDebtProvision),
                roundTrippedData["BadDebtProvision2c"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.TwoCCommsCostsByCountryWithBadDebtProvision),
                roundTrippedData["FeeForCommsCostsByCountryWideWithBadDebtprovision2c"]);

            // 1+2a+2b+2c
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.TotalOnePlus2A2B2CFeeWithBadDebtProvision),
                roundTrippedData["Total12a2b2cWithBadDebt"]);

            // 3
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.SaOperatingCostsWoTitleSection3),
                roundTrippedData["SaOperatingCostsWithoutBadDebtProvision3"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.BadDebtProvisionTitleSection3),
                roundTrippedData["BadDebtProvision3"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.SaOperatingCostsWithTitleSection3),
                roundTrippedData["SaOperatingCostsWithBadDebtProvision3"]);

            // 4
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.LaDataPrepCostsTitleSection4),
                roundTrippedData["LaDataPrepCostsWithoutBadDebtProvision4"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.LaDataPrepCostsBadDebtProvisionTitleSection4),
                roundTrippedData["BadDebtProvision4"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.LaDataPrepCostsWithBadDebtProvisionTitleSection4),
                roundTrippedData["LaDataPrepCostsWithbadDebtProvision4"]);

            // 5
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.SaSetupCostsTitleSection5),
                roundTrippedData["OneOffFeeSaSetuCostsWithbadDebtProvision5"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.SaSetupCostsBadDebtProvisionTitleSection5),
                roundTrippedData["BadDebtProvision5"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.SaSetupCostsWithBadDebtProvisionTitleSection5),
                roundTrippedData["OneOffFeeSaSetuCostsWithoutbadDebtProvision5"]);
        }

        [TestMethod]
        public void Export_ProducerDisposalFeesWithBadDebtProvision1_ReturnsValidValues()
        {
            // Arrange
            var data = TestDataHelper.GetCalcResultSummary();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var obj = this.TestClass.Export(data, new List<int> { 1, 2, 3 }, materials);
            var json = JsonSerializer.Serialize(obj);

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);

            var actual = roundTrippedData[0]!["ProducerDisposalFeesWithBadDebtProvision1"]!["MaterialBreakdown"]![0]!;
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level))!;
            var expected = producer.ProducerDisposalFeesByMaterial.First();

            Assert.AreEqual(expected.Value.PreviousInvoicedTonnage, actual["PreviousInvoicedTonnage"]!.ToString());
            Assert.AreEqual(expected.Value.HouseholdPackagingWasteTonnage, actual["HouseholdPackagingWasteTonnage"]!.GetValue<decimal>());
            Assert.AreEqual(expected.Value.PublicBinTonnage, actual["PublicBinTonnage"]!.GetValue<decimal>());
            Assert.AreEqual(expected.Value.TotalReportedTonnage, actual["TotalTonnage"]!.GetValue<decimal>());
            Assert.AreEqual(expected.Value.ManagedConsumerWasteTonnage, actual["SelfManagedConsumerWasteTonnage"]!.GetValue<decimal>());
            Assert.AreEqual(expected.Value.NetReportedTonnage, actual["NetTonnage"]!.GetValue<decimal>());
            Assert.AreEqual(expected.Value.TonnageChange, actual["TonnageChange"]!.ToString());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(expected.Value.PricePerTonne, 4), actual["PricePerTonne"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(expected.Value.ProducerDisposalFee), actual["ProducerDisposalFeeWithoutBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(expected.Value.BadDebtProvision), actual["BadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(expected.Value.ProducerDisposalFeeWithBadDebtProvision), actual["ProducerDisposalFeeWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(expected.Value.EnglandWithBadDebtProvision), actual["EnglandWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(expected.Value.WalesWithBadDebtProvision), actual["WalesWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(expected.Value.ScotlandWithBadDebtProvision), actual["ScotlandWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(expected.Value.NorthernIrelandWithBadDebtProvision), actual["NorthernIrelandWithBadDebtProvision"]!.GetValue<string>());
        }

        [TestMethod]
        public void Export_TotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c_ReturnsValidValues()
        {
            // Arrange
            var data = TestDataHelper.GetCalcResultSummary();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var obj = this.TestClass.Export(data, new List<int> { 1, 2, 3 }, materials);
            var json = JsonSerializer.Serialize(obj);

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);
            var actual = roundTrippedData[0]!["TotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c"]!;
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level))!;

            AssertAreEqual(CurrencyConverter.ConvertToCurrency(producer.ProducerTotalOnePlus2A2B2CWithBadDeptProvision), actual["TotalFeeWithBadDebtProvision"]);
            AssertAreEqual($"{producer.ProducerOverallPercentageOfCostsForOnePlus2A2B2C.ToString("F8")}%", actual["ProducerPercentageOfOverallProducerCost"]);
        }

        /// <summary>
        /// Serialises a <see cref="CalcResultSummary"/>, then parses the resulting JSON
        /// and checks that the values still match up with the original.
        /// </summary>
        [TestMethod]
        public void Export_CommsCost2AValues_AreValid()
        {
            // Arrange
            var data = TestDataHelper.GetCalcResultSummary();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var obj = this.TestClass.Export(data, new List<int> { 1, 2, 3 }, materials);
            var json = JsonSerializer.Serialize(obj);

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);
            var twoACosts = roundTrippedData[0]?["CommsCostsByMaterialFeesSummary2a"];
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
            Assert.IsNotNull(producer);
            AssertAreEqual(producer.NorthernIrelandTotalWithBadDebtProvision2A, twoACosts?["NorthernIrelandTotalWithBadDebtProvision"]!);
            AssertAreEqual(producer.ScotlandTotalWithBadDebtProvision2A, twoACosts?["ScotlandTotalWithBadDebtProvision"]!);
            AssertAreEqual(producer.WalesTotalWithBadDebtProvision2A, twoACosts?["WalesTotalWithBadDebtProvision"]!);
            AssertAreEqual(producer.EnglandTotalWithBadDebtProvision2A, twoACosts?["EnglandTotalWithBadDebtProvision"]!);
            AssertAreEqual(producer.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision, twoACosts?["TotalProducerFeeForCommsCostsWithoutBadDebtProvision2a"]!);
            AssertAreEqual(producer.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision, twoACosts?["TotalProducerFeeForCommsCostsWithBadDebtProvision2a"]!);
            AssertAreEqual(producer.BadDebtProvisionFor2A, twoACosts?["TotalBadDebtProvision"]);  
        }

        [TestMethod]
        public void Export_FeeForSASetUpCostsWithBadDebtProvision_5_ReturnsValidValues()
        {
            // Arrange
            var data = TestDataHelper.GetCalcResultSummary();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var obj = this.TestClass.Export(data, new List<int> { 1, 2, 3 }, materials);
            var json = JsonSerializer.Serialize(obj);
            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);
            var actual = roundTrippedData[0]!["FeeForSASetUpCostsWithBadDebtProvision_5"]!;
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level))!;

            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.TotalProducerFeeWithoutBadDebtProvisionSection5), actual["TotalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.BadDebtProvisionSection5), actual["BadDebtProvisionFor5"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.TotalProducerFeeWithBadDebtProvisionSection5), actual["TotalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.EnglandTotalWithBadDebtProvisionSection5), actual["EnglandTotalForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.WalesTotalWithBadDebtProvisionSection5), actual["WalesTotalForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.ScotlandTotalWithBadDebtProvisionSection5), actual["ScotlandTotalForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.NorthernIrelandTotalWithBadDebtProvisionSection5), actual["NorthernIrelandTotalForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
        }

        /// <summary>
        /// Serialises a <see cref="CalcResultSummary"/>, then parses the resulting JSON
        /// and checks that the values still match up with the original.
        /// </summary>
        [TestMethod]
        public void Export_CommsCost3SA_Operating_Costs_AreValid()
        {
            // Arrange
            var data = TestDataHelper.GetCalcResultSummary();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var obj = this.TestClass.Export(data, new List<int> { 1, 2, 3 }, materials);
            var json = JsonSerializer.Serialize(obj);

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);
            var threeSACosts = roundTrippedData[0]!["FeeForSAOperatingCostsWithBadDebtProvision_3"];
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
            Assert.IsNotNull(producer);
            Assert.IsNotNull(threeSACosts);
            AssertAreEqual(producer.NorthernIrelandTotalWithBadDebtProvision3, threeSACosts["NorthernIrelandTotalForSAOperatingCostsWithBadDebtProvision"]!);
            AssertAreEqual(producer.ScotlandTotalWithBadDebtProvision3, threeSACosts["ScotlandTotalForSAOperatingCostsWithBadDebtProvision"]!);
            AssertAreEqual(producer.WalesTotalWithBadDebtProvision3, threeSACosts["WalesTotalForSAOperatingCostsWithBadDebtProvision"]!);
            AssertAreEqual(producer.EnglandTotalWithBadDebtProvision3, threeSACosts["EnglandTotalForSAOperatingCostsWithBadDebtProvision"]!);
            AssertAreEqual(producer.Total3SAOperatingCostswithBadDebtprovision, threeSACosts["TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision"]!);
            AssertAreEqual(producer.Total3SAOperatingCostwoBadDebtprovision, threeSACosts["TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision"]!);
            AssertAreEqual(producer.BadDebtProvisionFor3, threeSACosts["BadDebProvisionFor3"]!);
        }

        /// <summary>
        /// Serialises a <see cref="CalcResultSummary"/>, then parses the resulting JSON
        /// and checks that the values still match up with the original.
        /// </summary>
        [TestMethod]
        public void Export_FeeForCommsCostsWithBadDebtProvision2a_AreValid()
        {
            // Arrange
            var data = TestDataHelper.GetCalcResultSummary();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var obj = this.TestClass.Export(data, new List<int> { 1, 2, 3 }, materials);
            var json = JsonSerializer.Serialize(obj);
            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);
            var twoACosts = roundTrippedData[0]!["FeeForCommsCostsWithBadDebtProvision_2a"];
            Assert.IsNotNull(twoACosts);
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
            Assert.IsNotNull(producer);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(producer.NorthernIrelandTotalWithBadDebtProvision2A), twoACosts["NorthernIrelandTotalWithBadDebtProvision"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(producer.ScotlandTotalWithBadDebtProvision2A), twoACosts["ScotlandTotalWithBadDebtProvision"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(producer.WalesTotalWithBadDebtProvision2A), twoACosts["WalesTotalWithBadDebtProvision"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(producer.EnglandTotalWithBadDebtProvision2A), twoACosts["EnglandTotalWithBadDebtProvision"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(producer.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision), twoACosts["TotalProducerFeeForCommsCostsWithoutBadDebtProvision"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(producer.BadDebtProvisionFor2A), twoACosts["BadDebtProvisionFor2a"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(producer.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision), twoACosts["TotalProducerFeeForCommsCostsWithBadDebtProvision"]);
        }

        [TestMethod]
        public void Export_FeeForCommsCostsWithBadDebtProvision2b_AreValid()
        {
            // Arrange
            var data = TestDataHelper.GetCalcResultSummary();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var obj = this.TestClass.Export(data, new List<int> { 1, 2, 3 }, materials);
            var json = JsonSerializer.Serialize(obj);
            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);
            var twoBCosts = roundTrippedData[0]!["FeeForCommsCostsWithBadDebtProvision_2b"];
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
            Assert.IsNotNull(producer);
            Assert.IsNotNull(twoBCosts);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.NorthernIrelandTotalWithBadDebtFor2bComms), twoBCosts["NorthernIrelandTotalWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.ScotlandTotalWithBadDebtFor2bComms), twoBCosts["ScotlandTotalWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.WalesTotalWithBadDebtFor2bComms), twoBCosts["WalesTotalWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.EnglandTotalWithBadDebtFor2bComms), twoBCosts["EnglandTotalWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.TotalProducerFeeWithoutBadDebtFor2bComms), twoBCosts["TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.BadDebtProvisionFor2bComms), twoBCosts["BadDebtProvisionFor2b"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.TotalProducerFeeWithBadDebtFor2bComms), twoBCosts["TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision"]!.GetValue<string>());
        }

        /// <summary>
        /// Serialises a <see cref="CalcResultSummary"/>, then parses the resulting JSON
        /// and checks that the values still match up with the original.
        /// </summary>
        [TestMethod]
        public void Export_CommsCost2CValues_AreValid()
        {
            // Arrange
            var data = TestDataHelper.GetCalcResultSummary();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var obj = this.TestClass.Export(data, new List<int> { 1, 2, 3 }, materials);
            var json = JsonSerializer.Serialize(obj);
            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);
            var twoCCosts = roundTrippedData[0]!["FeeForCommsCostsWithBadDebtProvision_2c"];
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
            Assert.IsNotNull(producer);
            Assert.IsNotNull(twoCCosts);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.TwoCNorthernIrelandTotalWithBadDebt), twoCCosts["NorthernIrelandTotalWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.TwoCScotlandTotalWithBadDebt), twoCCosts["ScotlandTotalWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.TwoCWalesTotalWithBadDebt), twoCCosts["WalesTotalWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.TwoCEnglandTotalWithBadDebt), twoCCosts["EnglandTotalWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt), twoCCosts["TotalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.TwoCTotalProducerFeeForCommsCostsWithBadDebt), twoCCosts["TotalProducerFeeForCommsCostsByCountryWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.TwoCBadDebtProvision), twoCCosts["BadDebProvisionFor2c"]!.GetValue<string>());
        }

        [TestMethod]
        public void Export_DisposalFeeSummary1()
        {
            // Arrange
            var data = TestDataHelper.GetCalcResultSummary();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var obj = this.TestClass.Export(data, new List<int> { 1, 2, 3 }, materials);
            var json = JsonSerializer.Serialize(obj);
            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);
            var disposalFeeSummary1 = roundTrippedData[0]!["DisposalFeeSummary1"]!;
            Assert.IsNotNull(roundTrippedData);
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level))!;

            // Disposal Fee
            AssertAreEqual(producer.TotalProducerDisposalFee,
                disposalFeeSummary1["TotalProducerDisposalFeeWithoutBadDebtProvision"]!);
            AssertAreEqual(producer.BadDebtProvision,
                disposalFeeSummary1["BadDebtProvision"]!);
            AssertAreEqual(producer.TotalProducerDisposalFeeWithBadDebtProvision,
                disposalFeeSummary1["TotalProducerDisposalFeeWithBadDebtProvision"]!);

            // Countries
            AssertAreEqual(producer.EnglandTotal,
                disposalFeeSummary1["EnglandTotal"]!);
            AssertAreEqual(producer.WalesTotal,
                disposalFeeSummary1["WalesTotal"]!);
            AssertAreEqual(producer.ScotlandTotal,
                disposalFeeSummary1["ScotlandTotal"]!);
            AssertAreEqual(producer.NorthernIrelandTotal,
                disposalFeeSummary1["NorthernIrelandTotal"]!);

            // Tonnage Change
            Assert.AreEqual(producer.TonnageChangeCount,
                disposalFeeSummary1["TonnageChangeCount"]?.ToString());
            Assert.AreEqual(producer.TonnageChangeAdvice,
                disposalFeeSummary1["TonnageChangeAdvice"]?.ToString());
        }

        [TestMethod]
        public void Export_BillingInstructions_AreValid()
        {
            // Arrange
            var data = TestDataHelper.GetCalcResultSummary();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var obj = this.TestClass.Export(data, new List<int> { 1, 2, 3 }, materials);
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            var json = JsonSerializer.Serialize(obj, options);

            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);
            Assert.IsNotNull(roundTrippedData[0]);
            var billingInstructions = roundTrippedData[0]?["CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts"];
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
            var data = TestDataHelper.GetCalcResultSummary();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var obj = this.TestClass.Export(data, new List<int> { 1, 2, 3 }, materials);
            var json = JsonSerializer.Serialize(obj);
            var node = JsonNode.Parse(json);
            var roundTrippedData = node?["producerCalculationResults"]?.AsArray();

            // Assert
            Assert.IsNotNull(roundTrippedData);
           
            var feeForLADisposalCosts1 = roundTrippedData[0]?["FeeForLADisposalCosts1"];
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
            if (producer == null)
            {
                Assert.Fail("Producer not found.");
            }
            AssertAreEqual(producer.NorthernIrelandTotalWithBadDebtProvision, feeForLADisposalCosts1?["NorthernIrelandTotalForLADisposalCostsWithBadDebtProvision"]!);
            AssertAreEqual(producer.ScotlandTotalWithBadDebtProvision, feeForLADisposalCosts1?["ScotlandTotalForLADisposalCostsWithBadDebtProvision"]!);
            AssertAreEqual(producer.WalesTotalWithBadDebtProvision, feeForLADisposalCosts1?["WalesTotalForLADisposalCostsWithBadDebtProvision"]!);
            AssertAreEqual(producer.EnglandTotalWithBadDebtProvision, feeForLADisposalCosts1?["EnglandTotalForLADisposalCostsWithBadDebtProvision"]!);
            AssertAreEqual(producer.TotalProducerFeeforLADisposalCostswithBadDebtprovision, feeForLADisposalCosts1?["TotalProducerFeeForLADisposalCostsWithBadDebtProvision"]!);
            AssertAreEqual(producer.BadDebtProvisionFor1, feeForLADisposalCosts1?["BadDebtProvisionForLADisposalCosts"]!);
            AssertAreEqual(producer.TotalProducerFeeforLADisposalCostswoBadDebtprovision, feeForLADisposalCosts1?["TotalProducerFeeForLADisposalCostsWithoutBadDebtProvision"]!);
        }

        /// <summary>
        /// Serialises a <see cref="CalcResultSummary"/>, then parses the resulting JSON
        /// and checks that the values still match up with the original.
        /// </summary>
        [TestMethod]
        public void Export_ProducerIdSubsidiaryId_AreValid()
        {
            // Arrange
            var data = TestDataHelper.GetCalcResultSummary();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var obj = this.TestClass.Export(data, new List<int> { 1, 2, 3 }, materials);
            var json = JsonSerializer.Serialize(obj);
            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["producerCalculationResults"];

            // Assert
            Assert.IsNotNull(roundTrippedData);

            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
            if (producer == null)
            {
                Assert.Fail("Producer not found.");
            }
            Assert.AreEqual(producer.ProducerId, roundTrippedData[0]!?["ProducerID"]?.ToString());
            Assert.AreEqual(producer.SubsidiaryId, roundTrippedData[0]!?["SubsidiaryID"]?.ToString());
            Assert.AreEqual(producer.ProducerName, roundTrippedData[0]!?["ProducerName"]?.ToString());
            Assert.AreEqual(producer?.TradingName, roundTrippedData[0]!?["TradingName"]?.ToString());
            Assert.AreEqual(producer?.Level ?? "1" , roundTrippedData[0]!?["Level"]?.ToString());
            Assert.AreEqual(producer?.IsProducerScaledup ?? "No", roundTrippedData[0]!?["ScaledUpTonnages"]?.ToString());
        }

        [TestMethod]
        public void Export_ProducerCalculationResultsTotal_CanBeNull()
        {
            // Arrange
            var data = TestDataHelper.GetCalcResultSummary();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var obj = this.TestClass.Export(data, new List<int> { 1, 2, 3 }, materials);
            var json = JsonSerializer.Serialize(obj);
            var producerCalculationResultsTotal = JsonSerializer.Deserialize<JsonObject>(json)!["producerCalculationResultsTotal"]!;

            // Assert
            Assert.IsNull(producerCalculationResultsTotal);
        }

        [TestMethod]
        public void Export_FeeForLADataPrepCostsWithBadDebtProvision_4_AreValid()
        {
            // Arrange
            var data = TestDataHelper.GetCalcResultSummary();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var obj = this.TestClass.Export(data, new List<int> { 1, 2, 3 }, materials);
            var json = JsonSerializer.Serialize(obj);
            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["producerCalculationResults"];


            // Assert
            Assert.IsNotNull(roundTrippedData);
            Assert.IsNotNull(roundTrippedData[0]);
            var costs = roundTrippedData[0]!["FeeForLADataPrepCostsWithBadDebtProvision_4"];
            Assert.IsNotNull(costs);

            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
            Assert.IsNotNull(producer);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.LaDataPrepCostsTotalWithoutBadDebtProvisionSection4), costs["TotalProducerFeeForLADataPrepCostsWithoutBadDebtProvision"]!.GetValue<String>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.LaDataPrepCostsBadDebtProvisionSection4), costs["BadDebtProvisionFor4"]!.GetValue<String>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.LaDataPrepCostsTotalWithBadDebtProvisionSection4), costs["TotalProducerFeeForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4), costs["NorthernIrelandTotalForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4), costs["ScotlandTotalForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4), costs["WalesTotalForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4), costs["EnglandTotalForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
        }

        [TestMethod]
        public void Export_CalculationResultsExporter_AreValid()
        {
            // Arrange
            var data = TestDataHelper.GetCalcResultSummary();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var obj = this.TestClass.Export(data, new List<int> { 1, 2, 3 }, materials);
            var json = JsonSerializer.Serialize(obj);
            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                    ["producerCalculationResults"]!;

            // Assert
            var calculationResult = roundTrippedData[0]!;
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level))!;

            // Main Fields
            Assert.AreEqual(producer.ProducerId, calculationResult["ProducerID"]?.GetValue<string>());
            Assert.AreEqual(producer.SubsidiaryId, calculationResult["SubsidiaryID"]?.GetValue<string>());
            Assert.AreEqual(producer.ProducerName, calculationResult["ProducerName"]?.GetValue<string>());
            Assert.AreEqual(producer.TradingName!, calculationResult["TradingName"]?.GetValue<string>());
            Assert.AreEqual(int.Parse(producer.Level!), calculationResult["Level"]?.GetValue<int>());
            Assert.AreEqual(producer.IsProducerScaledup, calculationResult["ScaledUpTonnages"]?.GetValue<string>());

            // Sub-Sections
            var producerDisposalFeesWithBadDebtProvision1 = roundTrippedData[0]!["ProducerDisposalFeesWithBadDebtProvision1"];
            Assert.IsNotNull(producerDisposalFeesWithBadDebtProvision1);
            var disposalFeeSummary1 = roundTrippedData[0]!["DisposalFeeSummary1"];
            Assert.IsNotNull(disposalFeeSummary1);
            var feesForCommsCostsWithBadDebtProvision2a = roundTrippedData[0]!["FeesForCommsCostsWithBadDebtProvision2a"];
            Assert.IsNotNull(feesForCommsCostsWithBadDebtProvision2a);
            var commsCostsByMaterialFeesSummary2a = roundTrippedData[0]!["CommsCostsByMaterialFeesSummary2a"];
            Assert.IsNotNull(commsCostsByMaterialFeesSummary2a);
            var feeForLADisposalCosts1 = roundTrippedData[0]!["FeeForLADisposalCosts1"];
            Assert.IsNotNull(feeForLADisposalCosts1);
            var feeForCommsCostsWithBadDebtProvision_2a = roundTrippedData[0]!["FeeForCommsCostsWithBadDebtProvision_2a"];
            Assert.IsNotNull(feeForCommsCostsWithBadDebtProvision_2a);
            var feeForCommsCostsWithBadDebtProvision_2b = roundTrippedData[0]!["FeeForCommsCostsWithBadDebtProvision_2b"];
            Assert.IsNotNull(feeForCommsCostsWithBadDebtProvision_2b);
            var feeForCommsCostsWithBadDebtProvision_2c = roundTrippedData[0]!["FeeForCommsCostsWithBadDebtProvision_2c"];
            Assert.IsNotNull(feeForCommsCostsWithBadDebtProvision_2c);
            var totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c = roundTrippedData[0]!["TotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c"];
            Assert.IsNotNull(totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c);
            var feeForSAOperatingCostsWithBadDebtProvision_3 = roundTrippedData[0]!["FeeForSAOperatingCostsWithBadDebtProvision_3"];
            Assert.IsNotNull(feeForSAOperatingCostsWithBadDebtProvision_3);
            var feeForLADataPrepCostsWithBadDebtProvision_4 = roundTrippedData[0]!["FeeForLADataPrepCostsWithBadDebtProvision_4"];
            Assert.IsNotNull(feeForLADataPrepCostsWithBadDebtProvision_4);
            var feeForSASetUpCostsWithBadDebtProvision_5 = roundTrippedData[0]!["FeeForSASetUpCostsWithBadDebtProvision_5"];
            Assert.IsNotNull(feeForSASetUpCostsWithBadDebtProvision_5);
            var totalProducerBillWithBadDebtProvision = roundTrippedData[0]!["TotalProducerBillWithBadDebtProvision"];
            Assert.IsNotNull(totalProducerBillWithBadDebtProvision);
            var calculationOfSuggestedBillingInstructionsAndInvoiceAmounts = roundTrippedData[0]!["CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts"];
            Assert.IsNotNull(calculationOfSuggestedBillingInstructionsAndInvoiceAmounts);
        }

        [TestMethod]
        public void Export_ProducerCalculationResult_Level1_AreDisplayed()
        {
            // Arrange
            var data = TestDataHelper.GetCalcResultSummary();
            var materials = TestDataHelper.GetMaterials();

            data.ProducerDisposalFees.First().isTotalRow = true;
            data.ProducerDisposalFees.First().Level = "1";

            // Act
            var obj = this.TestClass.Export(data, new List<int> { 1, 2, 3 }, materials);
            var json = JsonSerializer.Serialize(obj);
            var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                    ["producerCalculationResults"]!;

            // Assert
            var calculationResult = roundTrippedData[0]!;
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level))!;

            // Main Fields
            AssertAreEqual(producer.ProducerId, calculationResult["ProducerID"]);
        }        
    }
}