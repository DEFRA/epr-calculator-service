namespace EPR.Calculator.Service.Function.Builder.Summary.OneAndTwoA
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Models;

    public static class CalcResultOneAndTwoAUtil
    {
        public static decimal GetTotalDisposalCostswoBadDebtprovision1(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
        {
            return GetTotalFee(producerDisposalFees, fee => fee.TotalProducerDisposalFee);
        }

        public static decimal GetTotalBadDebtprovision1(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
        {
            return GetTotalFee(producerDisposalFees, fee => fee.LocalAuthorityDisposalCostsSectionOne?.BadDebtProvision ?? 0);
        }

        public static decimal GetTotalDisposalCostswithBadDebtprovision1(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
        {
            return GetTotalFee(producerDisposalFees, fee => fee.TotalProducerDisposalFeeWithBadDebtProvision);
        }

        public static decimal GetTotalCommsCostswoBadDebtprovision2A(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
        {
            return GetTotalFee(producerDisposalFees, fee => fee.TotalProducerCommsFee);
        }

        public static decimal GetTotalBadDebtprovision2A(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
        {
            return GetTotalFee(producerDisposalFees, fee => fee.CommunicationCostsSectionTwoA?.BadDebtProvision ?? 0);
        }

        public static decimal GetTotalCommsCostswithBadDebtprovision2A(IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
        {
            return GetTotalFee(producerDisposalFees, fee => fee.TotalProducerCommsFeeWithBadDebtProvision);
        }

        public static decimal GetTotalFee(IEnumerable<CalcResultSummaryProducerDisposalFees>? producerDisposalFees, Func<CalcResultSummaryProducerDisposalFees, decimal> selector)
        {
            if (producerDisposalFees == null)
            {
                return 0m;
            }

            var totalFee = producerDisposalFees
                .FirstOrDefault(t => t.LeaverDate == CommonConstants.Totals);

            if (totalFee is null)
            {
                return 0m;
            }

            return selector(totalFee);
        }
    }
}