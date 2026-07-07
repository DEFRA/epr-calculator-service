using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.UnitTests.Utils
{
    [TestClass]
    public class TonnageChangeUtilTests
    {
        // ---------- ComputePerMaterialChange ----------

        [TestMethod]
        public void ComputePerMaterialChange_levelNot1_returnsNull()
        {
            var result = TonnageChangeUtil.ComputePerMaterialChange(
                level: "2",
                netReportedTonnage: 100m,
                previousInvoicedTonnage: 80m);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ComputePerMaterialChange_previousNull_returnsNull()
        {
            var result = TonnageChangeUtil.ComputePerMaterialChange(
                level: CommonConstants.LevelOne.ToString(),
                netReportedTonnage: 100m,
                previousInvoicedTonnage: null);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ComputePerMaterialChange_previousZero_returnsZero()
        {
            var result = TonnageChangeUtil.ComputePerMaterialChange(
                level: CommonConstants.LevelOne.ToString(),
                netReportedTonnage: 100m,
                previousInvoicedTonnage: 0m);

            Assert.AreEqual(0m, result);
        }

        [TestMethod]
        public void ComputePerMaterialChange_validInputs_returnsDifference()
        {
            var result = TonnageChangeUtil.ComputePerMaterialChange(
                level: CommonConstants.LevelOne.ToString(),
                netReportedTonnage: 105.5m,
                previousInvoicedTonnage: 100.25m);

            Assert.AreEqual(5.25m, result);
        }

        // ---------- ComputeCountAndAdvice ----------

        [TestMethod]
        public void ComputeCountAndAdvice_levelNot1_returnsNulls()
        {
            var byMaterial = new Dictionary<string, DisposalFee>
            {
                ["PAPER"] = new() { HhTonnage = RamTonnage.Empty, PbTonnage = RamTonnage.Empty, HdcTonnage = RamTonnage.Empty, TotalTonnage = RamTonnage.Empty, TonnageChange = 5m, FeeWithBadDebtByCountry = ByCountryCost.Empty, ActionedSmcwTonnage = RamTonnageGroup.Empty, NetTonnage = RamTonnageGroup.Empty, PricePerTonne = RamTonnageGroup.Empty, Fee = RamTonnageGroup.Empty }
            };

            var (count, advice) = TonnageChangeUtil.ComputeCountAndAdvice("2", byMaterial);

            Assert.IsNull(count);
            Assert.IsNull(advice);
        }

        [TestMethod]
        public void ComputeCountAndAdvice_changesPresent_returnsCountAndCHANGE()
        {
            var byMaterial = new Dictionary<string, DisposalFee>
            {
                ["PAPER"] = new() { HhTonnage = RamTonnage.Empty, PbTonnage = RamTonnage.Empty, HdcTonnage = RamTonnage.Empty, TotalTonnage = RamTonnage.Empty, TonnageChange = 0m, FeeWithBadDebtByCountry = ByCountryCost.Empty, ActionedSmcwTonnage = RamTonnageGroup.Empty, NetTonnage = RamTonnageGroup.Empty, PricePerTonne = RamTonnageGroup.Empty, Fee = RamTonnageGroup.Empty},  // ignored
                ["GLASS"] = new() { HhTonnage = RamTonnage.Empty, PbTonnage = RamTonnage.Empty, HdcTonnage = RamTonnage.Empty, TotalTonnage = RamTonnage.Empty, TonnageChange = null, FeeWithBadDebtByCountry = ByCountryCost.Empty, ActionedSmcwTonnage = RamTonnageGroup.Empty, NetTonnage = RamTonnageGroup.Empty, PricePerTonne = RamTonnageGroup.Empty, Fee = RamTonnageGroup.Empty },  // ignored
                ["METAL"] = new() { HhTonnage = RamTonnage.Empty, PbTonnage = RamTonnage.Empty, HdcTonnage = RamTonnage.Empty, TotalTonnage = RamTonnage.Empty, TonnageChange = 3m, FeeWithBadDebtByCountry = ByCountryCost.Empty, ActionedSmcwTonnage = RamTonnageGroup.Empty, NetTonnage = RamTonnageGroup.Empty, PricePerTonne = RamTonnageGroup.Empty, Fee = RamTonnageGroup.Empty },  // counted
                ["PLASTIC"] = new() { HhTonnage = RamTonnage.Empty, PbTonnage = RamTonnage.Empty, HdcTonnage = RamTonnage.Empty, TotalTonnage = RamTonnage.Empty, TonnageChange = -1m, FeeWithBadDebtByCountry = ByCountryCost.Empty, ActionedSmcwTonnage = RamTonnageGroup.Empty, NetTonnage = RamTonnageGroup.Empty, PricePerTonne = RamTonnageGroup.Empty, Fee = RamTonnageGroup.Empty }   // counted
            };

            var (count, advice) = TonnageChangeUtil.ComputeCountAndAdvice(
                CommonConstants.LevelOne.ToString(), byMaterial);

            Assert.AreEqual("2", count);
            Assert.AreEqual("CHANGE", advice);
        }

        [TestMethod]
        public void ComputeCountAndAdvice_noChanges_returnsZeroAndEmptyAdvice()
        {
            var byMaterial = new Dictionary<string, DisposalFee>
            {
                ["PAPER"] = new() { HhTonnage = RamTonnage.Empty, PbTonnage = RamTonnage.Empty, HdcTonnage = RamTonnage.Empty, TotalTonnage = RamTonnage.Empty, TonnageChange = 0m, FeeWithBadDebtByCountry = ByCountryCost.Empty, ActionedSmcwTonnage = RamTonnageGroup.Empty, NetTonnage = RamTonnageGroup.Empty, PricePerTonne = RamTonnageGroup.Empty, Fee = RamTonnageGroup.Empty },
                ["GLASS"] = new() { HhTonnage = RamTonnage.Empty, PbTonnage = RamTonnage.Empty, HdcTonnage = RamTonnage.Empty, TotalTonnage = RamTonnage.Empty, TonnageChange = null, FeeWithBadDebtByCountry = ByCountryCost.Empty, ActionedSmcwTonnage = RamTonnageGroup.Empty, NetTonnage = RamTonnageGroup.Empty, PricePerTonne = RamTonnageGroup.Empty, Fee = RamTonnageGroup.Empty }
            };

            var (count, advice) = TonnageChangeUtil.ComputeCountAndAdvice(
                CommonConstants.LevelOne.ToString(), byMaterial);

            Assert.AreEqual("0", count);
            Assert.AreEqual(string.Empty, advice);
        }
    }
}
