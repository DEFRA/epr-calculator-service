using System.Collections.Generic;
using System.Threading.Tasks;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.RejectedProducers
{
    public interface ICalcResultRejectedProducersBuilder
    {
        public Task<IEnumerable<CalcResultRejectedProducer>> ConstructAsync(CalcResultsRequestDto resultsRequestDto);
    }
}
