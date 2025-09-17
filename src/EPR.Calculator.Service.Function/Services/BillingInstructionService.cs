using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Models;
using Microsoft.ApplicationInsights;
using EPR.Calculator.Service.Function.Misc;


namespace EPR.Calculator.Service.Function.Services
{
    public class BillingInstructionService : IBillingInstructionService
    {
        private IDbLoadingChunkerService<ProducerResultFileSuggestedBillingInstruction> billingInstructionChunker { get; init; }

        private readonly ICalculatorTelemetryLogger telemetryLogger;

        public BillingInstructionService(
            IDbLoadingChunkerService<ProducerResultFileSuggestedBillingInstruction> billingInstructionChunker,
            ICalculatorTelemetryLogger telemetryLogger)
        {
            this.billingInstructionChunker = billingInstructionChunker;
            this.telemetryLogger = telemetryLogger;
        }

        public async Task<bool> CreateBillingInstructions(CalcResult calcResult)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                var billingInstructions = new List<ProducerResultFileSuggestedBillingInstruction>();

                var producers = calcResult.CalcResultSummary.ProducerDisposalFees.Where(producer => producer.Level == CommonConstants.LevelOne.ToString());
                var cancelledProducers = calcResult.CalcResultCancelledProducers;

                foreach (var producer in producers)
                {
                    var billingInstructionSection = producer.BillingInstructionSection;

                    var isProducerIdParseSuccessful = int.TryParse(producer.ProducerId, out var producerId);
                    var isSuggestedInvoiceAmountParseSuccessful = decimal.TryParse(billingInstructionSection.SuggestedInvoiceAmount, out var suggestedInvoiceAmount);

                    if (isProducerIdParseSuccessful && isSuggestedInvoiceAmountParseSuccessful)
                    {
                        var billingInstruction = new ProducerResultFileSuggestedBillingInstruction
                        {
                            CalculatorRunId = calcResult.CalcResultDetail.RunId,
                            ProducerId = producerId,
                            TotalProducerBillWithBadDebt = producer.TotalProducerBillBreakdownCosts!.TotalProducerFeeWithBadDebtProvision,
                            CurrentYearInvoiceTotalToDate = GetValue(billingInstructionSection.CurrentYearInvoiceTotalToDate!),
                            TonnageChangeSinceLastInvoice = GetStringValue(billingInstructionSection.TonnageChangeSinceLastInvoice!),
                            AmountLiabilityDifferenceCalcVsPrev = GetValue(billingInstructionSection.LiabilityDifference!),
                            MaterialPoundThresholdBreached = GetStringValue(billingInstructionSection.MaterialThresholdBreached!),
                            TonnagePoundThresholdBreached = GetStringValue(billingInstructionSection.TonnageThresholdBreached!),
                            PercentageLiabilityDifferenceCalcVsPrev = GetValue(billingInstructionSection.PercentageLiabilityDifference!),
                            MaterialPercentageThresholdBreached = GetStringValue(billingInstructionSection.MaterialPercentageThresholdBreached!),
                            TonnagePercentageThresholdBreached = GetStringValue(billingInstructionSection.TonnagePercentageThresholdBreached!),
                            SuggestedBillingInstruction = billingInstructionSection.SuggestedBillingInstruction!,
                            SuggestedInvoiceAmount = suggestedInvoiceAmount
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
                    await this.billingInstructionChunker.InsertRecords(billingInstructions);
                }
                else
                {
                    this.telemetryLogger.LogInformation(new TrackMessage
                    {
                        RunId = calcResult.CalcResultDetail.RunId,
                        RunName = calcResult.CalcResultDetail.RunName,
                        Message = $"No billing instructions to insert into table for {calcResult.CalcResultDetail.RunId}",
                    });
                    return false;
                }

                var endTime = DateTime.UtcNow;
                var timeDiff = startTime - endTime;
                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = calcResult.CalcResultDetail.RunId,
                    RunName = calcResult.CalcResultDetail.RunName,
                    Message = $"Inserting records {billingInstructions.Count} into billing instructions table for {calcResult.CalcResultDetail.RunId} completed in {timeDiff.TotalSeconds} seconds",
                });

                return true;

            }
            catch (Exception exception)
            {
                this.telemetryLogger.LogError(new ErrorMessage
                {
                    RunId = calcResult.CalcResultDetail.RunId,
                    RunName = calcResult.CalcResultDetail.RunName,
                    Message = "Error occurred while populating the billing instructions",
                    Exception = exception,
                });

                return false;
            }
        }

        private bool IsDefaultValue(string value)
        {
            return (string.IsNullOrEmpty(value) || value == CommonConstants.Hyphen);
        }

        private decimal? GetValue(string value)
        {
            return IsDefaultValue(value)
                ? null
                : TypeConverterUtil.ConvertTo<decimal>(value);
        }

        private string? GetStringValue(string value)
        {
            return IsDefaultValue(value)
                ? null
                : TypeConverterUtil.ConvertTo<string>(value);
        }
    }
}
