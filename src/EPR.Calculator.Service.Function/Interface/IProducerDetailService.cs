using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Interface
{
    public interface IProducerDetailService
    {
        public Task<IEnumerable<ProducerInvoicedDto>> GetProducerDetails(RelativeYear relativeYear);

        public Task<IEnumerable<ProducerInvoicedDto>> GetProducerDetails(RelativeYear relativeYear, IEnumerable<int> missingProducersIdsInCurrentRun);

        public Task<IEnumerable<int>> GetProducers(int runId);
    }
}
