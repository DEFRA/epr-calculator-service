using EPR.Calculator.Service.Function.Dtos;
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