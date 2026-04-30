using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.CommsCost
{
    public interface ICalcResultCommsCostBuilder
    {
        Task<CalcResultCommsCost> ConstructAsync(
            RunContext runContext,
            CalcResultOnePlusFourApportionment apportionment,
            CalcResult calcResult);
    }
}