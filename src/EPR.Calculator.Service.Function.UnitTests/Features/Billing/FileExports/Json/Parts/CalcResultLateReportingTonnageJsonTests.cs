using EPR.Calculator.Service.Function.Features.Billing.FileExports.Json.Parts;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing.FileExports.Json.Parts;

[TestClass]
public class CalcResultLateReportingTonnageJsonTests
{
    [TestMethod]
    public void From_HandlesNullOrPopulated()
    {
        var data = TestData.GetCalcResultLateReportingTonnage();

        var result = CalcResultLateReportingTonnageJson.From(data);

        Assert.IsNotNull(result);
        Assert.AreEqual("Late Reporting Tonnage", result.Name);

        var details = result.calcResultLateReportingTonnageDetails;
        Assert.AreEqual(8, details.Count);
        var aluminium = details.Find(d => d.MaterialName == "Aluminium");
        var plastic = details.Find(d => d.MaterialName == "Plastic");
        Assert.IsNotNull(aluminium);
        Assert.IsNotNull(plastic);
        Assert.AreEqual(8000.00m, aluminium.TotalLateReportingTonnage);
        Assert.AreEqual(2000.00m, plastic.TotalLateReportingTonnage);
        Assert.AreEqual(10020.00m, result.CalcResultLateReportingTonnageTotal);
    }
}