using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment
{
    public interface ICalcResultOnePlusFourApportionmentBuilder
    {
        CalcResultOnePlusFourApportionment Construct(CalcResultLapcapData lapcapData, CalcResultParameterOtherCost otherCost);
    }

    public class CalcResultOnePlusFourApportionmentBuilder : ICalcResultOnePlusFourApportionmentBuilder
    {
        public CalcResultOnePlusFourApportionment Construct(CalcResultLapcapData lapcapData, CalcResultParameterOtherCost otherCost)
        {
            return new CalcResultOnePlusFourApportionment {
                LaDisposalCost   = lapcapData.Total,
                LADataPrepCharge = otherCost.LaDataPrepCharge with { }
            };
        }
    }
}
