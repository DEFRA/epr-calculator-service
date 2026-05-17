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
            var apportionmentDetails = new List<CalcResultOnePlusFourApportionmentDetail>();

            // Add disposal cost row
            var laDisposalCost = calcResult.CalcResultLapcapData.Total;

            // Add data preparation charge row
            var laDataPrepCharge = calcResult.CalcResultParameterOtherCost.LaDataPrepCharge;

            // Add total row
            var totalOnePlusFour = CreateTotalRow(laDisposalCost, laDataPrepCharge);

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

        private static CalcResultOnePlusFourApportionmentDetail CreateDataPrepChargeRow(CalcResultParameterOtherCostDetail dataPrepCharge)
        {
            return new CalcResultOnePlusFourApportionmentDetail
            {
                EnglandTotal         = dataPrepCharge.England,
                WalesTotal           = dataPrepCharge.Wales,
                ScotlandTotal        = dataPrepCharge.Scotland,
                NorthernIrelandTotal = dataPrepCharge.NorthernIreland,
                Total                = dataPrepCharge.Total
            };
        }

        private static CalcResultOnePlusFourApportionmentDetail CreateTotalRow(ByCountryValue totalLACost, CalcResultParameterOtherCostDetail dataPrepCharge)
        {
            return new CalcResultOnePlusFourApportionmentDetail
            {
                EnglandTotal         = totalLACost.England         + dataPrepCharge.England,
                WalesTotal           = totalLACost.Wales           + dataPrepCharge.Wales,
                ScotlandTotal        = totalLACost.Scotland        + dataPrepCharge.Scotland,
                NorthernIrelandTotal = totalLACost.NorthernIreland + dataPrepCharge.NorthernIreland,
                Total                = totalLACost.Total           + dataPrepCharge.Total
            };
        }

        private static CountryApportionmentData CalculateApportionment(CalcResultOnePlusFourApportionmentDetail apportionmentData)
        {
            var englandTotal =
                CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.EnglandTotal, apportionmentData.Total);
            var walesTotal =
                CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.WalesTotal, apportionmentData.Total);
            var scotlandTotal =
                CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.ScotlandTotal, apportionmentData.Total);
            var niTotal = CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.NorthernIrelandTotal, apportionmentData.Total);
            var total = CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.Total, apportionmentData.Total);
            return new CountryApportionmentData
            {
                England         = englandTotal,
                Wales           = walesTotal,
                Scotland        = scotlandTotal,
                NorthernIreland = niTotal
            };
        }
    }
}
