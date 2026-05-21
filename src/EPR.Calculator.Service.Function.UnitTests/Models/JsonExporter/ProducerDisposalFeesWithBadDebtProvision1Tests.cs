using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class ProducerDisposalFeesWithBadDebtProvision1Tests
    {
        [TestMethod]
        public void From_MapsMaterialBreakdown()
        {
            var byMaterial = TestDataHelper.GetProducerDisposalFeesByMaterial();
            var materials = TestDataHelper.GetMaterials();

            var result = ProducerDisposalFeesWithBadDebtProvision1.From(byMaterial, materials, "1", applyModulation: false);

            Assert.IsNotNull(result);
            var list = result.MaterialBreakdown.ToList();
            var al = list.Single(m => m.MaterialName == "Aluminium");
            Assert.AreEqual(1000m, al.HouseholdPackagingWasteTonnage);
            Assert.AreEqual("£0.6676", al.PricePerTonne);
            Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(643.97m), al.ProducerDisposalFeeWithBadDebtProvision);

            var gl = list.Single(m => m.MaterialName == "Glass");
            Assert.AreEqual(220m, gl.HouseholdDrinksContainersTonnageGlass);
        }

        [TestMethod]
        public void From_MapsMaterialBreakdown_Modulated()
        {
            var byMaterial = TestDataHelper.GetProducerDisposalFeesByMaterial(applyModulation: true);
            var materials = TestDataHelper.GetMaterials();

            var result = ProducerDisposalFeesWithBadDebtProvision1.From(byMaterial, materials, "1", applyModulation: true);

            Assert.IsNotNull(result);
            var list = result.MaterialBreakdown.ToList();
            var al = list.Single(m => m.MaterialName == "Aluminium");
            Assert.AreEqual(1000m, al.HouseholdPackagingWasteTonnage.Modulated!.Total);
            Assert.AreEqual("£1.0000", al.PricePerTonne.Modulated!.RedAndRedMedical);
            Assert.AreEqual("£2.0000", al.PricePerTonne.Modulated!.AmberAndAmberMedical);
            Assert.AreEqual("£3.0000", al.PricePerTonne.Modulated!.GreenAndGreenMedical);
            Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(643.97m), al.ProducerDisposalFeeWithBadDebtProvision);

            var gl = list.Single(m => m.MaterialName == "Glass");
            Assert.AreEqual(220m, gl.HouseholdDrinksContainersTonnageGlass!.Modulated!.Total);
        }
    }
}
