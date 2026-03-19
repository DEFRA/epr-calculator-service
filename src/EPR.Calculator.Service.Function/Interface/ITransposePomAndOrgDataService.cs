using EPR.Calculator.Service.Function.Dtos;

namespace EPR.Calculator.Service.Function.Interface
{
    public interface ITransposePomAndOrgDataService
    {
        Task<bool> TransposeBeforeResultsFileAsync(CalcResultsRequestDto resultsRequestDto, string? runName, CancellationToken cancellationToken);
    }
}
