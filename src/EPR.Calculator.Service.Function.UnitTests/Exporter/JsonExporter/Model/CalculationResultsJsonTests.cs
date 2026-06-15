using System.Text.Json;
using System.Text.Json.Nodes;
using EPR.Calculator.Service.Function.JsonExporter.Model;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.JsonExporter.Model;

/// <summary>
/// Tests for the per-producer fields produced by BillingFileJson.From() in the "producers" array,
/// replacing the removed CalculationResultsJson class.
/// </summary>
[TestClass]
public class ProducerResultsTests
{
    private static JsonNode GetProducers()
    {
        var calcResult = TestDataHelper.GetCalcResult();
        var materials  = TestDataHelper.GetMaterialDetails();
        var producers  = calcResult.CalcResultSummary.ProducerDisposalFees
            .Where(p => TestDataHelper.BillingRun2025.AcceptedProducerIds.Contains(p.ProducerId))
            .Select(p => ProducerResult.From(p, materials, applyModulation: false))
            .ToList();
        return JsonNode.Parse(JsonSerializer.Serialize(producers))!;
    }

    private static CalcResultSummaryProducerDisposalFees GetFirstProducer()
    {
        var calcResult = TestDataHelper.GetCalcResult();
        return calcResult.CalcResultSummary.ProducerDisposalFees
            .First(t => TestDataHelper.BillingRun2025.AcceptedProducerIds.Contains(t.ProducerId));
    }

    private static string F2(decimal value) =>
        value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

    [TestMethod]
    public void From_ProducerIdentity_IsValid()
    {
        var producers = GetProducers();
        var producer  = GetFirstProducer();

        Assert.IsNotNull(producers[0]);
        Assert.AreEqual(producer.ProducerId.ToString(), producers[0]!["producerID"]?.ToString());
        Assert.AreEqual(producer.SubsidiaryId,          producers[0]!["subsidiaryID"]?.ToString());
        Assert.AreEqual(producer.ProducerName,          producers[0]!["producerName"]?.ToString());
    }

    [TestMethod]
    public void From_DisposalCosts_AreValid()
    {
        var producers = GetProducers();
        var producer  = GetFirstProducer();
        var actual    = producers[0]!["disposalCosts"]!;
        var expected  = producer.LADisposalCostsSection1;

        Assert.AreEqual(F2(expected.FeeWithoutBadDebtProvision),       actual["base"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.BadDebtProvision),                  actual["badDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.FeeWithBadDebtProvision.Total),     actual["total"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.FeeWithBadDebtProvision.England),   actual["england"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.FeeWithBadDebtProvision.Wales),     actual["wales"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.FeeWithBadDebtProvision.Scotland),  actual["scotland"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.FeeWithBadDebtProvision.NorthernIreland), actual["northernIreland"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_CommsCostsByMaterial_AreValid()
    {
        var producers = GetProducers();
        var producer  = GetFirstProducer();
        var actual    = producers[0]!["commsCostsByMaterial"]!;
        var expected  = producer.CommsCostsSection2a;

        Assert.AreEqual(F2(expected.FeeWithoutBadDebtProvision), actual["base"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.BadDebtProvision),            actual["badDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_CommsCostsUKWide_AreValid()
    {
        var producers = GetProducers();
        var producer  = GetFirstProducer();
        var actual    = producers[0]!["commsCostsUKWide"]!;
        var expected  = producer.CommsCostsSection2b;

        Assert.AreEqual(F2(expected.FeeWithoutBadDebtProvision), actual["base"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.BadDebtProvision),            actual["badDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_CommsCostsByCountry_AreValid()
    {
        var producers = GetProducers();
        var producer  = GetFirstProducer();
        var actual    = producers[0]!["commsCostsByCountry"]!;
        var expected  = producer.CommsCostsSection2c;

        Assert.AreEqual(F2(expected.FeeWithoutBadDebtProvision), actual["base"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.BadDebtProvision),            actual["badDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_SaOperatingCosts_AreValid()
    {
        var producers = GetProducers();
        var producer  = GetFirstProducer();
        var actual    = producers[0]!["saOperatingCosts"]!;
        var expected  = producer.SaOperatingCostsSection3;

        Assert.AreEqual(F2(expected.FeeWithoutBadDebtProvision), actual["base"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.BadDebtProvision),            actual["badDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_LaDataPrepCosts_AreValid()
    {
        var producers = GetProducers();
        var producer  = GetFirstProducer();
        var actual    = producers[0]!["laDataPrepCosts"]!;
        var expected  = producer.LaDataPrepSection4;

        Assert.AreEqual(F2(expected.FeeWithoutBadDebtProvision), actual["base"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.BadDebtProvision),            actual["badDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_SaSetUpCosts_AreValid()
    {
        var producers = GetProducers();
        var producer  = GetFirstProducer();
        var actual    = producers[0]!["saSetUpCosts"]!;
        var expected  = producer.SaSetupCostsSection5;

        Assert.AreEqual(F2(expected.FeeWithoutBadDebtProvision), actual["base"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.BadDebtProvision),            actual["badDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_TotalBill_IsValid()
    {
        var producers = GetProducers();
        var producer  = GetFirstProducer();
        var expected  = producer.TotalProducerBillBreakdownCosts;

        Assert.AreEqual(F2(expected.FeeWithBadDebtProvision.Total), producers[0]!["totalBill"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_Invoice_IsPresent()
    {
        var producers = GetProducers();
        var invoice   = producers[0]!["invoice"];
        Assert.IsNotNull(invoice);
        Assert.IsNotNull(invoice!["instruction"]);
    }

    [TestMethod]
    public void From_DisposalFeesByMaterial_ContainsMaterialBreakdown()
    {
        var producers  = GetProducers();
        var producer   = GetFirstProducer();
        var byMaterial = producers[0]!["disposalFeesByMaterial"]!.AsArray();

        Assert.IsNotNull(byMaterial);
        Assert.IsTrue(byMaterial.Count > 0);

        var firstMaterial = producer.ProducerDisposalFeesByMaterial.First();
        var actual = byMaterial[0]!;

        Assert.IsNotNull(actual["material"]);
        Assert.IsNotNull(actual["feeWithoutBadDebt"]);
        Assert.IsNotNull(actual["fee"]);
        Assert.IsNotNull(actual["badDebtProvision"]);
        Assert.IsNotNull(actual["england"]);
        Assert.AreEqual(
            firstMaterial.Value.BadDebtProvision.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
            actual["badDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_Materials_ContainsAllMaterials()
    {
        var calcResult    = TestDataHelper.GetCalcResult();
        var materials     = TestDataHelper.GetMaterialDetails();
        var materialsList = MaterialPrices.FromAll(materials, calcResult).ToList();

        Assert.AreEqual(materials.Count, materialsList.Count);
        Assert.IsTrue(materialsList.All(m => m.DisposalPricePerTonne != null));
        Assert.IsTrue(materialsList.All(m => m.CommsPricePerTonne != null));
    }
}
