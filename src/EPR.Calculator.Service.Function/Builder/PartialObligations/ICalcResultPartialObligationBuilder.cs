using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.PartialObligations
{
    public interface ICalcResultPartialObligationBuilder
    {
        Task<CalcResultPartialObligations> ConstructAsync(RunContext runContext, IEnumerable<CalcResultScaledupProducer> scaledupProducers);
    }
}