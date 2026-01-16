using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Builder.Summary.BillingInstructions
{
    public static class BillingInstructionsProducer
    {
        public static readonly int ColumnIndex = 296;

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

        public static void SetValues(CalcResultSummary result, IEnumerable<ProducerInvoicedDto> ProducerInvoicedMaterialNetTonnage, IEnumerable<DefaultParamResultsClass> defaultParams)
        {
            decimal totalTonnage = 0;
            decimal liabilityDifferenceRunningTotal = 0m;
            decimal SuggestedInvoiceAmountTotal = 0m;

            var dpList = defaultParams as IList<DefaultParamResultsClass> ?? defaultParams.ToList();

            decimal? param_MATT_AI = dpList.FirstOrDefault(p => p.ParameterUniqueReference == CommonConstants.MaterialAmountIncrease)?.ParameterValue;
            decimal? param_MATT_AD = dpList.FirstOrDefault(p => p.ParameterUniqueReference == CommonConstants.MaterialAmountDecrease)?.ParameterValue;
            decimal? param_TONT_AI = dpList.FirstOrDefault(p => p.ParameterUniqueReference == CommonConstants.TonnageAmountIncrease)?.ParameterValue;
            decimal? param_TONT_AD = dpList.FirstOrDefault(p => p.ParameterUniqueReference == CommonConstants.TonnageAmountDecrease)?.ParameterValue;
            decimal? param_MATT_PI = dpList.FirstOrDefault(p => p.ParameterUniqueReference == CommonConstants.MaterialPercentageIncrease)?.ParameterValue;
            decimal? param_MATT_PD = dpList.FirstOrDefault(p => p.ParameterUniqueReference == CommonConstants.MaterialPercentageDecrease)?.ParameterValue;
            decimal? param_TONT_PI = dpList.FirstOrDefault(p => p.ParameterUniqueReference == CommonConstants.TonnagePercentageIncrease)?.ParameterValue;
            decimal? param_TONT_PD = dpList.FirstOrDefault(p => p.ParameterUniqueReference == CommonConstants.TonnagePercentageDecrease)?.ParameterValue;

            foreach (var fee in result.ProducerDisposalFees)
            {
                var currentYearInvoicedTotalTonnage = ProducerInvoicedMaterialNetTonnage
                                                    .Where(x => x.InvoicedTonnage!.ProducerId.ToString() == fee.ProducerId)
                                                    .Select(y => y.InvoiceInstruction!.CurrentYearInvoicedTotalAfterThisRun)
                                                    .FirstOrDefault();

                totalTonnage += currentYearInvoicedTotalTonnage.GetValueOrDefault();

                var liabilityDifferenceCalculated = CalculateLiabilityDifference(fee, currentYearInvoicedTotalTonnage);
                if (liabilityDifferenceCalculated.HasValue) liabilityDifferenceRunningTotal += liabilityDifferenceCalculated.Value;
                var currentYearInvoiceTotalToDate = GetCurrentYearInvoicedTotalToDate(fee, currentYearInvoicedTotalTonnage, totalTonnage);
                var tonnageChangeSinceLastInvoice = GetTonnageChangeSinceLastInvoice(fee);
                var liabilityDifference = GetLiabilityDifference(fee, liabilityDifferenceCalculated, liabilityDifferenceRunningTotal);
                var percentageLiabilityDifference = GetPercentageLiabilityDifference(fee, currentYearInvoiceTotalToDate, liabilityDifference);
                var materialThresholdBreached = GetMaterialThresholdBreached(fee, currentYearInvoicedTotalTonnage, liabilityDifferenceCalculated, param_MATT_AI, param_MATT_AD);
                var tonnageThresholdBreached = GetTonnageThresholdBreached(fee, currentYearInvoicedTotalTonnage, liabilityDifferenceCalculated, param_TONT_AI, param_TONT_AD);
                var materialPercentageThresholdBreached = GetMaterialPercentageThresholdBreached(fee, currentYearInvoiceTotalToDate, percentageLiabilityDifference, param_MATT_PI, param_MATT_PD);
                var tonnagePercentageThresholdBreached = GetTonnagePercentageThresholdBreached(fee, currentYearInvoiceTotalToDate, tonnageChangeSinceLastInvoice, percentageLiabilityDifference, param_TONT_PI, param_TONT_PD);
                var suggestedBillingInstruction = GetSuggestedBillingInstruction(fee, currentYearInvoiceTotalToDate, liabilityDifference, materialThresholdBreached, tonnageThresholdBreached, materialPercentageThresholdBreached, tonnagePercentageThresholdBreached);
                var suggestedInvoiceAmount = GetSuggestedInvoiceAmount(fee, suggestedBillingInstruction, liabilityDifference, SuggestedInvoiceAmountTotal);
                if (suggestedInvoiceAmount.HasValue) SuggestedInvoiceAmountTotal += suggestedInvoiceAmount.Value;

                fee.BillingInstructionSection = new CalcResultSummaryBillingInstruction
                {
                    CurrentYearInvoiceTotalToDate = currentYearInvoiceTotalToDate,
                    TonnageChangeSinceLastInvoice = tonnageChangeSinceLastInvoice,
                    LiabilityDifference = liabilityDifference,
                    MaterialThresholdBreached = materialThresholdBreached,
                    TonnageThresholdBreached = tonnageThresholdBreached,
                    PercentageLiabilityDifference = percentageLiabilityDifference,
                    MaterialPercentageThresholdBreached = materialPercentageThresholdBreached,
                    TonnagePercentageThresholdBreached = tonnagePercentageThresholdBreached,
                    SuggestedBillingInstruction = suggestedBillingInstruction,
                    SuggestedInvoiceAmount = suggestedInvoiceAmount
                };
            }
        }

        private static decimal? GetCurrentYearInvoicedTotalToDate(CalcResultSummaryProducerDisposalFees fee, decimal? currentYearInvoicedTotalTonnage, decimal totalTonnage)
        {
            decimal? result;

            if (fee.LeaverDate == CommonConstants.Totals)
            {
                result = totalTonnage;
            }
            else if (fee.Level == "1")
            {
                result = currentYearInvoicedTotalTonnage;
            }
            else
            {
                result = null;
            }

            return result;
        }

        private static string? GetTonnageChangeSinceLastInvoice(CalcResultSummaryProducerDisposalFees fee)
        {
            if (fee.LeaverDate == CommonConstants.Totals)
                return string.Empty;
            else if (fee.TonnageChangeAdvice == "CHANGE")
                return "Tonnage Changed";
            else
                return null;
        }

        private static decimal? CalculateLiabilityDifference(CalcResultSummaryProducerDisposalFees fee, decimal? currentInvoicedTotalToDate)
        {
            if (fee.LeaverDate == CommonConstants.Totals) return null;
            if (fee.Level != CommonConstants.LevelOne.ToString()) return null;
            if (!currentInvoicedTotalToDate.HasValue) return null;

            var calc = fee.TotalProducerBillBreakdownCosts;
            if (calc is null) return null;

            var diff = Math.Round(calc.TotalProducerFeeWithBadDebtProvision, 2) - Math.Round(currentInvoicedTotalToDate.Value, 2);

            return diff;
        }

        private static decimal? GetLiabilityDifference(CalcResultSummaryProducerDisposalFees fee, decimal? liabilityDifferenceCalculated, decimal liabilityDifferenceRunningTotal)
        {
            if (fee.LeaverDate == CommonConstants.Totals)
            {
                if (liabilityDifferenceRunningTotal == 0m) return null;
                return liabilityDifferenceRunningTotal;
            }

            return liabilityDifferenceCalculated;
        }

        private static string GetMaterialThresholdBreached(CalcResultSummaryProducerDisposalFees fee, decimal? currentInvoicedTotalToDate, decimal? liabilityDifferenceCalculated, decimal? param_MATT_AI, decimal? param_MATT_AD)
        {
            if (fee.LeaverDate == CommonConstants.Totals) return String.Empty;
            if (fee.Level != CommonConstants.LevelOne.ToString()) return CommonConstants.Hyphen;
            if (!currentInvoicedTotalToDate.HasValue) return CommonConstants.Hyphen;
            if (!liabilityDifferenceCalculated.HasValue) return CommonConstants.Hyphen;

            if (!param_MATT_AI.HasValue || !param_MATT_AD.HasValue) return CommonConstants.Hyphen;

            if (liabilityDifferenceCalculated >= param_MATT_AI.Value) return "+ve";
            if (liabilityDifferenceCalculated <= param_MATT_AD.Value) return "-ve";

            return CommonConstants.Hyphen;
        }

        private static string GetTonnageThresholdBreached(CalcResultSummaryProducerDisposalFees fee, decimal? currentInvoicedTotalToDate, decimal? liabilityDifferenceCalculated, decimal? param_TONT_AI, decimal? param_TONT_AD)
        {
            if (fee.LeaverDate == CommonConstants.Totals) return String.Empty;
            if (fee.Level != CommonConstants.LevelOne.ToString()) return CommonConstants.Hyphen;
            if (!currentInvoicedTotalToDate.HasValue) return CommonConstants.Hyphen;
            if (fee.TonnageChangeAdvice != "CHANGE") return CommonConstants.Hyphen;
            if (!liabilityDifferenceCalculated.HasValue) return CommonConstants.Hyphen;

            if (!param_TONT_AI.HasValue || !param_TONT_AD.HasValue) return CommonConstants.Hyphen;

            if (liabilityDifferenceCalculated >= param_TONT_AI) return "+ve";
            if (liabilityDifferenceCalculated <= param_TONT_AD) return "-ve";

            return CommonConstants.Hyphen;
        }

        private static decimal? GetPercentageLiabilityDifference(CalcResultSummaryProducerDisposalFees fee, decimal? currentYearInvoiceTotalToDate, decimal? liabilityDifference)
        {
            if (fee.Level != CommonConstants.LevelOne.ToString() ||
                fee.LeaverDate == CommonConstants.Totals ||
                !currentYearInvoiceTotalToDate.HasValue ||
                !liabilityDifference.HasValue ||
                currentYearInvoiceTotalToDate == 0m)
            {
                return null;
            }            
            return Math.Round(liabilityDifference.Value / currentYearInvoiceTotalToDate.Value * 100, 2);
        }

        private static string GetMaterialPercentageThresholdBreached(CalcResultSummaryProducerDisposalFees fee, decimal? currentYearInvoiceTotalToDate, decimal? percentageLiabilityDifference, decimal? param_MATT_PI, decimal? param_MATT_PD)
        {
            if (fee.LeaverDate == CommonConstants.Totals) return String.Empty;
            if (fee.Level != CommonConstants.LevelOne.ToString()) return CommonConstants.Hyphen;
            if (!currentYearInvoiceTotalToDate.HasValue) return CommonConstants.Hyphen;

            if (percentageLiabilityDifference >= param_MATT_PI) return CommonConstants.Positive;
            if (percentageLiabilityDifference <= param_MATT_PD) return CommonConstants.Negative;

            return CommonConstants.Hyphen;

        }

        private static string GetTonnagePercentageThresholdBreached(CalcResultSummaryProducerDisposalFees fee, decimal? currentYearInvoiceTotalToDate, string? tonnageChangeSinceLastInvoice, decimal? percentageLiabilityDifference, decimal? param_TONT_PI, decimal? param_TONT_PD)
        {
            if (fee.LeaverDate == CommonConstants.Totals) return String.Empty;
            if (fee.Level != CommonConstants.LevelOne.ToString()) return CommonConstants.Hyphen;

            if (!currentYearInvoiceTotalToDate.HasValue) return CommonConstants.Hyphen;
            if (tonnageChangeSinceLastInvoice != CommonConstants.TonnageChanged) return CommonConstants.Hyphen;

            if (percentageLiabilityDifference >= param_TONT_PI) return CommonConstants.Positive;
            if (percentageLiabilityDifference <= param_TONT_PD) return CommonConstants.Negative;

            return CommonConstants.Hyphen;
        }

        private static string GetSuggestedBillingInstruction(CalcResultSummaryProducerDisposalFees fee, decimal? currentYearInvoiceTotalToDate, decimal? liabilityDifference, string materialThresholdBreached, string tonnageThresholdBreached, string materialPercentageThresholdBreached, string tonnagePercentageThresholdBreached)
        {
            if (fee.LeaverDate == CommonConstants.Totals) return String.Empty;
            if (fee.Level != CommonConstants.LevelOne.ToString()) return CommonConstants.Hyphen;

            if (!currentYearInvoiceTotalToDate.HasValue) return CommonConstants.Initial;

            if (liabilityDifference > 0 &&
                (materialThresholdBreached != CommonConstants.Hyphen || tonnageThresholdBreached != CommonConstants.Hyphen || materialPercentageThresholdBreached != CommonConstants.Hyphen || tonnagePercentageThresholdBreached != CommonConstants.Hyphen))
                return CommonConstants.Delta;

            if (liabilityDifference < 0 &&
                (materialThresholdBreached != CommonConstants.Hyphen || tonnageThresholdBreached != CommonConstants.Hyphen || materialPercentageThresholdBreached != CommonConstants.Hyphen || tonnagePercentageThresholdBreached != CommonConstants.Hyphen))
                return CommonConstants.Rebill;

            if (liabilityDifference == 0) return CommonConstants.Hyphen;

            return CommonConstants.Hyphen;
        }

        private static decimal? GetSuggestedInvoiceAmount(CalcResultSummaryProducerDisposalFees fee, string suggestedBillingInstruction, decimal? liabilityDifference, decimal? suggestedInvoiceAmountTotal)
        {
            if (fee.LeaverDate == CommonConstants.Totals) return suggestedInvoiceAmountTotal;
            if (fee.Level != CommonConstants.LevelOne.ToString()) return null;

            if (suggestedBillingInstruction == CommonConstants.Initial || suggestedBillingInstruction == CommonConstants.Rebill)
                return fee.TotalProducerBillBreakdownCosts?.TotalProducerFeeWithBadDebtProvision;

            if (suggestedBillingInstruction == CommonConstants.Delta) return liabilityDifference;

            return null;
        }
    }
}
