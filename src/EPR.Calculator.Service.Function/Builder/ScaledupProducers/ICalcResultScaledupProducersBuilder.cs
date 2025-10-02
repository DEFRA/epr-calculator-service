namespace EPR.Calculator.Service.Function.Builder.ScaledupProducers
{
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Models;

    public interface ICalcResultScaledupProducersBuilder
    {
        Task<CalcResultScaledupProducers> ConstructAsync(CalcResultsRequestDto resultsRequestDto);
    }
}
