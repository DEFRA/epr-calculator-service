using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Common;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Builder.CancelledProducers
{
    public interface ICalcResultCancelledProducersBuilder
    {
        Task<CalcResultCancelledProducersResponse> ConstructAsync(CalcResultsRequestDto resultsRequestDto);
    }
}