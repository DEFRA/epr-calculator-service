using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.RejectedProducers
{
    public interface ICalcResultRejectedProducersBuilder
    {
        public Task<IEnumerable<CalcResultRejectedProducer>> ConstructAsync(CalcResultsRequestDto resultsRequestDto);
    }
}
