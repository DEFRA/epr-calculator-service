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
            var laDisposalCost = GetTotalCost(calcResult);

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

        private static CalcResultLapcapDataDetail GetTotalCost(CalcResult calcResult)
        {
            // TODO store Total separately in model
            return calcResult.CalcResultLapcapData.CalcResultLapcapDataDetails
                .Single(p => p.Name.Equals("Total", StringComparison.OrdinalIgnoreCase));
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

        private static CalcResultOnePlusFourApportionmentDetail CreateTotalRow(CalcResultLapcapDataDetail totalLACost, CalcResultParameterOtherCostDetail dataPrepCharge)
        {
            return new CalcResultOnePlusFourApportionmentDetail
            {
                EnglandTotal         = totalLACost.EnglandCost         + dataPrepCharge.England,
                WalesTotal           = totalLACost.WalesCost           + dataPrepCharge.Wales,
                ScotlandTotal        = totalLACost.ScotlandCost        + dataPrepCharge.Scotland,
                NorthernIrelandTotal = totalLACost.NorthernIrelandCost + dataPrepCharge.NorthernIreland,
                Total                = totalLACost.TotalCost           + dataPrepCharge.Total
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
