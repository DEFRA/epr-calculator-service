using EPR.Calculator.Service.Function.Misc;

namespace EPR.Calculator.Service.Function.Interface
{
    public interface ITransposePomAndOrgDataService
    {
        Task<bool> TransposeBeforeResultsFileAsync(CalcResultsRequestDto resultsRequestDto, string? runName, CancellationToken cancellationToken);
    }
}
