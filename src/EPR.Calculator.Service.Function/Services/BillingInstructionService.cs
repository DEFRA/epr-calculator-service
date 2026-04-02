using System.Diagnostics;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.Services
{
    public class BillingInstructionService : IBillingInstructionService
    {
        private IDbLoadingChunkerService<ProducerResultFileSuggestedBillingInstruction> billingInstructionChunker { get; init; }

        private readonly ILogger<BillingInstructionService> _logger;

        public BillingInstructionService(
            IDbLoadingChunkerService<ProducerResultFileSuggestedBillingInstruction> billingInstructionChunker,
            ILogger<BillingInstructionService> logger)
        {
            this.billingInstructionChunker = billingInstructionChunker;
            _logger = logger;
        }

        public async Task<bool> CreateBillingInstructions(CalcResult calcResult)
        {
            try
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
                        ProducerId =  cancelledProducer.ProducerId,
                        TotalProducerBillWithBadDebt =null,
                        CurrentYearInvoiceTotalToDate = cancelledProducer.LatestInvoice?.CurrentYearInvoicedTotalToDateValue,
                        TonnageChangeSinceLastInvoice = null,
                        AmountLiabilityDifferenceCalcVsPrev = null,
                        MaterialPoundThresholdBreached = null,
                        TonnagePoundThresholdBreached =null,
                        PercentageLiabilityDifferenceCalcVsPrev = null,
                        MaterialPercentageThresholdBreached = null,
                        TonnagePercentageThresholdBreached = null,
                        SuggestedBillingInstruction = CommonConstants.CancelStatus,
                        SuggestedInvoiceAmount = null
                    };
                    billingInstructions.Add(billingInstruction);
                }



                if (billingInstructions.Count > 0)
                {
                    await billingInstructionChunker.InsertRecords(billingInstructions);
                }
                else
                {
                    _logger.LogWarning("No billing instructions to insert");
                    return false;
                }

                stopwatch.Stop();
                _logger.LogInformation("Inserted {RecordCount} billing instructions in {Elapsed}",
                    billingInstructions.Count, stopwatch.Elapsed.ToString("g"));

                return true;

            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error occurred while populating the billing instructions");

                return false;
            }
        }

        private bool IsDefaultValue(string? value)
        {
            return (string.IsNullOrEmpty(value) || value == CommonConstants.Hyphen);
        }

        private string? GetStringValue(string value)
        {
            return IsDefaultValue(value)
                ? null
                : TypeConverterUtil.ConvertTo<string>(value);
        }
    }
}
