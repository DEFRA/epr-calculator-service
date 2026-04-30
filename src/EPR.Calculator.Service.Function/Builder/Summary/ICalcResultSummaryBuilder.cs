using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary
{
    public interface ICalcResultSummaryBuilder
    {
        Task<CalcResultSummary> ConstructAsync(RunContext runContext, CalcResult calcResult);
    }
}