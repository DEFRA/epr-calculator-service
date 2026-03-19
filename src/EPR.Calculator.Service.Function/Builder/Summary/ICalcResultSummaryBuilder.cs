using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary
{
    public interface ICalcResultSummaryBuilder
    {
        Task<CalcResultSummary> ConstructAsync(int runId, RelativeYear relativeYear, bool isBillingFile, CalcResult calcResult);
    }
}
