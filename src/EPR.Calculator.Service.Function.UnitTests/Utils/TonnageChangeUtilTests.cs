using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;

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
                ["PAPER"] = new() { TonnageChange = 5m, ProducerDisposalFeeWithBadDebtProvision = ByCountryCost.Empty }
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
                ["PAPER"] = new() { TonnageChange = 0m, ProducerDisposalFeeWithBadDebtProvision = ByCountryCost.Empty },  // ignored
                ["GLASS"] = new() { TonnageChange = null, ProducerDisposalFeeWithBadDebtProvision = ByCountryCost.Empty },  // ignored
                ["METAL"] = new() { TonnageChange = 3m, ProducerDisposalFeeWithBadDebtProvision = ByCountryCost.Empty },  // counted
                ["PLASTIC"] = new() { TonnageChange = -1m, ProducerDisposalFeeWithBadDebtProvision = ByCountryCost.Empty }   // counted
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
                ["PAPER"] = new() { TonnageChange = 0m, ProducerDisposalFeeWithBadDebtProvision = ByCountryCost.Empty },
                ["GLASS"] = new() { TonnageChange = null, ProducerDisposalFeeWithBadDebtProvision = ByCountryCost.Empty }
            };

            var (count, advice) = TonnageChangeUtil.ComputeCountAndAdvice(
                CommonConstants.LevelOne.ToString(), byMaterial);

            Assert.AreEqual("0", count);
            Assert.AreEqual(string.Empty, advice);
        }

        // ---------- Inline helpers (no external classes) ----------

        private static CalcResultSummaryProducerDisposalFees MakeProducerRow(
            string level, string materialCode, decimal? tonnageChange)
        {
            return new CalcResultSummaryProducerDisposalFees
            {
                // minimal required fields (set to dummy values)
                ProducerIdInt = 1,
                ProducerId = "1",
                ProducerName = "Test Producer",
                SubsidiaryId = string.Empty,
                TradingName = string.Empty,
                Level = level,
                TwoCTotalProducerFeeForCommsCostsWithBadDebt = ByCountryCost.Empty,
                TotalProducerDisposalFeeWithBadDebtProvision = ByCountryCost.Empty,
                TotalProducerCommsFeeWithBadDebtProvision = ByCountryCost.Empty,
                ProducerDisposalFeesByMaterial =
                    new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>
                    {
                        [materialCode] = new CalcResultSummaryProducerDisposalFeesByMaterial
                        {
                            TonnageChange = tonnageChange,
                            ProducerDisposalFeeWithBadDebtProvision = ByCountryCost.Empty
                        }
                    }
            };
        }
    }
}
