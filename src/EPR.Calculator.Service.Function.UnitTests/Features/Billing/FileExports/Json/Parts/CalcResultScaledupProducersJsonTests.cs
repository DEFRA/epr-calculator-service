using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Billing.FileExports.Json.Parts;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing.FileExports.Json.Parts;

[TestClass]
public class CalcResultScaledupProducersJsonTests
{
    [TestMethod]
    public void From_ProducesProducerSubmissions()
    {
        var runContext = TestFixtures.Default.Create<BillingRunContext>() with { AcceptedProducerIds = [101001] };
        var scaled = TestData.GetScaledupProducers();

        var result = CalcResultScaledupProducersJson.From(runContext, scaled, TestData.Materials);
        Assert.IsNotNull(result);
        var submissions = result.ProducerSubmissions;
        Assert.IsNotNull(submissions);
        var list = submissions.ToList();
        Assert.AreEqual(0, list.Count);

        var acceptedReal = runContext with { AcceptedProducerIds = [1] };
        var withRealAccepted = CalcResultScaledupProducersJson.From(acceptedReal, scaled, TestData.Materials);
        var realList = withRealAccepted.ProducerSubmissions!.ToList();
        Assert.AreEqual(1, realList.Count);
        var first = realList[0];
        Assert.AreEqual(1, first.ProducerId);
        var al = first.MaterialBreakdown.Single(m => m.MaterialName == "Aluminium");
        Assert.AreEqual(100m, al.ReportedHouseholdPackagingWasteTonnage);
        Assert.AreEqual(200m, al.ScaledUpReportedHouseholdPackagingWasteTonnage);
    }
}