using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.RejectedProducers
{
    public interface ICalcResultRejectedProducersBuilder
    {
        public Task<IEnumerable<CalcResultRejectedProducer>> ConstructAsync(RunContext runContext);
    }
}