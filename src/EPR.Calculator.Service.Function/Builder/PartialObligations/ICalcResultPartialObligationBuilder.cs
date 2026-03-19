using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.PartialObligations
{
    public interface ICalcResultPartialObligationBuilder
    {
        Task<CalcResultPartialObligations> ConstructAsync(CalcResultsRequestDto resultsRequestDto, IEnumerable<CalcResultScaledupProducer> scaledupProducers);
    }
}
