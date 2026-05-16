using System.Globalization;
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
            const string totalLabel = "Total";
            var apportionmentDetails = new List<CalcResultOnePlusFourApportionmentDetail>();
            int orderId = 1;

            // Add the header row
            //apportionmentDetails.Add(CreateHeaderRow(orderId));

            // Add disposal cost row
            var totalLACost = GetTotalCost(calcResult, totalLabel);
            apportionmentDetails.Add(CreateDisposalDetailRow(OnePlus4ApportionmentColumnHeaders.LADisposalCost, totalLACost, orderId++));

            // Add data preparation charge row
            var dataPrepCharge = calcResult.CalcResultParameterOtherCost.LaDataPrepCharge;

            apportionmentDetails.Add(CreateDataPrepChargeRow(dataPrepCharge, orderId++));

            // Add total row
            apportionmentDetails.Add(CreateTotalRow(totalLACost, dataPrepCharge, orderId++));

            // Calculate apportionment
#pragma warning disable S1854
            var apportionmentData = CalculateApportionment(apportionmentDetails.First(x => x.OrderId == 3), orderId++);
#pragma warning restore
            apportionmentDetails.Add(apportionmentData);

            return new CalcResultOnePlusFourApportionment { CalcResultOnePlusFourApportionmentDetails = apportionmentDetails };
        }

        private static CalcResultLapcapDataDetail GetTotalCost(CalcResult calcResult, string name)
        {
            return calcResult.CalcResultLapcapData.CalcResultLapcapDataDetails
                .Single(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private static CalcResultOnePlusFourApportionmentDetail CreateDisposalDetailRow(string name, CalcResultLapcapDataDetail totalLACost, int orderId)
        {
            return new CalcResultOnePlusFourApportionmentDetail
            {
                Name = name,
                Total = totalLACost.TotalCost,
                EnglandTotal = totalLACost.EnglandCost,
                WalesTotal = totalLACost.WalesCost,
                ScotlandTotal = totalLACost.ScotlandCost,
                NorthernIrelandTotal = totalLACost.NorthernIrelandCost,
                OrderId = orderId,
            };
        }

        private static CalcResultOnePlusFourApportionmentDetail CreateDataPrepChargeRow(CalcResultParameterOtherCostDetail dataPrepCharge, int orderId)
        {
            return new CalcResultOnePlusFourApportionmentDetail
            {
                Name = OnePlus4ApportionmentColumnHeaders.LADataPrepCharge,
                Total = dataPrepCharge.Total,
                EnglandTotal = dataPrepCharge.England,
                WalesTotal = dataPrepCharge.Wales,
                ScotlandTotal = dataPrepCharge.Scotland,
                NorthernIrelandTotal = dataPrepCharge.NorthernIreland,
                OrderId = orderId
            };
        }

        private static CalcResultOnePlusFourApportionmentDetail CreateTotalRow(CalcResultLapcapDataDetail totalLACost, CalcResultParameterOtherCostDetail dataPrepCharge, int orderId)
        {
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencySymbol = "£";
            culture.NumberFormat.CurrencyPositivePattern = 0;

            return new CalcResultOnePlusFourApportionmentDetail
            {
                Name = OnePlus4ApportionmentColumnHeaders.TotalOnePlusFour,
                EnglandTotal = totalLACost.EnglandCost + dataPrepCharge.England,
                WalesTotal = totalLACost.WalesCost + dataPrepCharge.Wales,
                ScotlandTotal = totalLACost.ScotlandCost + dataPrepCharge.Scotland,
                NorthernIrelandTotal = totalLACost.NorthernIrelandCost + dataPrepCharge.NorthernIreland,
                Total = totalLACost.TotalCost + dataPrepCharge.Total,
                //Total = (totalLACost.TotalCost + dataPrepCharge.TotalValue).ToString("C", culture),
                //EnglandDisposalTotal = (totalLACost.EnglandCost + dataPrepCharge.England).ToString("C", culture),
                //WalesDisposalTotal = (totalLACost.WalesCost + dataPrepCharge.Wales).ToString("C", culture),
                //ScotlandDisposalTotal = (totalLACost.ScotlandCost + dataPrepCharge.Scotland).ToString("C", culture),
                //NorthernIrelandDisposalTotal = (totalLACost.NorthernIrelandCost + dataPrepCharge.NorthernIreland).ToString("C", culture),
                OrderId = orderId
            };
        }

        private static CalcResultOnePlusFourApportionmentDetail CalculateApportionment(CalcResultOnePlusFourApportionmentDetail apportionmentData, int orderId)
        {
            var englandTotal =
                CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.EnglandTotal, apportionmentData.Total);
            var walesTotal =
                CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.WalesTotal, apportionmentData.Total);
            var scotlandTotal =
                CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.ScotlandTotal, apportionmentData.Total);
            var niTotal = CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.NorthernIrelandTotal, apportionmentData.Total);
            var total = CalcResultLapcapDataBuilder.CalculateApportionment(apportionmentData.Total, apportionmentData.Total);
            return new CalcResultOnePlusFourApportionmentDetail
            {
                Name = OnePlus4ApportionmentColumnHeaders.OnePluseFourApportionment,
                ScotlandTotal = scotlandTotal,
                EnglandTotal = englandTotal,
                NorthernIrelandTotal = niTotal,
                WalesTotal = walesTotal,
                Total = total,
                OrderId = orderId,
            };
        }
    }
}
