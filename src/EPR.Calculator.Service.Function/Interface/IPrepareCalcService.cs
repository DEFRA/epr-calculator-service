using EPR.Calculator.Service.Function.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Interface
{
    public interface IPrepareCalcService
    {
        Task<bool> PrepareCalcResults([FromBody] CalcResultsRequestDto resultsRequestDto, CancellationToken cancellationToken);
    }
}