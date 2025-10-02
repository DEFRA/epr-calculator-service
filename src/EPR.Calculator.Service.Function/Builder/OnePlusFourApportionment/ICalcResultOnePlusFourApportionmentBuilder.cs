using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment;

public interface ICalcResultOnePlusFourApportionmentBuilder
{
    CalcResultOnePlusFourApportionment ConstructAsync(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult);
}