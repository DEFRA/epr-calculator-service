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
            return new CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts
            {
                CurrentYearInvoicedTotalToDate = GetFormattedCurrencyValue(fees.CurrentYearInvoiceTotalToDate!),
                TonnageChangeSinceLastInvoice = fees.TonnageChangeSinceLastInvoice ?? string.Empty,
                LiabilityDifferenceCalcVsPrev = GetFormattedCurrencyValue(fees.LiabilityDifference!),
                MaterialThresholdBreached = fees.MaterialThresholdBreached ?? string.Empty,
                TonnageThresholdBreached = fees.TonnageThresholdBreached ?? string.Empty,
                PercentageLiabilityDifferenceCalcVsPrev = GetPercentageLiabilityDifference(fees.PercentageLiabilityDifference!),
                MaterialPercentageThresholdBreached = fees.MaterialPercentageThresholdBreached ?? string.Empty,
                TonnagePercentageThresholdBreached = fees.TonnagePercentageThresholdBreached ?? string.Empty,
                SuggestedBillingInstruction = fees.SuggestedBillingInstruction ?? string.Empty,
                SuggestedInvoiceAmount = GetFormattedCurrencyValue(fees.SuggestedInvoiceAmount!)
            };
        }

        private string GetPercentageLiabilityDifference(string percentageLiabilityDifference)
        {
            if (percentageLiabilityDifference == null)
                return string.Empty;

            if (percentageLiabilityDifference == CommonConstants.Hyphen)
                return CommonConstants.Hyphen;

            var isConversionSuccessful = decimal.TryParse(percentageLiabilityDifference, out decimal value);

            if (!isConversionSuccessful)
                return string.Empty;

            return $"{Math.Round(value, (int)DecimalPlaces.Two).ToString()}%";
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
