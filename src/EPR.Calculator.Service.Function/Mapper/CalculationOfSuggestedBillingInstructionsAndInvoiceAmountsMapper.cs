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
                CurrentYearInvoicedTotalToDate = GetFormattedCurrencyValue(costs!.CurrentYearInvoiceTotalToDate),
                TonnageChangeSinceLastInvoice = costs.TonnageChangeSinceLastInvoice ?? CommonConstants.Hyphen,
                LiabilityDifferenceCalcVsPrev = GetFormattedCurrencyValue(costs.LiabilityDifference),
                MaterialThresholdBreached = costs.MaterialThresholdBreached ?? CommonConstants.Hyphen,
                TonnageThresholdBreached = costs.TonnageThresholdBreached ?? CommonConstants.Hyphen,
                PercentageLiabilityDifferenceCalcVsPrev = GetPercentageLiabilityDifference(costs.PercentageLiabilityDifference)!,
                MaterialPercentageThresholdBreached = costs.MaterialPercentageThresholdBreached ?? CommonConstants.Hyphen,
                TonnagePercentageThresholdBreached = costs.TonnagePercentageThresholdBreached ?? CommonConstants.Hyphen,
                SuggestedBillingInstruction = costs.SuggestedBillingInstruction ?? CommonConstants.Hyphen,
                SuggestedInvoiceAmount = GetFormattedCurrencyValue(costs.SuggestedInvoiceAmount!)
            };
        }

        private string? GetPercentageLiabilityDifference(decimal? percentageLiabilityDifference)
        {
            if (percentageLiabilityDifference == null)
                return CommonConstants.Hyphen;

            return $"{Math.Round((decimal)percentageLiabilityDifference, (int)DecimalPlaces.Two).ToString()}%";
        }

        private string GetFormattedCurrencyValue(decimal? value)
        {
            if (value == null)
                return CommonConstants.Hyphen;

            return CurrencyConverter.ConvertToCurrency(value.ToString()!);
        }
    }
}
