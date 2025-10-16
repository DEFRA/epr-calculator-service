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
        public IEnumerable<ProducerInvoicedDto> GetLatestProducerDetailsForThisFinancialYear(string financialYear);

        public IEnumerable<ProducerDetailDto> GetProducers(int runId);
    }
}
