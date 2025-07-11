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

                foreach (var producer in producers)
                {
                    var isProducerIdParseSuccessful = int.TryParse(producer.ProducerId, out var producerId);
                    var isSuggestedInvoiceAmountParseSuccessful = decimal.TryParse(producer.BillingInstructionSection.SuggestedInvoiceAmount, out var suggestedInvoiceAmount);

                    if (isProducerIdParseSuccessful && isSuggestedInvoiceAmountParseSuccessful)
                    {
                        var billingInstructionSection = producer.BillingInstructionSection;

                        var billingInstruction = new ProducerResultFileSuggestedBillingInstruction
                        {
                            CalculatorRunId = calcResult.CalcResultDetail.RunId,
                            ProducerId = producerId,
                            TotalProducerBillWithBadDebt = producer.TotalProducerBillBreakdownSection.TotalProducerFeeWithBadDebtProvision,
                            CurrentYearInvoiceTotalToDate = IsDefaultValue(billingInstructionSection.CurrentYearInvoiceTotalToDate!) ? null: TypeConverterUtil.ConvertTo<decimal>(billingInstructionSection.CurrentYearInvoiceTotalToDate!),
                            TonnageChangeSinceLastInvoice = IsDefaultValue(billingInstructionSection.TonnageChangeSinceLastInvoice!)? null: TypeConverterUtil.ConvertTo<string>(billingInstructionSection.TonnageChangeSinceLastInvoice!),
                            AmountLiabilityDifferenceCalcVsPrev = IsDefaultValue(billingInstructionSection.LiabilityDifference!) ? null : TypeConverterUtil.ConvertTo<decimal>(billingInstructionSection.LiabilityDifference!) ,
                            MaterialPoundThresholdBreached = IsDefaultValue(billingInstructionSection.MaterialThresholdBreached!) ? null : TypeConverterUtil.ConvertTo<string>(billingInstructionSection.MaterialThresholdBreached!),
                            TonnagePoundThresholdBreached = IsDefaultValue(billingInstructionSection.TonnageThresholdBreached!) ? null : TypeConverterUtil.ConvertTo<string>(billingInstructionSection.TonnageThresholdBreached!),
                            PercentageLiabilityDifferenceCalcVsPrev = IsDefaultValue(billingInstructionSection.PercentageLiabilityDifference!) ? null : TypeConverterUtil.ConvertTo<decimal>(billingInstructionSection.PercentageLiabilityDifference!) ,
                            MaterialPercentageThresholdBreached = IsDefaultValue(billingInstructionSection.MaterialPercentageThresholdBreached!) ? null : TypeConverterUtil.ConvertTo<string>(billingInstructionSection.MaterialPercentageThresholdBreached!),
                            TonnagePercentageThresholdBreached = IsDefaultValue(billingInstructionSection.TonnagePercentageThresholdBreached!) ? null : TypeConverterUtil.ConvertTo<string>(billingInstructionSection.TonnagePercentageThresholdBreached!),
                            SuggestedBillingInstruction = billingInstructionSection.SuggestedBillingInstruction!,
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
