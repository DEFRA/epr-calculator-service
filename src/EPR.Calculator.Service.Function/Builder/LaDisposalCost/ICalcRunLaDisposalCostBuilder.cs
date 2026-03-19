using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.LaDisposalCost
{
    public interface ICalcRunLaDisposalCostBuilder
    {
        Task<CalcResultLaDisposalCostData> ConstructAsync(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult);
    }
}
