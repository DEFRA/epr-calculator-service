namespace EPR.Calculator.Service.Function.Interface
{
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Function.Dtos;
    using Microsoft.AspNetCore.Mvc;

    public interface IPrepareCalcService
    {
        Task<bool> PrepareCalcResults([FromBody] CalcResultsRequestDto resultsRequestDto, string? runName, CancellationToken cancellationToken);
    }
}