using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Interface
{
    public interface IProducerDetailService
    {
        public Task<IEnumerable<ProducerInvoicedDto>> GetLatestProducerDetailsForThisFinancialYear(string financialYear,
            IEnumerable<int> missingProducersIdsInCurrentRun);

        public Task<IEnumerable<int>> GetProducers(int runId);
    }
}