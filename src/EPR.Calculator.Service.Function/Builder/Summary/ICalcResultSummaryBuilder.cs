using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Builder.Summary
{
    public interface ICalcResultSummaryBuilder
    {
        Task<CalcResultSummary> ConstructAsync(int runId, RelativeYear relativeYear, bool isBillingFile, CalcResult calcResult);
    }
}
