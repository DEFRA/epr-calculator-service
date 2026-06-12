using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.JsonExporter.Model;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using EPR.Calculator.Service.Function.UnitTests.Utils;
using EPR.Calculator.Service.Function.Utils;
using static EPR.Calculator.Service.Function.JsonExporter.Model.CalculationResultsJson;

namespace EPR.Calculator.Service.Function.UnitTests.JsonExporter.Model;

using static JsonNodeComparer;

[TestClass]
public class CalculationResultsJsonFromTests
{
    [TestMethod]
    public void From_ValuesAreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.CalcResultSummary;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);

        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!["producerCalculationResultsSummary"];

        // Assert
        Assert.IsNotNull(roundTrippedData);

        // 1
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.LADisposalCostsSection1.FeeWithoutBadDebtProvision),
            roundTrippedData["feeForLaDisposalCostsWithoutBadDebtprovision1"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.LADisposalCostsSection1.BadDebtProvision),
            roundTrippedData["badDebtProvision1"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.LADisposalCostsSection1.FeeWithBadDebtProvision.Total),
            roundTrippedData["feeForLaDisposalCostsWithBadDebtprovision1"]);

        // 2a
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSection2a.FeeWithoutBadDebtProvision),
            roundTrippedData["feeForCommsCostsByMaterialWithoutBadDebtprovision2a"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSection2a.BadDebtProvision),
            roundTrippedData["badDebtProvision2a"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSection2a.FeeWithBadDebtProvision.Total),
            roundTrippedData["feeForCommsCostsByMaterialWitBadDebtprovision2a"]);

        // 2b
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSection2b.FeeWithoutBadDebtProvision),
            roundTrippedData["feeForCommsCostsUkWideWithoutBadDebtprovision2b"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSection2b.BadDebtProvision),
            roundTrippedData["badDebtProvision2b"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSection2b.FeeWithBadDebtProvision.Total),
            roundTrippedData["feeForCommsCostsUkWideWithBadDebtprovision2b"]);

        // 2c
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSection2c.FeeWithoutBadDebtProvision),
            roundTrippedData["feeForCommsCostsByCountryWithoutBadDebtprovision2c"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSection2c.BadDebtProvision),
            roundTrippedData["badDebtProvision2c"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.CommsCostsSection2c.FeeWithBadDebtProvision.Total),
            roundTrippedData["feeForCommsCostsByCountryWideWithBadDebtprovision2c"]);

        // 1+2a+2b+2c
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.TotalOnePlus2A2B2CFeeWithBadDebtProvision),
            roundTrippedData["total12a2b2cWithBadDebt"]);

        // 3
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.SaOperatingCostsSection3.FeeWithoutBadDebtProvision),
            roundTrippedData["saOperatingCostsWithoutBadDebtProvision3"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.SaOperatingCostsSection3.BadDebtProvision),
            roundTrippedData["badDebtProvision3"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.SaOperatingCostsSection3.FeeWithBadDebtProvision.Total),
            roundTrippedData["saOperatingCostsWithBadDebtProvision3"]);

        // 4
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.LaDataPrepSection4.FeeWithoutBadDebtProvision),
            roundTrippedData["laDataPrepCostsWithoutBadDebtProvision4"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.LaDataPrepSection4.BadDebtProvision),
            roundTrippedData["badDebtProvision4"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.LaDataPrepSection4.FeeWithBadDebtProvision.Total),
            roundTrippedData["laDataPrepCostsWithbadDebtProvision4"]);

        // 5
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.SaSetupCostsSection5.FeeWithoutBadDebtProvision),
            roundTrippedData["oneOffFeeSaSetupCostsWithoutBadDebtProvision5"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.SaSetupCostsSection5.BadDebtProvision),
            roundTrippedData["badDebtProvision5"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(data.SaSetupCostsSection5.FeeWithBadDebtProvision.Total),
            roundTrippedData["oneOffFeeSaSetupCostsWithBadDebtProvision5"]);
    }

    [TestMethod]
    public void From_ProducerDisposalFeesWithBadDebtProvision1_ReturnsValidValues()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var materials  = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);

        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);

        var actual = roundTrippedData[0]!["producerDisposalFeesWithBadDebtProvision1"]!["materialBreakdown"]![0]!;
        var producer = calcResult.CalcResultSummary.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level))!;
        var expected = producer.ProducerDisposalFeesByMaterial.First();

        decimal? actualValue = 0;
        if(actual["previousInvoicedTonnage"]?.ToString() == "-")
        {
            actualValue = null;
        }

        Assert.AreEqual(expected.Value.PreviousInvoicedTonnage, actualValue);
        Assert.AreEqual(expected.Value.HouseholdPackagingWasteTonnage, actual["householdPackagingWasteTonnage"]!.GetValue<decimal>());
        Assert.AreEqual(expected.Value.PublicBinTonnage, actual["publicBinTonnage"]!.GetValue<decimal>());
        Assert.AreEqual(expected.Value.TotalReportedTonnage, actual["totalTonnage"]!.GetValue<decimal>());
        Assert.AreEqual(expected.Value.SelfManagedConsumerWasteTonnage, actual["selfManagedConsumerWasteTonnage"]!.GetValue<decimal>());
        Assert.AreEqual(expected.Value.NetReportedTonnage.total, actual["netTonnage"]!.GetValue<decimal>());

        var actualPrev = ReadNullableDecimal(actual, "previousInvoicedTonnage");
        Assert.AreEqual(expected.Value.PreviousInvoicedTonnage, actualPrev);

        var actualChange = ReadNullableDecimal(actual, "tonnageChange");
        Assert.AreEqual(expected.Value.TonnageChange, actualChange);

        Assert.AreEqual("£0.6676", actual["pricePerTonne"]!.GetValue<string>());
        Assert.AreEqual("£607.52", actual["producerDisposalFeeWithoutBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual("£36.45" , actual["badDebtProvision"]!.GetValue<string>());
        Assert.AreEqual("£643.98", actual["producerDisposalFeeWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual("£348.06", actual["englandWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual("£78.46" , actual["walesWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual("£156.28", actual["scotlandWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual("£61.18" , actual["northernIrelandWithBadDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_TotalProducerFeeWithBadDebtProvisionFor2con_1_2a_2b_2c_ReturnsValidValues()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.CalcResultSummary;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);

        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);
        var actual = roundTrippedData[0]!["totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c"]!;
        var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level))!;

        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.ProducerTotalOnePlus2A2B2CWithBadDeptProvision), actual["totalFeeWithBadDebtProvision"]);
        AssertAreEqual($"{producer.ProducerOverallPercentageOfCostsForOnePlus2A2B2C.ToString("F8")}%", actual["producerPercentageOfOverallProducerCost"]);
    }

    [TestMethod]
    public void From_CommsCost2AValues_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.CalcResultSummary;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);

        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);
        var twoACosts = roundTrippedData[0]?["commsCostsByMaterialFeesSummary2a"];
        var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
        Assert.IsNotNull(producer);
        AssertAreEqual(producer.CommsCostsSection2a.FeeWithoutBadDebtProvision, twoACosts?["totalProducerFeeForCommsCostsWithoutBadDebtProvision2a"]!);
        AssertAreEqual(producer.CommsCostsSection2a.BadDebtProvision, twoACosts?["totalBadDebtProvision"]);
        AssertAreEqual(producer.CommsCostsSection2a.FeeWithBadDebtProvision.Total, twoACosts?["totalProducerFeeForCommsCostsWithBadDebtProvision2a"]!);
        AssertAreEqual(producer.CommsCostsSection2a.FeeWithBadDebtProvision.England, twoACosts?["englandTotalWithBadDebtProvision"]!);
        AssertAreEqual(producer.CommsCostsSection2a.FeeWithBadDebtProvision.Wales, twoACosts?["walesTotalWithBadDebtProvision"]!);
        AssertAreEqual(producer.CommsCostsSection2a.FeeWithBadDebtProvision.Scotland, twoACosts?["scotlandTotalWithBadDebtProvision"]!);
        AssertAreEqual(producer.CommsCostsSection2a.FeeWithBadDebtProvision.NorthernIreland, twoACosts?["northernIrelandTotalWithBadDebtProvision"]!);
    }

    [TestMethod]
    public void From_FeeForSASetUpCostsWithBadDebtProvision_5_ReturnsValidValues()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.CalcResultSummary;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);
        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);
        var actual = roundTrippedData[0]!["feeForSASetUpCostsWithBadDebtProvision_5"]!;
        var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level))!;

        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.SaSetupCostsSection5!.FeeWithoutBadDebtProvision), actual["totalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.SaSetupCostsSection5.BadDebtProvision), actual["badDebtProvisionFor5"]!.GetValue<string>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.SaSetupCostsSection5.FeeWithBadDebtProvision.Total), actual["totalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.SaSetupCostsSection5.FeeWithBadDebtProvision.England), actual["englandTotalForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.SaSetupCostsSection5.FeeWithBadDebtProvision.Wales), actual["walesTotalForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.SaSetupCostsSection5.FeeWithBadDebtProvision.Scotland), actual["scotlandTotalForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.SaSetupCostsSection5.FeeWithBadDebtProvision.NorthernIreland), actual["northernIrelandTotalForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_CommsCost3SA_Operating_Costs_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.CalcResultSummary;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);

        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);
        var threeSACosts = roundTrippedData[0]!["feeForSAOperatingCostsWithBadDebtProvision_3"];
        var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
        Assert.IsNotNull(producer);
        Assert.IsNotNull(threeSACosts);
        AssertAreEqual(producer.SaOperatingCostsSection3!.FeeWithoutBadDebtProvision            , threeSACosts["totalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision"]!);
        AssertAreEqual(producer.SaOperatingCostsSection3.BadDebtProvision                       , threeSACosts["badDebProvisionFor3"]!);
        AssertAreEqual(producer.SaOperatingCostsSection3.FeeWithBadDebtProvision.Total          , threeSACosts["totalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision"]!);
        AssertAreEqual(producer.SaOperatingCostsSection3.FeeWithBadDebtProvision.England        , threeSACosts["englandTotalForSAOperatingCostsWithBadDebtProvision"]!);
        AssertAreEqual(producer.SaOperatingCostsSection3.FeeWithBadDebtProvision.Wales          , threeSACosts["walesTotalForSAOperatingCostsWithBadDebtProvision"]!);
        AssertAreEqual(producer.SaOperatingCostsSection3.FeeWithBadDebtProvision.Scotland       , threeSACosts["scotlandTotalForSAOperatingCostsWithBadDebtProvision"]!);
        AssertAreEqual(producer.SaOperatingCostsSection3.FeeWithBadDebtProvision.NorthernIreland, threeSACosts["northernIrelandTotalForSAOperatingCostsWithBadDebtProvision"]!);
    }

    [TestMethod]
    public void From_FeeForCommsCostsWithBadDebtProvision2a_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.CalcResultSummary;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);
        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);
        var twoACosts = roundTrippedData[0]!["feeForCommsCostsWithBadDebtProvision_2a"];
        Assert.IsNotNull(twoACosts);
        var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
        Assert.IsNotNull(producer);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2a.FeeWithoutBadDebtProvision), twoACosts["totalProducerFeeForCommsCostsWithoutBadDebtProvision"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2a.BadDebtProvision), twoACosts["badDebtProvisionFor2a"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2a.FeeWithBadDebtProvision.Total), twoACosts["totalProducerFeeForCommsCostsWithBadDebtProvision"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2a.FeeWithBadDebtProvision.England), twoACosts["englandTotalWithBadDebtProvision"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2a.FeeWithBadDebtProvision.Wales), twoACosts["walesTotalWithBadDebtProvision"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2a.FeeWithBadDebtProvision.Scotland), twoACosts["scotlandTotalWithBadDebtProvision"]);
        AssertAreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2a.FeeWithBadDebtProvision.NorthernIreland), twoACosts["northernIrelandTotalWithBadDebtProvision"]);
    }

    [TestMethod]
    public void From_FeeForCommsCostsWithBadDebtProvision2b_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.CalcResultSummary;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);
        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);
        var twoBCosts = roundTrippedData[0]!["feeForCommsCostsWithBadDebtProvision_2b"];
        var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
        Assert.IsNotNull(producer);
        Assert.IsNotNull(twoBCosts);
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2b!.FeeWithoutBadDebtProvision            ), twoBCosts["totalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2b.BadDebtProvision                       ), twoBCosts["badDebtProvisionFor2b"                                     ]!.GetValue<string>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2b.FeeWithBadDebtProvision.Total          ), twoBCosts["totalProducerFeeForCommsCostsUKWideWithBadDebtProvision"   ]!.GetValue<string>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2b.FeeWithBadDebtProvision.England        ), twoBCosts["englandTotalWithBadDebtProvision"                          ]!.GetValue<string>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2b.FeeWithBadDebtProvision.Wales          ), twoBCosts["walesTotalWithBadDebtProvision"                            ]!.GetValue<string>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2b.FeeWithBadDebtProvision.Scotland       ), twoBCosts["scotlandTotalWithBadDebtProvision"                         ]!.GetValue<string>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2b.FeeWithBadDebtProvision.NorthernIreland), twoBCosts["northernIrelandTotalWithBadDebtProvision"                  ]!.GetValue<string>());
    }

    [TestMethod]
    public void From_CommsCost2CValues_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.CalcResultSummary;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);
        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);
        var twoCCosts = roundTrippedData[0]!["feeForCommsCostsWithBadDebtProvision_2c"];
        var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level));
        Assert.IsNotNull(producer);
        Assert.IsNotNull(twoCCosts);
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2c.FeeWithoutBadDebtProvision), twoCCosts["totalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2c.BadDebtProvision), twoCCosts["badDebProvisionFor2c"]!.GetValue<string>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2c.FeeWithBadDebtProvision.Total), twoCCosts["totalProducerFeeForCommsCostsByCountryWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2c.FeeWithBadDebtProvision.England), twoCCosts["englandTotalWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2c.FeeWithBadDebtProvision.Wales), twoCCosts["walesTotalWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2c.FeeWithBadDebtProvision.Scotland), twoCCosts["scotlandTotalWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.CommsCostsSection2c.FeeWithBadDebtProvision.NorthernIreland), twoCCosts["northernIrelandTotalWithBadDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_DisposalFeeSummary1()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.CalcResultSummary;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);
        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);
        var disposalFeeSummary1 = roundTrippedData[0]!["disposalFeeSummary1"]!;
        Assert.IsNotNull(roundTrippedData);
        var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level))!;

        // Disposal Fee
        AssertAreEqual(producer.LADisposalCostsSection1.FeeWithoutBadDebtProvision,
            disposalFeeSummary1["totalProducerDisposalFeeWithoutBadDebtProvision"]!);
        AssertAreEqual(producer.LADisposalCostsSection1.BadDebtProvision,
            disposalFeeSummary1["badDebtProvision"]!);
        AssertAreEqual(producer.LADisposalCostsSection1.FeeWithBadDebtProvision.Total,
            disposalFeeSummary1["totalProducerDisposalFeeWithBadDebtProvision"]!);

        // Countries
        AssertAreEqual(producer.LADisposalCostsSection1.FeeWithBadDebtProvision.England,
            disposalFeeSummary1["englandTotal"]!);
        AssertAreEqual(producer.LADisposalCostsSection1.FeeWithBadDebtProvision.Wales,
            disposalFeeSummary1["walesTotal"]!);
        AssertAreEqual(producer.LADisposalCostsSection1.FeeWithBadDebtProvision.Scotland,
            disposalFeeSummary1["scotlandTotal"]!);
        AssertAreEqual(producer.LADisposalCostsSection1.FeeWithBadDebtProvision.NorthernIreland,
            disposalFeeSummary1["northernIrelandTotal"]!);

        // Tonnage Change
        Assert.AreEqual(producer.TonnageChangeCount,
            disposalFeeSummary1["tonnageChangeCount"]?.ToString());
        Assert.AreEqual(producer.TonnageChangeAdvice,
            disposalFeeSummary1["tonnageChangeAdvice"]?.ToString());
    }

    [TestMethod]
    public void From_BillingInstructions_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.CalcResultSummary;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
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

    [TestMethod]
    public void From_FeeForLADisposalCost1_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.CalcResultSummary;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
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
        AssertAreEqual(producer.LADisposalCostsSection1!.FeeWithoutBadDebtProvision, feeForLADisposalCosts1?["totalProducerFeeForLADisposalCostsWithoutBadDebtProvision"]!);
        AssertAreEqual(producer.LADisposalCostsSection1.BadDebtProvision, feeForLADisposalCosts1?["badDebtProvisionForLADisposalCosts"]!);
        AssertAreEqual(producer.LADisposalCostsSection1.FeeWithBadDebtProvision.Total, feeForLADisposalCosts1?["totalProducerFeeForLADisposalCostsWithBadDebtProvision"]!);
        AssertAreEqual(producer.LADisposalCostsSection1.FeeWithBadDebtProvision.England, feeForLADisposalCosts1?["englandTotalForLADisposalCostsWithBadDebtProvision"]!);
        AssertAreEqual(producer.LADisposalCostsSection1.FeeWithBadDebtProvision.Wales, feeForLADisposalCosts1?["walesTotalForLADisposalCostsWithBadDebtProvision"]!);
        AssertAreEqual(producer.LADisposalCostsSection1.FeeWithBadDebtProvision.Scotland, feeForLADisposalCosts1?["scotlandTotalForLADisposalCostsWithBadDebtProvision"]!);
        AssertAreEqual(producer.LADisposalCostsSection1.FeeWithBadDebtProvision.NorthernIreland, feeForLADisposalCosts1?["northernIrelandTotalForLADisposalCostsWithBadDebtProvision"]!);
    }

    [TestMethod]
    public void From_ProducerIdSubsidiaryId_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.CalcResultSummary;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
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
        Assert.AreEqual(producer.ProducerId.ToString(), roundTrippedData[0]!["producerID"]?.ToString());
        Assert.AreEqual(producer.SubsidiaryId, roundTrippedData[0]!["subsidiaryID"]?.ToString());
        Assert.AreEqual(producer.ProducerName, roundTrippedData[0]!["producerName"]?.ToString());
        Assert.AreEqual(producer.TradingName, roundTrippedData[0]!["tradingName"]?.ToString());
        Assert.AreEqual(producer.Level ?? "1" , roundTrippedData[0]!["level"]?.ToString());
        Assert.AreEqual(producer.IsProducerScaledup, roundTrippedData[0]!["scaledUpTonnages"]?.ToString());
    }

    [TestMethod]
    public void From_ProducerCalculationResultsTotal_CanBeNull()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.CalcResultSummary;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);
        var producerCalculationResultsTotal = JsonSerializer.Deserialize<JsonObject>(json)!["producerCalculationResultsTotal"]!;

        // Assert
        Assert.IsNull(producerCalculationResultsTotal);
    }

    [TestMethod]
    public void From_FeeForLADataPrepCostsWithBadDebtProvision_4_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.CalcResultSummary;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
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
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.LaDataPrepSection4!.FeeWithoutBadDebtProvision), costs["totalProducerFeeForLADataPrepCostsWithoutBadDebtProvision"]!.GetValue<String>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.LaDataPrepSection4.BadDebtProvision), costs["badDebtProvisionFor4"]!.GetValue<String>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.LaDataPrepSection4.FeeWithBadDebtProvision.Total), costs["totalProducerFeeForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.LaDataPrepSection4.FeeWithBadDebtProvision.England), costs["englandTotalForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.LaDataPrepSection4.FeeWithBadDebtProvision.Wales), costs["walesTotalForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.LaDataPrepSection4.FeeWithBadDebtProvision.Scotland), costs["scotlandTotalForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(producer.LaDataPrepSection4.FeeWithBadDebtProvision.NorthernIreland), costs["northernIrelandTotalForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
    }

    [TestMethod]
    public void From_CalculationResultsJson_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.CalcResultSummary;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);
        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["producerCalculationResults"]!;

        // Assert
        var calculationResult = roundTrippedData[0]!;
        var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level))!;

        // Main Fields
        Assert.AreEqual(producer.ProducerId.ToString(), calculationResult["producerID"]?.GetValue<string>());
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
    public void From_ProducerCalculationResult_Level1_AreDisplayed()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.CalcResultSummary;
        var materials = TestDataHelper.GetMaterialDetails();

        data.ProducerDisposalFees.First().IsTotalRow = true;
        data.ProducerDisposalFees.First().Level = "1";

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);
        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)![
                "producerCalculationResults"]!;

        // Assert
        var calculationResult = roundTrippedData[0]!;
        var producer = data.ProducerDisposalFees.SingleOrDefault(t => !string.IsNullOrEmpty(t.Level))!;

        // Main Fields
        AssertAreEqual(producer.ProducerId.ToString(), calculationResult["producerID"]);
    }

    private static decimal? ReadNullableDecimal(JsonNode obj, string prop)
    {
        var n = obj[prop];
        if (n is null) return null;

        //May emit numbers or strings ("-" for null)
        var s = n.ToString();
        if (string.Equals(s, "-", StringComparison.Ordinal)) return null;

        return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
            ? d
            : null;
    }

}
