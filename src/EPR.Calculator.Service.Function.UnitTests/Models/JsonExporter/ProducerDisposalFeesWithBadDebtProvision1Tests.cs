namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    using System.Linq;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Models.JsonExporter;
    using EPR.Calculator.Service.Function.UnitTests.Builder;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProducerDisposalFeesWithBadDebtProvision1Tests
    {
        [TestMethod]
        public void From_MapsMaterialBreakdown()
        {
            var byMaterial = TestDataHelper.GetProducerDisposalFeesByMaterial();
            var materials = TestDataHelper.GetMaterials();

            var result = ProducerDisposalFeesWithBadDebtProvision1.From(byMaterial, materials, "1");

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
}
