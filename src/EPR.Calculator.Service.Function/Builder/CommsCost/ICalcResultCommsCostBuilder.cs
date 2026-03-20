using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.CommsCost
{
    public interface ICalcResultCommsCostBuilder
    {
        Task<CalcResultCommsCost> ConstructAsync(
            CalcResultsRequestDto resultsRequestDto,
            CalcResultOnePlusFourApportionment apportionment,
            CalcResult calcResult);
    }
}