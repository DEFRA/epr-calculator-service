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
                CurrentYearInvoicedTotalToDate = GetFormattedCurrencyValue(fees.BillingInstructionSection.CurrentYearInvoiceTotalToDate!),
                TonnageChangeSinceLastInvoice = fees.BillingInstructionSection.TonnageChangeSinceLastInvoice ?? string.Empty,
                LiabilityDifferenceCalcVsPrev = GetFormattedCurrencyValue(fees.BillingInstructionSection.LiabilityDifference!),
                MaterialThresholdBreached = fees.BillingInstructionSection.MaterialThresholdBreached ?? string.Empty,
                TonnageThresholdBreached = fees.BillingInstructionSection.TonnageThresholdBreached ?? string.Empty,
                PercentageLiabilityDifferenceCalcVsPrev = GetPercentageLiabilityDifference(fees.BillingInstructionSection.PercentageLiabilityDifference!),
                MaterialPercentageThresholdBreached = fees.BillingInstructionSection.MaterialPercentageThresholdBreached ?? string.Empty,
                TonnagePercentageThresholdBreached = fees.BillingInstructionSection.TonnagePercentageThresholdBreached ?? string.Empty,
                SuggestedBillingInstruction = fees.BillingInstructionSection.SuggestedBillingInstruction ?? string.Empty,
                SuggestedInvoiceAmount = GetFormattedCurrencyValue(fees.BillingInstructionSection.SuggestedInvoiceAmount!)
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
