using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Builder.Summary.BillingInstructions
{
    public static class BillingInstructionsProducer
    {
        public static readonly int ColumnIndex = 292;

        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.CurrentYearInvoicedTotalToDate, ColumnIndex = ColumnIndex },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.TonnageChangeSinceLastInvoice, ColumnIndex = ColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.LiabilityDifference, ColumnIndex = ColumnIndex + 2 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.MaterialThresholdBreached, ColumnIndex = ColumnIndex + 3 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.TonnageThresholdBreached, ColumnIndex = ColumnIndex + 4 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.PercentageLiabilityDifference, ColumnIndex = ColumnIndex + 5 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.MaterialPercentageThresholdBreached, ColumnIndex = ColumnIndex + 6 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.TonnagePercentagThresholdBreached, ColumnIndex = ColumnIndex + 6 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.SuggestedBillingInstruction, ColumnIndex = ColumnIndex + 6 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.SuggestedInvoiceAmount, ColumnIndex = ColumnIndex + 6 }
            ];
        }

        public static IEnumerable<CalcResultSummaryHeader> GetSummaryHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.Title, ColumnIndex = ColumnIndex }
            ];
        }

        public static void SetValues(CalcResultSummary result)
        {
            foreach (var fee in result.ProducerDisposalFees)
            {
                fee.BillingInstructionSection = new CalcResultSummaryBillingInstruction
                {
                    CurrentYearInvoiceTotalToDate = GetCurrentYearInvoicedTotalToDate(fee),
                    TonnageChangeSinceLastInvoice = GetTonnageChangeSinceLastInvoice(fee),
                    LiabilityDifference = GetLiabilityDifference(fee),
                    MaterialThresholdBreached = GetMaterialThresholdBreached(fee),
                    TonnageThresholdBreached = GetTonnageThresholdBreached(fee),
                    PercentageLiabilityDifference = GetPercentageLiabilityDifference(fee),
                    MaterialPercentageThresholdBreached = GetMaterialPercentageThresholdBreached(fee),
                    TonnagePercentageThresholdBreached = GetTonnagePercentagThresholdBreached(fee),
                    SuggestedBillingInstruction = GetSuggestedBillingInstruction(fee),
                    SuggestedInvoiceAmount = GetSuggestedInvoiceAmount(fee)
                };
            }
        }

        private static string GetCurrentYearInvoicedTotalToDate(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.IsProducerScaledup == CommonConstants.Totals
                ? CommonConstants.ZeroCurrency
                : CommonConstants.Hyphen;
        }

        private static string GetTonnageChangeSinceLastInvoice(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.IsProducerScaledup == CommonConstants.Totals
                ? string.Empty
                : CommonConstants.Hyphen;
        }

        private static string GetLiabilityDifference(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.IsProducerScaledup == CommonConstants.Totals
                ? CommonConstants.ZeroCurrency
                : CommonConstants.Hyphen;
        }

        private static string GetMaterialThresholdBreached(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.IsProducerScaledup == CommonConstants.Totals
                ? string.Empty
                : CommonConstants.Hyphen;
        }

        private static string GetTonnageThresholdBreached(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.IsProducerScaledup == CommonConstants.Totals
                ? string.Empty
                : CommonConstants.Hyphen;
        }

        private static string GetPercentageLiabilityDifference(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.IsProducerScaledup == CommonConstants.Totals
                ? string.Empty
                : CommonConstants.Hyphen;
        }

        private static string GetMaterialPercentageThresholdBreached(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.IsProducerScaledup == CommonConstants.Totals
                ? string.Empty
                : CommonConstants.Hyphen;
        }

        private static string GetTonnagePercentagThresholdBreached(CalcResultSummaryProducerDisposalFees fee)
        {
            return fee.IsProducerScaledup == CommonConstants.Totals
                ? string.Empty
                : CommonConstants.Hyphen;
        }

        private static string GetSuggestedBillingInstruction(CalcResultSummaryProducerDisposalFees fee)
        {
            if (fee.IsProducerScaledup == CommonConstants.Totals)
            {
                return string.Empty;
            }

            return fee.Level == CommonConstants.LevelOne.ToString()
                ? CommonConstants.Initial
                : CommonConstants.Hyphen;
        }

        private static string? GetSuggestedInvoiceAmount(CalcResultSummaryProducerDisposalFees fee)
        {
            if (fee.IsProducerScaledup == CommonConstants.Totals)
            {
                return fee.TotalProducerBillBreakdownCosts?.TotalProducerFeeWithBadDebtProvision.ToString();
            }

            return fee.Level == CommonConstants.LevelOne.ToString()
                ? fee.TotalProducerBillBreakdownCosts?.TotalProducerFeeWithBadDebtProvision.ToString()
                : CommonConstants.Hyphen;
        }
    }
}
