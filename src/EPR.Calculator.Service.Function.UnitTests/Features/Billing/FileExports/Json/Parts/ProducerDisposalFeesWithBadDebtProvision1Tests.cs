using EPR.Calculator.Service.Function.Features.Billing.FileExports.Json.Parts;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing.FileExports.Json.Parts;

[TestClass]
public class ProducerDisposalFeesWithBadDebtProvision1Tests
{
    [TestMethod]
    public void From_MapsMaterialBreakdown()
    {
        var byMaterial = TestData.GetProducerDisposalFeesByMaterial();

        var result = ProducerDisposalFeesWithBadDebtProvision1.From(byMaterial, TestData.Materials, "1");

        Assert.IsNotNull(result);
        var list = result.MaterialBreakdown.ToList();
        var al = list.Single(m => m.MaterialName == "Aluminium");
        Assert.AreEqual(1000m, al.HouseholdPackagingWasteTonnage);
        Assert.AreEqual("£0.6676", al.PricePerTonne);
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(643.97m), al.ProducerDisposalFeeWithBadDebtProvision);

        var gl = list.Single(m => m.MaterialName == "Glass");
        Assert.AreEqual(220m, gl.HouseholdDrinksContainersTonnageGlass);
    }
}