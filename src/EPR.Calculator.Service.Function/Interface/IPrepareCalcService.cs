using EPR.Calculator.Service.Function.Misc;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Service.Function.Interface
{
    public interface IPrepareCalcService
    {
        Task<bool> PrepareCalcResultsAsync([FromBody] CalcResultsRequestDto resultsRequestDto, string? runName, CancellationToken cancellationToken);

        Task<bool> PrepareBillingResultsAsync([FromBody] CalcResultsRequestDto resultsRequestDto, string runName, CancellationToken cancellationToken);
    }
}