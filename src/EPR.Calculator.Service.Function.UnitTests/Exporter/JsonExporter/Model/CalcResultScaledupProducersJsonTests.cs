using EPR.Calculator.Service.Function.JsonExporter.Model;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.JsonExporter.Model;

[TestClass]
public class CalcResultScaledupProducersJsonTests
{
    [TestMethod]
    public void From_ProducesProducerSubmissions()
    {
        var runContext = TestDataHelper.BillingRun2025 with { AcceptedProducerIds = [101001] };
        var scaled = TestDataHelper.GetScaledupProducers();
        var materials = TestDataHelper.GetMaterialDetails();

        var result = CalcResultScaledupProducersJson.From(runContext, scaled, materials);
        Assert.IsNotNull(result);
        var submissions = result.ProducerSubmissions;
        Assert.IsNotNull(submissions);
        var list = submissions.ToList();
        Assert.AreEqual(0, list.Count);

        var runContext2 = TestDataHelper.BillingRun2025;
        var withRealAccepted = CalcResultScaledupProducersJson.From(runContext2, scaled, materials);
        var realList = withRealAccepted.ProducerSubmissions!.ToList();
        Assert.AreEqual(1, realList.Count);
        var first = realList[0];
        Assert.AreEqual(1, first.ProducerId);
        var al = first.MaterialBreakdowns.Single(m => m.MaterialName == "Aluminium");
        Assert.AreEqual(100m, al.ReportedHouseholdPackagingWasteTonnage);
        Assert.AreEqual(200m, al.ScaledUpReportedHouseholdPackagingWasteTonnage);
    }
}
