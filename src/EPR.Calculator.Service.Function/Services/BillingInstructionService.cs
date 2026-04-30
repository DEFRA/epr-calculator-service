using System.Diagnostics;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.Billing.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IBillingInstructionService
    {
        public Task CreateBillingInstructions(RunContext runContext, CalcResult calcResult);
    }

    public class BillingInstructionService(
        ApplicationDBContext dbContext,
        IBulkOperations bulkOps,
        ILogger<BillingInstructionService> logger)
        : IBillingInstructionService
    {
        public async Task CreateBillingInstructions(RunContext runContext, CalcResult calcResult)
        {
            var stopwatch = Stopwatch.StartNew();
            var billingInstructions = new List<ProducerResultFileSuggestedBillingInstruction>();

            var producers = calcResult.CalcResultSummary.ProducerDisposalFees.Where(producer => producer.Level == CommonConstants.LevelOne.ToString());
            var cancelledProducers = calcResult.CalcResultCancelledProducers;

            foreach (var producer in producers)
            {
                var billingInstructionSection = producer.BillingInstructionSection;

                var isProducerIdParseSuccessful = int.TryParse(producer.ProducerId, out var producerId);

                if (isProducerIdParseSuccessful)
                {
                    var billingInstruction = new ProducerResultFileSuggestedBillingInstruction
                    {
                        CalculatorRunId = calcResult.CalcResultDetail.RunId,
                        ProducerId = producerId,
                        TotalProducerBillWithBadDebt = producer.TotalProducerBillBreakdownCosts!.TotalProducerFeeWithBadDebtProvision,
                        CurrentYearInvoiceTotalToDate = billingInstructionSection?.CurrentYearInvoiceTotalToDate,
                        TonnageChangeSinceLastInvoice = GetStringValue(billingInstructionSection?.TonnageChangeSinceLastInvoice!),
                        AmountLiabilityDifferenceCalcVsPrev = billingInstructionSection?.LiabilityDifference!,
                        MaterialPoundThresholdBreached = GetStringValue(billingInstructionSection?.MaterialThresholdBreached!),
                        TonnagePoundThresholdBreached = GetStringValue(billingInstructionSection?.TonnageThresholdBreached!),
                        PercentageLiabilityDifferenceCalcVsPrev = billingInstructionSection?.PercentageLiabilityDifference!,
                        MaterialPercentageThresholdBreached = GetStringValue(billingInstructionSection?.MaterialPercentageThresholdBreached!),
                        TonnagePercentageThresholdBreached = GetStringValue(billingInstructionSection?.TonnagePercentageThresholdBreached!),
                        SuggestedBillingInstruction = billingInstructionSection?.SuggestedBillingInstruction!,
                        SuggestedInvoiceAmount = billingInstructionSection?.SuggestedInvoiceAmount ?? 0m
                    };

                    billingInstructions.Add(billingInstruction);
                }
            }

            foreach (var cancelledProducer in cancelledProducers.CancelledProducers)
            {
                var billingInstruction = new ProducerResultFileSuggestedBillingInstruction
                {
                    CalculatorRunId = calcResult.CalcResultDetail.RunId,
                    ProducerId = cancelledProducer.ProducerId,
                    TotalProducerBillWithBadDebt = null,
                    CurrentYearInvoiceTotalToDate = cancelledProducer.LatestInvoice?.CurrentYearInvoicedTotalToDateValue,
                    TonnageChangeSinceLastInvoice = null,
                    AmountLiabilityDifferenceCalcVsPrev = null,
                    MaterialPoundThresholdBreached = null,
                    TonnagePoundThresholdBreached = null,
                    PercentageLiabilityDifferenceCalcVsPrev = null,
                    MaterialPercentageThresholdBreached = null,
                    TonnagePercentageThresholdBreached = null,
                    SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel,
                    SuggestedInvoiceAmount = null
                };
                billingInstructions.Add(billingInstruction);
            }

            if (billingInstructions.Count == 0)
            {
                throw new RunProcessingException(runContext, "No billing instructions generated.");
            }

            await bulkOps.BulkInsertAsync(dbContext, billingInstructions);

            stopwatch.Stop();
            logger.LogInformation("Inserted {RecordCount} billing instructions in {Elapsed}",
                billingInstructions.Count, stopwatch.Elapsed);
        }

        private static string? GetStringValue(string value)
        {
            return string.IsNullOrEmpty(value) || value == CommonConstants.Hyphen
                ? null
                : value;
        }
    }
}
