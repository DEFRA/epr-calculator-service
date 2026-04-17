using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment;

public interface ICalcResultOnePlusFourApportionmentBuilder
{
    CalcResultOnePlusFourApportionment ConstructAsync(RunContext runContext, CalcResult calcResult);
}