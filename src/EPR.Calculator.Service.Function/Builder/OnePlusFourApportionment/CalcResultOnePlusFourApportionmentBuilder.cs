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
        CalcResultOnePlusFourApportionment Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult);
    }

    public class CalcResultOnePlusFourApportionmentBuilder : ICalcResultOnePlusFourApportionmentBuilder
    {
        public CalcResultOnePlusFourApportionment Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult)
        {
            var apportionmentDetails = new List<CalcResultOnePlusFourApportionmentDetail>();

            // Add disposal cost row
            var laDisposalCost = calcResult.CalcResultLapcapData.Total;

            // Add data preparation charge row
            var laDataPrepCharge = calcResult.CalcResultParameterOtherCost.LaDataPrepCharge;

            // Add total row
            var totalOnePlusFour = CreateTotalOnePlusFour(laDisposalCost, laDataPrepCharge);

            // Calculate apportionment
#pragma warning disable S1854
            var apportionmentData = CalculateApportionment(totalOnePlusFour);
#pragma warning restore

            return new CalcResultOnePlusFourApportionment {
                LaDisposalCost           = laDisposalCost,
                LADataPrepCharge         = laDataPrepCharge,
                TotalOnePlusFour         = totalOnePlusFour,
                OnePlusFourApportionment = apportionmentData
            };
        }

        private static ByCountryCost CreateTotalOnePlusFour(ByCountryCost totalLACost, ByCountryCost dataPrepCharge)
        {
            return new ByCountryCost
            {
                England         = totalLACost.England         + dataPrepCharge.England,
                Wales           = totalLACost.Wales           + dataPrepCharge.Wales,
                Scotland        = totalLACost.Scotland        + dataPrepCharge.Scotland,
                NorthernIreland = totalLACost.NorthernIreland + dataPrepCharge.NorthernIreland
            };
        }

        private static ByCountryApportionment CalculateApportionment(ByCountryCost apportionmentData)
        {
            return apportionmentData.Total == 0
            ? ByCountryApportionment.Empty
            : new ByCountryApportionment
            {
                England         = 100 * apportionmentData.England         / apportionmentData.Total,
                Wales           = 100 * apportionmentData.Wales           / apportionmentData.Total,
                Scotland        = 100 * apportionmentData.Scotland        / apportionmentData.Total,
                NorthernIreland = 100 * apportionmentData.NorthernIreland / apportionmentData.Total
            };
        }
    }
}
