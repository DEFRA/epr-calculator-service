using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Builder
{
    public interface ICalcResultBuilder
    {
        Task<CalcResult> BuildAsync(CalcResultsRequestDto resultsRequestDto);
    }
}
