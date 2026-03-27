using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function.Models;

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
                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = calcResult.CalcResultDetail.RunId,
                    RunName = calcResult.CalcResultDetail.RunName,
                    Message = "Create billing instructions start...",
                });
                var IsBiilingInstructionsInserted = await billingInstructionService.CreateBillingInstructions(calcResult);

                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = calcResult.CalcResultDetail.RunId,
                    RunName = calcResult.CalcResultDetail.RunName,
                    Message = "Create billing instructions end...",
                });

                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = calcResult.CalcResultDetail.RunId,
                    RunName = calcResult.CalcResultDetail.RunName,
                    Message = "Create producer Invoice Tonnage start...",
                });

                var IsProduceInvoiceTonnageInserted = await producerInvoiceNetTonnageService.CreateProducerInvoiceNetTonnage(calcResult);

                telemetryLogger.LogInformation(new TrackMessage
                {
                    RunId = calcResult.CalcResultDetail.RunId,
                    RunName = calcResult.CalcResultDetail.RunName,
                    Message = "Create producer Invoice Tonnage end...",
                });

                return IsBiilingInstructionsInserted && IsProduceInvoiceTonnageInserted;
            }
            catch (Exception exception)
            {
                telemetryLogger.LogError(new ErrorMessage
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
