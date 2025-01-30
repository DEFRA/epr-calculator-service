namespace EPR.Calculator.Service.Function.Interface
{
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Misc;

    public interface ITransposePomAndOrgDataService
    {
        Task<TransposeResult> TransposeBeforeCalcResults(CalcResultsRequestDto resultsRequestDto, CancellationToken cancellationToken);
    }
}
