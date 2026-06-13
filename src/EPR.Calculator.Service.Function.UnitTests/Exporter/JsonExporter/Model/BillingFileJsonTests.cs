using EPR.Calculator.Service.Function.JsonExporter.Model;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.JsonExporter.Model;

[TestClass]
public class BillingFileJsonTests
{
    [TestMethod]
    public void From_MapFieldsCorrectly()
    {
        var calcResult = TestDataHelper.GetCalcResult();
        var materials = TestDataHelper.GetMaterialDetails();
        var result = BillingFileJson.From(TestDataHelper.BillingRun2025, calcResult, materials);

        Assert.IsNotNull(result);

        // Root-level billing fields
        Assert.AreEqual(calcResult.CalcResultDetail.RunId, result.RunId);
        Assert.AreEqual(calcResult.CalcResultDetail.RelativeYear.ToFinancialYear(), result.FinancialYear);
        Assert.IsNotNull(result.BadDebtProvisionPercentage);

        // Existing detail sections still present
        Assert.IsNotNull(result.CalcResultDetail);
        Assert.AreEqual(1, result.CalcResultDetail!.RunId);
        Assert.IsNotNull(result.CalcResultLapcapData);

        var lapcap = result.CalcResultLapcapData as CalcResultLapcapDataJson;
        Assert.IsNotNull(lapcap);
        Assert.IsNotNull(lapcap.CalcResultLapcapDataTotal);
        Assert.AreEqual("£203,150.00", lapcap.CalcResultLapcapDataTotal!.TotalLaDisposalCost);

        var ladetails = result.CalcResultLaDisposalCostData!.CalcResultLaDisposalCostDetails.ToList();
        Assert.IsTrue(ladetails.Any(d => d.DisposalCostPricePerTonne == "£5.4000"));

        Assert.IsNotNull(result.CalcResult2aCommsDataByMaterial);
        var comms = result.CalcResult2aCommsDataByMaterial!.CalcResult2aCommsDataDetails;
        Assert.IsTrue(comms.Any(d => d.MaterialName == "Aluminium"));
        var aluminium = comms.Single(d => d.MaterialName == "Aluminium");
        Assert.AreEqual("£0.4200", aluminium.CommsCostByMaterialPricePerTonne);

        Assert.IsNotNull(result.CalcResult2bCommsDataByUkWide);
        Assert.AreEqual("£1,500.00", result.CalcResult2bCommsDataByUkWide!.EnglandCommsCostUKWide);

        Assert.IsNotNull(result.CalcResult2cCommsDataByCountry);
        Assert.AreEqual("£250.00", result.CalcResult2cCommsDataByCountry.WalesCommsCostByCountry);

        Assert.IsNotNull(result.ParametersCommsCost);
        var onePlusFourPct = result.ParametersCommsCost!.OnePlusFourCommsCostApportionmentPercentages;
        Assert.IsNotNull(onePlusFourPct);
        Assert.AreEqual("50.23000000%", onePlusFourPct.England);
        Assert.AreEqual("30.34000000%", onePlusFourPct.Wales);

        Assert.IsNotNull(result.ScaleUpProducers!.ProducerSubmissions);
        var subs = result.ScaleUpProducers.ProducerSubmissions!.ToList();
        Assert.IsTrue(subs.Count >= 1);
        Assert.AreEqual(1, subs[0].ProducerId);

        // New producers and materials arrays replace calculationResults
        Assert.IsNotNull(result.Producers);
        Assert.IsNotNull(result.Materials);
    }

    [TestMethod]
    public void From_Materials_ContainsRagPrices()
    {
        var calcResult = TestDataHelper.GetCalcResult();
        var materials  = TestDataHelper.GetMaterialDetails();
        var result     = BillingFileJson.From(TestDataHelper.BillingRun2025, calcResult, materials);

        var materialsList = result.Materials!.ToList();
        Assert.AreEqual(materials.Count, materialsList.Count);
        Assert.IsTrue(materialsList.All(m => m.DisposalPricePerTonne.RedAndRedMedical != null));
        Assert.IsTrue(materialsList.All(m => m.CommsPricePerTonne != null));
    }

    [TestMethod]
    public void From_Producers_ContainsAcceptedProducers()
    {
        var calcResult = TestDataHelper.GetCalcResult();
        var materials  = TestDataHelper.GetMaterialDetails();
        var result     = BillingFileJson.From(TestDataHelper.BillingRun2025, calcResult, materials);

        var producersList = result.Producers!.ToList();
        Assert.IsTrue(producersList.Count > 0);

        var first = producersList[0];
        Assert.IsNotNull(first.TotalBill);
        Assert.IsNotNull(first.Invoice);
        Assert.IsNotNull(first.DisposalFeesByMaterial);
        Assert.IsNotNull(first.DisposalCosts);
        Assert.IsNotNull(first.CommsCostsByMaterial);
        Assert.IsNotNull(first.CommsCostsUKWide);
        Assert.IsNotNull(first.CommsCostsByCountry);
        Assert.IsNotNull(first.SaOperatingCosts);
        Assert.IsNotNull(first.LaDataPrepCosts);
        Assert.IsNotNull(first.SaSetUpCosts);
    }
}
