using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Builder.Summary.Common
{
    public static class MaterialCostsUtil
    {
        public static SelfManagedConsumerWasteData SumSelfManagedConsumerWasteData(
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            MaterialDetail material,
            bool isOverAllTotalRow,
            SelfManagedConsumerWaste smcw)
        {
            return isOverAllTotalRow
                ? smcw.OverallTotalPerMaterials.GetValueOrDefault(material.Code) ?? SelfManagedConsumerWasteData.Zero
                : smcw.ProducerTotals
                    .Where(x => x.Level == 1 && producersAndSubsidiaries.Any(y => x.ProducerId == y.ProducerId))
                    .Select(x => x.SelfManagedConsumerWasteDataPerMaterials[material.Code])
                    .Single();
        }

        public static decimal? GetPreviousInvoicedTonnage(
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            MaterialDetail material,
            bool isOverAllTotalRow,
            decimal? previousInvoicedNetTonnage
        )
        {
            return isOverAllTotalRow
                ? CalcResultSummaryUtil.GetPreviousInvoicedTonnageOverallTotal(producerDisposalFees, material)
                : previousInvoicedNetTonnage;
        }

        public static decimal GetProducerDisposalFee(
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            MaterialDetail material,
            CalcResult calcResult,
            bool isOverAllTotalRow
        )
        {
            return isOverAllTotalRow
                ? CalcResultSummaryUtil.GetProducerDisposalFeeOverallTotal(producerDisposalFees, material)
                : CalcResultSummaryUtil.GetProducerDisposalFeeProducerTotal(producersAndSubsidiaries, material, calcResult);
        }

        public static decimal GetBadDebtProvision(
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            MaterialDetail material,
            CalcResult calcResult,
            bool isOverAllTotalRow
        )
        {
            return isOverAllTotalRow
                ? CalcResultSummaryUtil.GetBadDebtProvisionOverallTotal(producerDisposalFees, material)
                : CalcResultSummaryUtil.GetBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult);
        }

        public static decimal GetProducerDisposalFeeWithBadDebtProvision(
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            MaterialDetail material,
            CalcResult calcResult,
            bool isOverAllTotalRow
        )
        {
            return isOverAllTotalRow
                ? CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvisionOverallTotal(producerDisposalFees, material)
                : CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult);
        }

        [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
        public static decimal GetCountryDisposalFeeWithBadDebtProvision(
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            MaterialDetail material,
            CalcResult calcResult,
            Countries country,
            bool isOverAllTotalRow
        )
        {
            return isOverAllTotalRow
                ? CalcResultSummaryUtil.GetCountryBadDebtProvisionOverallTotal(producerDisposalFees, material, country)
                : CalcResultSummaryUtil.GetCountryBadDebtProvisionTotal(producersAndSubsidiaries, material, calcResult, country);
        }
    }
}
