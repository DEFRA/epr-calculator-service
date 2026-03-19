using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.LateReportingTonnages
{
    public interface ICalcResultLateReportingBuilder
    {
        Task<CalcResultLateReportingTonnage> ConstructAsync(CalcResultsRequestDto resultsRequestDto);
    }
}