using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Billing.FileExports.Json.Parts;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing.FileExports.Json.Parts;

[TestClass]
public class CalcResultLaDisposalCostDataJsonTests
{
    [TestMethod]
    public void From_ConvertsLaDisposalDetailsAndTotal()
    {
        var la = TestDataHelper.GetCalcResultLaDisposalCostData();
        var result = CalcResultLaDisposalCostDataJson.From(la.CalcResultLaDisposalCostDetails);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.CalcResultLaDisposalCostDetails.Any());
        Assert.IsNotNull(result.CalcResultLaDisposalCostDataDetailsTotal);

        Assert.AreEqual(CommonConstants.LADisposalCostData, result.Name);
        Assert.AreEqual(63005.000m, result.CalcResultLaDisposalCostDataDetailsTotal.ProducerHouseholdPackagingWasteTonnageTotal);
    }
}