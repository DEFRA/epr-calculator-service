using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.LateReportingTonnages
{
    public interface ICalcResultLateReportingBuilder
    {
        Task<CalcResultLateReportingTonnage> ConstructAsync(CalcResultsRequestDto resultsRequestDto);
    }
}