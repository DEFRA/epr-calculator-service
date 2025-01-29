namespace EPR.Calculator.Service.Function.Interface
{
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.API.Dtos;

    public interface ITransposePomAndOrgDataService
    {
        Task<bool> Transpose(CalcResultsRequestDto resultsRequestDto, CancellationToken cancellationToken);
    }
}
