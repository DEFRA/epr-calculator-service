using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Services
{
    public class PrepareProducerDataInsertService : IPrepareProducerDataInsertService
    {

        private IBillingInstructionService billingInstructionService { get; init; }
        private IProducerInvoiceNetTonnageService producerInvoiceNetTonnageService {  get; init; }

        public PrepareProducerDataInsertService(IBillingInstructionService billingInstructionService, 
            IProducerInvoiceNetTonnageService producerInvoiceNetTonnageService,
            ICalculatorTelemetryLogger telemetryLogger)
        {
            this.billingInstructionService = billingInstructionService;
            this.producerInvoiceNetTonnageService = producerInvoiceNetTonnageService;
            this.telemetryLogger = telemetryLogger;
        }      

        private readonly ICalculatorTelemetryLogger telemetryLogger;

        public async Task<bool> InsertProducerDataToDatabase(CalcResult calcResult)
        {
            try
            {
                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = calcResult.CalcResultDetail.RunId,
                    RunName = calcResult.CalcResultDetail.RunName,
                    Message = "Create billing instructions end...",
                });
                var IsBiilingInstructionsInserted = await this.billingInstructionService.CreateBillingInstructions(calcResult);

                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = calcResult.CalcResultDetail.RunId,
                    RunName = calcResult.CalcResultDetail.RunName,
                    Message = "Create billing instructions end...",
                });

                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = calcResult.CalcResultDetail.RunId,
                    RunName = calcResult.CalcResultDetail.RunName,
                    Message = "Create producer Invoice Tonnage start...",
                });

                var IsProduceInvoiceTonnageInserted = await this.producerInvoiceNetTonnageService.CreateProducerInvoiceNetTonnage(calcResult);

                this.telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = calcResult.CalcResultDetail.RunId,
                    RunName = calcResult.CalcResultDetail.RunName,
                    Message = "Create producer Invoice Tonnage end...",
                });

                return IsBiilingInstructionsInserted && IsProduceInvoiceTonnageInserted;
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

    }
}
