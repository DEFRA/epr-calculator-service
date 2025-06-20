using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CalcResultBillingInstructionsJsonMapper : IBillingInstructionsJsonMapper
    {
        public CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts Map(CalcResultSummaryProducerDisposalFees fees)
        {
            return new CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts
            {
                CurrentYearInvoicedTotalToDate = fees.CurrentYearInvoiceTotalToDate ?? string.Empty,
                TonnageChangeSinceLastInvoice = fees.TonnageChangeSinceLastInvoice ?? string.Empty,
                LiabilityDifferenceCalcVsPrev = fees.LiabilityDifference ?? string.Empty,
                MaterialThresholdBreached = fees.MaterialThresholdBreached ?? string.Empty,
                TonnageThresholdBreached = fees.TonnageThresholdBreached ?? string.Empty,
                PercentageLiabilityDifferenceCalcVsPrev = fees.PercentageLiabilityDifference ?? string.Empty,
                MaterialPercentageThresholdBreached = fees.MaterialPercentageThresholdBreached ?? string.Empty,
                TonnagePercentageThresholdBreached = fees.TonnagePercentageThresholdBreached ?? string.Empty,
                SuggestedBillingInstruction = fees.SuggestedBillingInstruction ?? string.Empty,
                SuggestedInvoiceAmount = fees.SuggestedInvoiceAmount ?? string.Empty
            };
        }
    }
}
