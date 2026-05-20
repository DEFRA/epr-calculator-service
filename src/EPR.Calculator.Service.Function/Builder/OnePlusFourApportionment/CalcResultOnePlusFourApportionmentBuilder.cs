using System.Globalization;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment
{
    public interface ICalcResultOnePlusFourApportionmentBuilder
    {
        CalcResultOnePlusFourApportionment ConstructAsync(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult);
    }

    public class CalcResultOnePlusFourApportionmentBuilder : ICalcResultOnePlusFourApportionmentBuilder
    {
        public CalcResultOnePlusFourApportionment ConstructAsync(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult)
        {
            return new CalcResultOnePlusFourApportionment {
                LaDisposalCost   = calcResult.CalcResultLapcapData.Total,
                LADataPrepCharge = calcResult.CalcResultParameterOtherCost.LaDataPrepCharge
            };
        }
    }
}
