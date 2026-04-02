using EPR.Calculator.Service.Function.Models;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.Services
{
    public class PrepareProducerDataInsertService : IPrepareProducerDataInsertService
    {

        private IBillingInstructionService billingInstructionService { get; init; }
        private IProducerInvoiceNetTonnageService producerInvoiceNetTonnageService {  get; init; }

        public PrepareProducerDataInsertService(IBillingInstructionService billingInstructionService, 
            IProducerInvoiceNetTonnageService producerInvoiceNetTonnageService,
            ILogger<PrepareProducerDataInsertService> logger)
        {
            this.billingInstructionService = billingInstructionService;
            this.producerInvoiceNetTonnageService = producerInvoiceNetTonnageService;
            this.logger = logger;
        }      

        private readonly ILogger<PrepareProducerDataInsertService> logger;

        public async Task<bool> InsertProducerDataToDatabase(CalcResult calcResult)
        {
            try
            {
                var IsBiilingInstructionsInserted = await billingInstructionService.CreateBillingInstructions(calcResult);

                var IsProduceInvoiceTonnageInserted = await producerInvoiceNetTonnageService.CreateProducerInvoiceNetTonnage(calcResult);

                return IsBiilingInstructionsInserted && IsProduceInvoiceTonnageInserted;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error occurred while populating the billing instructions");

                return false;
            }
        }

    }
}
