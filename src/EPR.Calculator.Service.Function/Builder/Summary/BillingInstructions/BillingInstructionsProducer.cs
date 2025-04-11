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
        public static readonly int ColumnIndex = 273;

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
                fee.CurrentYearInvoicedTotalToDate = GetCurrentYearInvoicedTotalToDate(fee);
                fee.TonnageChangeSinceLastInvoice = GetTonnageChangeSinceLastInvoice(fee);
                fee.LiabilityDifference = GetLiabilityDifference(fee);
                fee.MaterialThresholdBreached = GetMaterialThresholdBreached(fee);
                fee.TonnageThresholdBreached = GetTonnageThresholdBreached(fee);
                fee.PercentageLiabilityDifference = GetPercentageLiabilityDifference(fee);
                fee.MaterialPercentageThresholdBreached = GetMaterialPercentageThresholdBreached(fee);
                fee.TonnagePercentageThresholdBreached = GetTonnagePercentagThresholdBreached(fee);
                fee.SuggestedBillingInstruction = GetSuggestedBillingInstruction(fee);
                fee.SuggestedInvoiceAmount = GetSuggestedInvoiceAmount(fee);
            }
        }

        private static decimal GetCurrentYearInvoicedTotalToDate(CalcResultSummaryProducerDisposalFees fee)
        {
            return 0;
        }

        private static string GetTonnageChangeSinceLastInvoice(CalcResultSummaryProducerDisposalFees fee)
        {
            return "-";
        }

        private static decimal GetLiabilityDifference(CalcResultSummaryProducerDisposalFees fee)
        {
            return 0;
        }

        private static string GetMaterialThresholdBreached(CalcResultSummaryProducerDisposalFees fee)
        {
            return "-";
        }

        private static string GetTonnageThresholdBreached(CalcResultSummaryProducerDisposalFees fee)
        {
            return "-";
        }

        private static string GetPercentageLiabilityDifference(CalcResultSummaryProducerDisposalFees fee)
        {
            return "-";
        }

        private static string GetMaterialPercentageThresholdBreached(CalcResultSummaryProducerDisposalFees fee)
        {
            return "-";
        }

        private static string GetTonnagePercentagThresholdBreached(CalcResultSummaryProducerDisposalFees fee)
        {
            return "-";
        }

        private static string GetSuggestedBillingInstruction(CalcResultSummaryProducerDisposalFees fee)
        {
            return "-";
        }

        private static decimal GetSuggestedInvoiceAmount(CalcResultSummaryProducerDisposalFees fee)
        {
            return 0;
        }
    }
}
