using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper : ICalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper
    {
        public CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts Map(CalcResultSummaryProducerDisposalFees fees)
        {
            var costs = fees.BillingInstructionSection;
            return new CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts
            {
                CurrentYearInvoicedTotalToDate = CurrencyConverter.FormattedCurrencyValue(costs!.CurrentYearInvoiceTotalToDate!),
                TonnageChangeSinceLastInvoice = costs.TonnageChangeSinceLastInvoice ?? CommonConstants.Hyphen,
                LiabilityDifferenceCalcVsPrev = costs.LiabilityDifference,
                MaterialThresholdBreached = costs.MaterialThresholdBreached ?? string.Empty,
                TonnageThresholdBreached = costs.TonnageThresholdBreached ?? string.Empty,
                PercentageLiabilityDifferenceCalcVsPrev = GetPercentageLiabilityDifference(costs.PercentageLiabilityDifference)!,
                MaterialPercentageThresholdBreached = costs.MaterialPercentageThresholdBreached ?? string.Empty,
                TonnagePercentageThresholdBreached = costs.TonnagePercentageThresholdBreached ?? string.Empty,
                SuggestedBillingInstruction = costs.SuggestedBillingInstruction ?? string.Empty,
                SuggestedInvoiceAmount = GetFormattedCurrencyValue(costs.SuggestedInvoiceAmount!)
            };
        }

        private string? GetPercentageLiabilityDifference(decimal? percentageLiabilityDifference)
        {
            if (percentageLiabilityDifference == null)
                return CommonConstants.Hyphen;

            return $"{Math.Round((decimal)percentageLiabilityDifference, (int)DecimalPlaces.Two).ToString()}%";
        }

        private string GetFormattedCurrencyValue(string value)
        {
            if (value == null)
                return string.Empty;

            if (value == CommonConstants.Hyphen)
                return CommonConstants.Hyphen;

            return CurrencyConverter.ConvertToCurrency(value);
        }
    }
}
