using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder
{
    public interface ICalcResultBuilder
    {
        Task<CalcResult> BuildAsync(RunContext runContext);
    }
}