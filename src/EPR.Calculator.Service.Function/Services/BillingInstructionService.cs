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

namespace EPR.Calculator.Service.Function.Services
{
    public class BillingInstructionService : IBillingInstructionService
    {
        private IDbLoadingChunkerService<BillingInstruction> billingInstructionChunker { get; init; }

        private readonly ICalculatorTelemetryLogger telemetryLogger;

        public BillingInstructionService(
            IDbLoadingChunkerService<BillingInstruction> billingInstructionChunker,
            ICalculatorTelemetryLogger telemetryLogger)
        {
            this.billingInstructionChunker = billingInstructionChunker;
            this.telemetryLogger = telemetryLogger;
        }

        public async Task<bool> CreateBillingInstructions(CalcResult calcResult)
        {
            try
            {
                var startTime = DateTime.Now;

                // TODO: Delete the BillingInstruction class from this project and use it from the API.Data project
                var billingInstructions = new List<BillingInstruction>();

                var producers = calcResult.CalcResultSummary.ProducerDisposalFees.Where(producer => producer.Level == CommonConstants.LevelOne.ToString());

                foreach (var producer in producers)
                {
                    var isProducerIdParseSuccessful = int.TryParse(producer.ProducerId, out var producerId);
                    var isSuggestedInvoiceAmountParseSuccessful = decimal.TryParse(producer.SuggestedInvoiceAmount, out var suggestedInvoiceAmount);

                    if (isProducerIdParseSuccessful && isSuggestedInvoiceAmountParseSuccessful)
                    {
                        var billingInstruction = new BillingInstruction
                        {
                            CalculatorRunId = calcResult.CalcResultDetail.RunId,
                            ProducerId = producerId,
                            TotalProducerBillWithBadDebt = producer.TotalProducerBillWithBadDebtProvision,
                            CurrentYearInvoiceTotalToDate = GetParsedValue(producer.CurrentYearInvoiceTotalToDate),
                            TonnageChangeSinceLastInvoice = producer.TonnageChangeSinceLastInvoice,
                            AmountLiabilityDifferenceCalcVsPrev = GetParsedValue(producer.LiabilityDifference),
                            MaterialPoundThresholdBreached = producer.MaterialThresholdBreached,
                            TonnagePoundThresholdBreached = producer.TonnageThresholdBreached,
                            PercentageLiabilityDifferenceCalcVsPrev = GetParsedValue(producer.PercentageLiabilityDifference),
                            MaterialPercentageThresholdBreached = producer.MaterialPercentageThresholdBreached,
                            TonnagePercentageThresholdBreached = producer.TonnagePercentageThresholdBreached,
                            SuggestedBillingInstruction = producer.SuggestedBillingInstruction,
                            SuggestedInvoiceAmount = suggestedInvoiceAmount
                        };


                        billingInstructions.Add(billingInstruction);
                    }
                }

                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = calcResult.CalcResultDetail.RunId,
                    RunName = calcResult.CalcResultDetail.RunName,
                    Message = $"Inserting records {billingInstructions.Count} into billing instructions table for {calcResult.CalcResultDetail.RunId} started",
                });

                await this.billingInstructionChunker.InsertRecords(billingInstructions);

                var endTime = DateTime.Now;
                var timeDiff = startTime - endTime;
                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = calcResult.CalcResultDetail.RunId,
                    RunName = calcResult.CalcResultDetail.RunName,
                    Message = $"Inserting records {billingInstructions.Count} into billing instructions table for {calcResult.CalcResultDetail.RunId} completed in {timeDiff.TotalSeconds} seconds",
                });

                return true;
            }
            catch(Exception exception)
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

        private decimal? GetParsedValue(string value)
        {
            if (string.IsNullOrEmpty(value) || value == CommonConstants.Hyphen)
            {
                return null;
            }

            var isParseSuccessful = decimal.TryParse(value, out decimal result);

            return isParseSuccessful ? result : null;
        }
    }
}
