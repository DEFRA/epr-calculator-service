using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Builder.ScaledupProducers
{
    public interface ICalcResultScaledupProducersBuilder
    {
        Task<CalcResultScaledupProducers> Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult);
    }
}
