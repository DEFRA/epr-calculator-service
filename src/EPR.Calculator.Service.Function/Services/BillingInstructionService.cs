﻿using System;
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

                foreach (var producer in producers)
                {
                    var isProducerIdParseSuccessful = int.TryParse(producer.ProducerId, out var producerId);
                    var isSuggestedInvoiceAmountParseSuccessful = decimal.TryParse(producer.SuggestedInvoiceAmount, out var suggestedInvoiceAmount);

                    if (isProducerIdParseSuccessful && isSuggestedInvoiceAmountParseSuccessful)
                    {
                        var billingInstruction = new ProducerResultFileSuggestedBillingInstruction
                        {
                            CalculatorRunId = calcResult.CalcResultDetail.RunId,
                            ProducerId = producerId,
                            TotalProducerBillWithBadDebt = producer.TotalProducerBillWithBadDebtProvision,
                            CurrentYearInvoiceTotalToDate = IsDefaultValue(producer.CurrentYearInvoiceTotalToDate) ? null: TypeConverterUtil.ConvertTo<decimal>(producer.CurrentYearInvoiceTotalToDate),
                            TonnageChangeSinceLastInvoice = IsDefaultValue(producer.TonnageChangeSinceLastInvoice)? null: TypeConverterUtil.ConvertTo<string>(producer.TonnageChangeSinceLastInvoice),
                            AmountLiabilityDifferenceCalcVsPrev = IsDefaultValue(producer.LiabilityDifference) ? null : TypeConverterUtil.ConvertTo<decimal>(producer.LiabilityDifference) ,
                            MaterialPoundThresholdBreached = IsDefaultValue(producer.MaterialThresholdBreached) ? null : TypeConverterUtil.ConvertTo<string>(producer.MaterialThresholdBreached),
                            TonnagePoundThresholdBreached = IsDefaultValue(producer.TonnageThresholdBreached) ? null : TypeConverterUtil.ConvertTo<string>(producer.TonnageThresholdBreached),
                            PercentageLiabilityDifferenceCalcVsPrev = IsDefaultValue(producer.PercentageLiabilityDifference) ? null : TypeConverterUtil.ConvertTo<decimal>(producer.PercentageLiabilityDifference) ,
                            MaterialPercentageThresholdBreached = IsDefaultValue(producer.MaterialPercentageThresholdBreached) ? null : TypeConverterUtil.ConvertTo<string>(producer.MaterialPercentageThresholdBreached),
                            TonnagePercentageThresholdBreached = IsDefaultValue(producer.TonnagePercentageThresholdBreached) ? null : TypeConverterUtil.ConvertTo<string>(producer.TonnagePercentageThresholdBreached),
                            SuggestedBillingInstruction = producer.SuggestedBillingInstruction,
                            SuggestedInvoiceAmount = suggestedInvoiceAmount
                        };

                        billingInstructions.Add(billingInstruction);
                    }
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

        private bool IsDefaultValue(string value)
        {
            return (string.IsNullOrEmpty(value) || value == CommonConstants.Hyphen);
        }       
    }
}
