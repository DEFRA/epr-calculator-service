namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    using System.Linq;
    using EPR.Calculator.Service.Function.Models.JsonExporter;
    using EPR.Calculator.Service.Function.UnitTests.Builder;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            Assert.AreEqual(EPR.Calculator.Service.Function.Constants.CommonConstants.LADisposalCostData, result.Name);
            Assert.AreEqual(63005.000m, result.CalcResultLaDisposalCostDataDetailsTotal.ProducerHouseholdPackagingWasteTonnageTotal);
        }
    }
}
