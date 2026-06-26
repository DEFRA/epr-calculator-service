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
            var byMaterial = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>
            {
                ["PAPER"] = new() { TonnageChange = 5m, ProducerDisposalFeeWithBadDebtProvision = ByCountryCost.Empty, ActionedSelfManagedConsumerWasteTonnage = RAMTonnageGroup.Empty, NetReportedTonnage = RAMTonnageGroup.Empty, PricePerTonne = RAMTonnageGroup.Empty, ProducerDisposalFee = RAMTonnageGroup.Empty }
            };

            var (count, advice) = TonnageChangeUtil.ComputeCountAndAdvice("2", byMaterial);

            Assert.IsNull(count);
            Assert.IsNull(advice);
        }

        [TestMethod]
        public void ComputeCountAndAdvice_changesPresent_returnsCountAndCHANGE()
        {
            var byMaterial = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>
            {
                ["PAPER"] = new() { TonnageChange = 0m, ProducerDisposalFeeWithBadDebtProvision = ByCountryCost.Empty, ActionedSelfManagedConsumerWasteTonnage = RAMTonnageGroup.Empty, NetReportedTonnage = RAMTonnageGroup.Empty, PricePerTonne = RAMTonnageGroup.Empty, ProducerDisposalFee = RAMTonnageGroup.Empty},  // ignored
                ["GLASS"] = new() { TonnageChange = null, ProducerDisposalFeeWithBadDebtProvision = ByCountryCost.Empty, ActionedSelfManagedConsumerWasteTonnage = RAMTonnageGroup.Empty, NetReportedTonnage = RAMTonnageGroup.Empty, PricePerTonne = RAMTonnageGroup.Empty, ProducerDisposalFee = RAMTonnageGroup.Empty },  // ignored
                ["METAL"] = new() { TonnageChange = 3m, ProducerDisposalFeeWithBadDebtProvision = ByCountryCost.Empty, ActionedSelfManagedConsumerWasteTonnage = RAMTonnageGroup.Empty, NetReportedTonnage = RAMTonnageGroup.Empty, PricePerTonne = RAMTonnageGroup.Empty, ProducerDisposalFee = RAMTonnageGroup.Empty },  // counted
                ["PLASTIC"] = new() { TonnageChange = -1m, ProducerDisposalFeeWithBadDebtProvision = ByCountryCost.Empty, ActionedSelfManagedConsumerWasteTonnage = RAMTonnageGroup.Empty, NetReportedTonnage = RAMTonnageGroup.Empty, PricePerTonne = RAMTonnageGroup.Empty, ProducerDisposalFee = RAMTonnageGroup.Empty }   // counted
            };

            var (count, advice) = TonnageChangeUtil.ComputeCountAndAdvice(
                CommonConstants.LevelOne.ToString(), byMaterial);

            Assert.AreEqual("2", count);
            Assert.AreEqual("CHANGE", advice);
        }

        [TestMethod]
        public void ComputeCountAndAdvice_noChanges_returnsZeroAndEmptyAdvice()
        {
            var byMaterial = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>
            {
                ["PAPER"] = new() { TonnageChange = 0m, ProducerDisposalFeeWithBadDebtProvision = ByCountryCost.Empty, ActionedSelfManagedConsumerWasteTonnage = RAMTonnageGroup.Empty, NetReportedTonnage = RAMTonnageGroup.Empty, PricePerTonne = RAMTonnageGroup.Empty, ProducerDisposalFee = RAMTonnageGroup.Empty },
                ["GLASS"] = new() { TonnageChange = null, ProducerDisposalFeeWithBadDebtProvision = ByCountryCost.Empty, ActionedSelfManagedConsumerWasteTonnage = RAMTonnageGroup.Empty, NetReportedTonnage = RAMTonnageGroup.Empty, PricePerTonne = RAMTonnageGroup.Empty, ProducerDisposalFee = RAMTonnageGroup.Empty }
            };

            var (count, advice) = TonnageChangeUtil.ComputeCountAndAdvice(
                CommonConstants.LevelOne.ToString(), byMaterial);

            Assert.AreEqual("0", count);
            Assert.AreEqual(string.Empty, advice);
        }
    }
}
