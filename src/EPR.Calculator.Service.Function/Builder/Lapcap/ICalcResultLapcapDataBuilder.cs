using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Builder.Lapcap
{
    public interface ICalcResultLapcapDataBuilder
    {
        Task<CalcResultLapcapData> Construct(CalcResultsRequestDto resultsRequestDto);
    }
}