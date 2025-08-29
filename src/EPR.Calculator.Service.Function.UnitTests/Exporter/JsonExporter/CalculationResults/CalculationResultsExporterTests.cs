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
                new TotalProducerFeeWithBadDebtProvisionFor2con_1_2a_2b_2cMapper(),
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
                roundTrippedData["feeForLaDisposalCostsWithoutBadDebtprovision1"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.BadDebtProvisionFor1),
                roundTrippedData["badDebtProvision1"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.TotalFeeforLADisposalCostswithBadDebtprovision1),
                roundTrippedData["feeForLaDisposalCostsWithBadDebtprovision1"]);

            // 2a
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A),
                roundTrippedData["feeForCommsCostsByMaterialWithoutBadDebtprovision2a"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.BadDebtProvisionFor2A),
                roundTrippedData["badDebtProvision2a"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A),
                roundTrippedData["feeForCommsCostsByMaterialWitBadDebtprovision2a"]);

            // 2b
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.CommsCostHeaderWithoutBadDebtFor2bTitle),
                roundTrippedData["feeForCommsCostsUkWideWithoutBadDebtprovision2b"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.CommsCostHeaderBadDebtProvisionFor2bTitle),
                roundTrippedData["badDebtProvision2b"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.CommsCostHeaderWithBadDebtFor2bTitle),
                roundTrippedData["feeForCommsCostsUkWideWithBadDebtprovision2b"]);

            // 2c
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.TwoCCommsCostsByCountryWithoutBadDebtProvision),
                roundTrippedData["feeForCommsCostsByCountryWithoutBadDebtprovision2c"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.TwoCBadDebtProvision),
                roundTrippedData["badDebtProvision2c"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.TwoCCommsCostsByCountryWithBadDebtProvision),
                roundTrippedData["feeForCommsCostsByCountryWideWithBadDebtprovision2c"]);

            // 1+2a+2b+2c
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.TotalOnePlus2A2B2CFeeWithBadDebtProvision),
                roundTrippedData["total12a2b2cWithBadDebt"]);

            // 3
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.SaOperatingCostsWoTitleSection3),
                roundTrippedData["saOperatingCostsWithoutBadDebtProvision3"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.BadDebtProvisionTitleSection3),
                roundTrippedData["badDebtProvision3"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.SaOperatingCostsWithTitleSection3),
                roundTrippedData["saOperatingCostsWithBadDebtProvision3"]);

            // 4
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.LaDataPrepCostsTitleSection4),
                roundTrippedData["laDataPrepCostsWithoutBadDebtProvision4"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.LaDataPrepCostsBadDebtProvisionTitleSection4),
                roundTrippedData["badDebtProvision4"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.LaDataPrepCostsWithBadDebtProvisionTitleSection4),
                roundTrippedData["laDataPrepCostsWithbadDebtProvision4"]);

            // 5
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.SaSetupCostsTitleSection5),
                roundTrippedData["oneOffFeeSaSetuCostsWithbadDebtProvision5"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.SaSetupCostsBadDebtProvisionTitleSection5),
                roundTrippedData["badDebtProvision5"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(data.SaSetupCostsWithBadDebtProvisionTitleSection5),
                roundTrippedData["oneOffFeeSaSetuCostsWithoutbadDebtProvision5"]);
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

            var actual = roundTrippedData[0]!["producerDisposalFeesWithBadDebtProvision1"]!["materialBreakdown"]![0]!;
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level))!;
            var expected = producer.ProducerDisposalFeesByMaterial.First();

            Assert.AreEqual(expected.Value.PreviousInvoicedTonnage, actual["previousInvoicedTonnage"]!.GetValue<decimal>());
            Assert.AreEqual(expected.Value.HouseholdPackagingWasteTonnage, actual["householdPackagingWasteTonnage"]!.GetValue<decimal>());
            Assert.AreEqual(expected.Value.PublicBinTonnage, actual["publicBinTonnage"]!.GetValue<decimal>());
            Assert.AreEqual(expected.Value.TotalReportedTonnage, actual["totalTonnage"]!.GetValue<decimal>());
            Assert.AreEqual(expected.Value.ManagedConsumerWasteTonnage, actual["selfManagedConsumerWasteTonnage"]!.GetValue<decimal>());
            Assert.AreEqual(expected.Value.NetReportedTonnage, actual["netTonnage"]!.GetValue<decimal>());
            Assert.AreEqual(expected.Value.TonnageChange, actual["tonnageChange"]!.ToString());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(expected.Value.PricePerTonne, 4), actual["pricePerTonne"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(expected.Value.ProducerDisposalFee), actual["producerDisposalFeeWithoutBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(expected.Value.BadDebtProvision), actual["badDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(expected.Value.ProducerDisposalFeeWithBadDebtProvision), actual["producerDisposalFeeWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(expected.Value.EnglandWithBadDebtProvision), actual["englandWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(expected.Value.WalesWithBadDebtProvision), actual["walesWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(expected.Value.ScotlandWithBadDebtProvision), actual["scotlandWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(expected.Value.NorthernIrelandWithBadDebtProvision), actual["northernIrelandWithBadDebtProvision"]!.GetValue<string>());
        }

        [TestMethod]
        public void Export_TotalProducerFeeWithBadDebtProvisionFor2con_1_2a_2b_2c_ReturnsValidValues()
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
            var actual = roundTrippedData[0]!["totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c"]!;
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level))!;

            AssertAreEqual(CurrencyConverter.ConvertToCurrency(producer.ProducerTotalOnePlus2A2B2CWithBadDeptProvision), actual["totalFeeWithBadDebtProvision"]);
            AssertAreEqual($"{producer.ProducerOverallPercentageOfCostsForOnePlus2A2B2C.ToString("F8")}%", actual["producerPercentageOfOverallProducerCost"]);
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
            var twoACosts = roundTrippedData[0]?["commsCostsByMaterialFeesSummary2a"];
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
            Assert.IsNotNull(producer);
            AssertAreEqual(producer.CommunicationCostsSectionTwoA!.TotalProducerFeeWithoutBadDebtProvision, twoACosts?["totalProducerFeeForCommsCostsWithoutBadDebtProvision2a"]!);
            AssertAreEqual(producer.CommunicationCostsSectionTwoA.BadDebtProvision, twoACosts?["totalBadDebtProvision"]);
            AssertAreEqual(producer.CommunicationCostsSectionTwoA.TotalProducerFeeWithBadDebtProvision, twoACosts?["totalProducerFeeForCommsCostsWithBadDebtProvision2a"]!);
            AssertAreEqual(producer.CommunicationCostsSectionTwoA.EnglandTotalWithBadDebtProvision, twoACosts?["englandTotalWithBadDebtProvision"]!);
            AssertAreEqual(producer.CommunicationCostsSectionTwoA.WalesTotalWithBadDebtProvision, twoACosts?["walesTotalWithBadDebtProvision"]!);
            AssertAreEqual(producer.CommunicationCostsSectionTwoA.ScotlandTotalWithBadDebtProvision, twoACosts?["scotlandTotalWithBadDebtProvision"]!);
            AssertAreEqual(producer.CommunicationCostsSectionTwoA.NorthernIrelandTotalWithBadDebtProvision, twoACosts?["northernIrelandTotalWithBadDebtProvision"]!);
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
            var actual = roundTrippedData[0]!["feeForSASetUpCostsWithBadDebtProvision_5"]!;
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level))!;

            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.OneOffSchemeAdministrationSetupCosts!.TotalProducerFeeWithoutBadDebtProvision), actual["totalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.OneOffSchemeAdministrationSetupCosts.BadDebtProvision), actual["badDebtProvisionFor5"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.OneOffSchemeAdministrationSetupCosts.TotalProducerFeeWithBadDebtProvision), actual["totalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.OneOffSchemeAdministrationSetupCosts.EnglandTotalWithBadDebtProvision), actual["englandTotalForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.OneOffSchemeAdministrationSetupCosts.WalesTotalWithBadDebtProvision), actual["walesTotalForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.OneOffSchemeAdministrationSetupCosts.ScotlandTotalWithBadDebtProvision), actual["scotlandTotalForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.OneOffSchemeAdministrationSetupCosts.NorthernIrelandTotalWithBadDebtProvision), actual["northernIrelandTotalForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
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
            var threeSACosts = roundTrippedData[0]!["feeForSAOperatingCostsWithBadDebtProvision_3"];
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
            Assert.IsNotNull(producer);
            Assert.IsNotNull(threeSACosts);
            AssertAreEqual(producer.SchemeAdministratorOperatingCosts!.TotalProducerFeeWithoutBadDebtProvision, threeSACosts["totalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision"]!);
            AssertAreEqual(producer.SchemeAdministratorOperatingCosts.BadDebtProvision, threeSACosts["badDebProvisionFor3"]!);
            AssertAreEqual(producer.SchemeAdministratorOperatingCosts.TotalProducerFeeWithBadDebtProvision, threeSACosts["totalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision"]!);
            AssertAreEqual(producer.SchemeAdministratorOperatingCosts.EnglandTotalWithBadDebtProvision, threeSACosts["englandTotalForSAOperatingCostsWithBadDebtProvision"]!);
            AssertAreEqual(producer.SchemeAdministratorOperatingCosts.WalesTotalWithBadDebtProvision, threeSACosts["walesTotalForSAOperatingCostsWithBadDebtProvision"]!);
            AssertAreEqual(producer.SchemeAdministratorOperatingCosts.ScotlandTotalWithBadDebtProvision, threeSACosts["scotlandTotalForSAOperatingCostsWithBadDebtProvision"]!);
            AssertAreEqual(producer.SchemeAdministratorOperatingCosts.NorthernIrelandTotalWithBadDebtProvision, threeSACosts["northernIrelandTotalForSAOperatingCostsWithBadDebtProvision"]!);
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
            var twoACosts = roundTrippedData[0]!["feeForCommsCostsWithBadDebtProvision_2a"];
            Assert.IsNotNull(twoACosts);
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
            Assert.IsNotNull(producer);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(producer.CommunicationCostsSectionTwoA!.TotalProducerFeeWithoutBadDebtProvision), twoACosts["totalProducerFeeForCommsCostsWithoutBadDebtProvision"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(producer.CommunicationCostsSectionTwoA.BadDebtProvision), twoACosts["badDebtProvisionFor2a"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(producer.CommunicationCostsSectionTwoA.TotalProducerFeeWithBadDebtProvision), twoACosts["totalProducerFeeForCommsCostsWithBadDebtProvision"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(producer.CommunicationCostsSectionTwoA.EnglandTotalWithBadDebtProvision), twoACosts["englandTotalWithBadDebtProvision"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(producer.CommunicationCostsSectionTwoA.WalesTotalWithBadDebtProvision), twoACosts["walesTotalWithBadDebtProvision"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(producer.CommunicationCostsSectionTwoA.ScotlandTotalWithBadDebtProvision), twoACosts["scotlandTotalWithBadDebtProvision"]);
            AssertAreEqual(CurrencyConverter.ConvertToCurrency(producer.CommunicationCostsSectionTwoA.NorthernIrelandTotalWithBadDebtProvision), twoACosts["northernIrelandTotalWithBadDebtProvision"]);
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
            var twoBCosts = roundTrippedData[0]!["feeForCommsCostsWithBadDebtProvision_2b"];
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
            Assert.IsNotNull(producer);
            Assert.IsNotNull(twoBCosts);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.CommunicationCostsSectionTwoB!.TotalProducerFeeWithoutBadDebtProvision), twoBCosts["totalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.CommunicationCostsSectionTwoB.BadDebtProvision), twoBCosts["badDebtProvisionFor2b"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.CommunicationCostsSectionTwoB.TotalProducerFeeWithBadDebtProvision), twoBCosts["totalProducerFeeForCommsCostsUKWideWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.CommunicationCostsSectionTwoB.EnglandTotalWithBadDebtProvision), twoBCosts["englandTotalWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.CommunicationCostsSectionTwoB.WalesTotalWithBadDebtProvision), twoBCosts["walesTotalWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.CommunicationCostsSectionTwoB.ScotlandTotalWithBadDebtProvision), twoBCosts["scotlandTotalWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.CommunicationCostsSectionTwoB.NorthernIrelandTotalWithBadDebtProvision), twoBCosts["northernIrelandTotalWithBadDebtProvision"]!.GetValue<string>());
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
            var twoCCosts = roundTrippedData[0]!["feeForCommsCostsWithBadDebtProvision_2c"];
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
            Assert.IsNotNull(producer);
            Assert.IsNotNull(twoCCosts);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt), twoCCosts["totalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.TwoCBadDebtProvision), twoCCosts["badDebProvisionFor2c"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.TwoCTotalProducerFeeForCommsCostsWithBadDebt), twoCCosts["totalProducerFeeForCommsCostsByCountryWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.TwoCEnglandTotalWithBadDebt), twoCCosts["englandTotalWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.TwoCWalesTotalWithBadDebt), twoCCosts["walesTotalWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.TwoCScotlandTotalWithBadDebt), twoCCosts["scotlandTotalWithBadDebtProvision"]!.GetValue<string>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.TwoCNorthernIrelandTotalWithBadDebt), twoCCosts["northernIrelandTotalWithBadDebtProvision"]!.GetValue<string>());
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
            var disposalFeeSummary1 = roundTrippedData[0]!["disposalFeeSummary1"]!;
            Assert.IsNotNull(roundTrippedData);
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level))!;

            // Disposal Fee
            AssertAreEqual(producer.TotalProducerDisposalFee,
                disposalFeeSummary1["totalProducerDisposalFeeWithoutBadDebtProvision"]!);
            AssertAreEqual(producer.BadDebtProvision,
                disposalFeeSummary1["badDebtProvision"]!);
            AssertAreEqual(producer.TotalProducerDisposalFeeWithBadDebtProvision,
                disposalFeeSummary1["totalProducerDisposalFeeWithBadDebtProvision"]!);

            // Countries
            AssertAreEqual(producer.EnglandTotal,
                disposalFeeSummary1["englandTotal"]!);
            AssertAreEqual(producer.WalesTotal,
                disposalFeeSummary1["walesTotal"]!);
            AssertAreEqual(producer.ScotlandTotal,
                disposalFeeSummary1["scotlandTotal"]!);
            AssertAreEqual(producer.NorthernIrelandTotal,
                disposalFeeSummary1["northernIrelandTotal"]!);

            // Tonnage Change
            Assert.AreEqual(producer.TonnageChangeCount,
                disposalFeeSummary1["tonnageChangeCount"]?.ToString());
            Assert.AreEqual(producer.TonnageChangeAdvice,
                disposalFeeSummary1["tonnageChangeAdvice"]?.ToString());
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
            var billingInstructions = roundTrippedData[0]?["calculationOfSuggestedBillingInstructionsAndInvoiceAmounts"];
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
           
            var feeForLADisposalCosts1 = roundTrippedData[0]?["feeForLADisposalCosts1"];
            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
            if (producer == null)
            {
                Assert.Fail("Producer not found.");
            }
            AssertAreEqual(producer.LocalAuthorityDisposalCostsSectionOne!.TotalProducerFeeWithoutBadDebtProvision, feeForLADisposalCosts1?["totalProducerFeeForLADisposalCostsWithoutBadDebtProvision"]!);
            AssertAreEqual(producer.LocalAuthorityDisposalCostsSectionOne.BadDebtProvision, feeForLADisposalCosts1?["badDebtProvisionForLADisposalCosts"]!);
            AssertAreEqual(producer.LocalAuthorityDisposalCostsSectionOne.TotalProducerFeeWithBadDebtProvision, feeForLADisposalCosts1?["totalProducerFeeForLADisposalCostsWithBadDebtProvision"]!);
            AssertAreEqual(producer.LocalAuthorityDisposalCostsSectionOne.EnglandTotalWithBadDebtProvision, feeForLADisposalCosts1?["englandTotalForLADisposalCostsWithBadDebtProvision"]!);
            AssertAreEqual(producer.LocalAuthorityDisposalCostsSectionOne.WalesTotalWithBadDebtProvision, feeForLADisposalCosts1?["walesTotalForLADisposalCostsWithBadDebtProvision"]!);
            AssertAreEqual(producer.LocalAuthorityDisposalCostsSectionOne.ScotlandTotalWithBadDebtProvision, feeForLADisposalCosts1?["scotlandTotalForLADisposalCostsWithBadDebtProvision"]!);
            AssertAreEqual(producer.LocalAuthorityDisposalCostsSectionOne.NorthernIrelandTotalWithBadDebtProvision, feeForLADisposalCosts1?["northernIrelandTotalForLADisposalCostsWithBadDebtProvision"]!);
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
            Assert.AreEqual(producer.ProducerId, roundTrippedData[0]!?["producerID"]?.ToString());
            Assert.AreEqual(producer.SubsidiaryId, roundTrippedData[0]!?["subsidiaryID"]?.ToString());
            Assert.AreEqual(producer.ProducerName, roundTrippedData[0]!?["producerName"]?.ToString());
            Assert.AreEqual(producer?.TradingName, roundTrippedData[0]!?["tradingName"]?.ToString());
            Assert.AreEqual(producer?.Level ?? "1" , roundTrippedData[0]!?["level"]?.ToString());
            Assert.AreEqual(producer?.IsProducerScaledup ?? "No", roundTrippedData[0]!?["scaledUpTonnages"]?.ToString());
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
            var costs = roundTrippedData[0]!["feeForLADataPrepCostsWithBadDebtProvision_4"];
            Assert.IsNotNull(costs);

            var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
            Assert.IsNotNull(producer);
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.LocalAuthorityDataPreparationCosts!.TotalProducerFeeWithoutBadDebtProvision), costs["totalProducerFeeForLADataPrepCostsWithoutBadDebtProvision"]!.GetValue<String>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.LocalAuthorityDataPreparationCosts.BadDebtProvision), costs["badDebtProvisionFor4"]!.GetValue<String>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.LocalAuthorityDataPreparationCosts.TotalProducerFeeWithBadDebtProvision), costs["totalProducerFeeForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.LocalAuthorityDataPreparationCosts.EnglandTotalWithBadDebtProvision), costs["englandTotalForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.LocalAuthorityDataPreparationCosts.WalesTotalWithBadDebtProvision), costs["walesTotalForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.LocalAuthorityDataPreparationCosts.ScotlandTotalWithBadDebtProvision), costs["scotlandTotalForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
            Assert.AreEqual(CurrencyConverter.ConvertToCurrency(producer.LocalAuthorityDataPreparationCosts.NorthernIrelandTotalWithBadDebtProvision), costs["northernIrelandTotalForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
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
            Assert.AreEqual(producer.ProducerId, calculationResult["producerID"]?.GetValue<string>());
            Assert.AreEqual(producer.SubsidiaryId, calculationResult["subsidiaryID"]?.GetValue<string>());
            Assert.AreEqual(producer.ProducerName, calculationResult["producerName"]?.GetValue<string>());
            Assert.AreEqual(producer.TradingName!, calculationResult["tradingName"]?.GetValue<string>());
            Assert.AreEqual(int.Parse(producer.Level!), calculationResult["level"]?.GetValue<int>());
            Assert.AreEqual(producer.IsProducerScaledup, calculationResult["scaledUpTonnages"]?.GetValue<string>());

            // Sub-Sections
            var producerDisposalFeesWithBadDebtProvision1 = roundTrippedData[0]!["producerDisposalFeesWithBadDebtProvision1"];
            Assert.IsNotNull(producerDisposalFeesWithBadDebtProvision1);
            var disposalFeeSummary1 = roundTrippedData[0]!["disposalFeeSummary1"];
            Assert.IsNotNull(disposalFeeSummary1);
            var feesForCommsCostsWithBadDebtProvision2a = roundTrippedData[0]!["feesForCommsCostsWithBadDebtProvision2a"];
            Assert.IsNotNull(feesForCommsCostsWithBadDebtProvision2a);
            var commsCostsByMaterialFeesSummary2a = roundTrippedData[0]!["commsCostsByMaterialFeesSummary2a"];
            Assert.IsNotNull(commsCostsByMaterialFeesSummary2a);
            var feeForLADisposalCosts1 = roundTrippedData[0]!["feeForLADisposalCosts1"];
            Assert.IsNotNull(feeForLADisposalCosts1);
            var feeForCommsCostsWithBadDebtProvision_2a = roundTrippedData[0]!["feeForCommsCostsWithBadDebtProvision_2a"];
            Assert.IsNotNull(feeForCommsCostsWithBadDebtProvision_2a);
            var feeForCommsCostsWithBadDebtProvision_2b = roundTrippedData[0]!["feeForCommsCostsWithBadDebtProvision_2b"];
            Assert.IsNotNull(feeForCommsCostsWithBadDebtProvision_2b);
            var feeForCommsCostsWithBadDebtProvision_2c = roundTrippedData[0]!["feeForCommsCostsWithBadDebtProvision_2c"];
            Assert.IsNotNull(feeForCommsCostsWithBadDebtProvision_2c);
            var totalProducerFeeWithBadDebtProvisionFor2con_1_2a_2b_2c = roundTrippedData[0]!["totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c"];
            Assert.IsNotNull(totalProducerFeeWithBadDebtProvisionFor2con_1_2a_2b_2c);
            var feeForSAOperatingCostsWithBadDebtProvision_3 = roundTrippedData[0]!["feeForSAOperatingCostsWithBadDebtProvision_3"];
            Assert.IsNotNull(feeForSAOperatingCostsWithBadDebtProvision_3);
            var feeForLADataPrepCostsWithBadDebtProvision_4 = roundTrippedData[0]!["feeForLADataPrepCostsWithBadDebtProvision_4"];
            Assert.IsNotNull(feeForLADataPrepCostsWithBadDebtProvision_4);
            var feeForSASetUpCostsWithBadDebtProvision_5 = roundTrippedData[0]!["feeForSASetUpCostsWithBadDebtProvision_5"];
            Assert.IsNotNull(feeForSASetUpCostsWithBadDebtProvision_5);
            var totalProducerBillWithBadDebtProvision = roundTrippedData[0]!["totalProducerBillWithBadDebtProvision"];
            Assert.IsNotNull(totalProducerBillWithBadDebtProvision);
            var calculationOfSuggestedBillingInstructionsAndInvoiceAmounts = roundTrippedData[0]!["calculationOfSuggestedBillingInstructionsAndInvoiceAmounts"];
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
            AssertAreEqual(producer.ProducerId, calculationResult["producerID"]);
        }        
    }
}
