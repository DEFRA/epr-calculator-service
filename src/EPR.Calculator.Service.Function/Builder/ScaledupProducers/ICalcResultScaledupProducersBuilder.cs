using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.ScaledupProducers
{
    public interface ICalcResultScaledupProducersBuilder
    {
        Task<CalcResultScaledupProducers> ConstructAsync(RunContext runContext);
    }
}