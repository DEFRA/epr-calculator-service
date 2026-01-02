namespace EPR.Calculator.Service.Function.Builder.PartialObligations
{
    using System.Collections.Generic;

    using System.Threading.Tasks;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Models;

    public interface ICalcResultPartialObligationBuilder
    {
        Task<CalcResultPartialObligations> ConstructAsync(CalcResultsRequestDto resultsRequestDto, IEnumerable<CalcResultScaledupProducer> scaledupProducers);
    }
}
