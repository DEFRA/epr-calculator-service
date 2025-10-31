using EPR.Calculator.Service.Function.Models;
using System.Threading;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IProducerInvoiceNetTonnageService
    {
        public Task<bool> CreateProducerInvoiceNetTonnage(CalcResult calcResult, CancellationToken cancellationToken);
    }
}
