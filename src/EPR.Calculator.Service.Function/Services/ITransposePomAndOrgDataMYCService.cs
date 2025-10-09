using EPR.Calculator.Service.Function.Dtos;
using System.Threading;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Services
{
    public interface ITransposePomAndOrgDataMYCService
    {
        Task<bool> TransposeBeforeResultsFileAsync(CalcResultsRequestDto resultsRequestDto, string? runName, CancellationToken cancellationToken);
    }
}
