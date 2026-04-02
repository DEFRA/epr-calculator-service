using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IProducerInvoiceNetTonnageService
    {
        public Task<bool> CreateProducerInvoiceNetTonnage(CalcResult calcResult);
    }
}
