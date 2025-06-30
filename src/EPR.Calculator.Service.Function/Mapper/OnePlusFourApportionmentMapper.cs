using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Mapper
{
    internal class OnePlusFourApportionmentMapper : IOnePlusFourApportionmentMapper
    {
        public CalcResultOnePlusFourApportionmentJson Map(CalcResultOnePlusFourApportionment calcResultOnePlusFourApportionment)
        {
            var i = 1;

            return new CalcResultOnePlusFourApportionmentJson()
            {
                OneFeeForLADisposalCosts = calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.
                Where(t => t.OrderId == i && !string.IsNullOrWhiteSpace(t.Name)).
                Select(y => new CalcResultOnePlusFourApportionmentDetailJson
                {
                    England = CurrencyConverter.ConvertToCurrency(y.EnglandTotal),
                    Scotland = CurrencyConverter.ConvertToCurrency(y.ScotlandTotal),
                    Wales = CurrencyConverter.ConvertToCurrency(y.WalesTotal),
                    NorthernIreland = CurrencyConverter.ConvertToCurrency(y.NorthernIrelandTotal),
                    Total = y.Total
                }).SingleOrDefault(),
                FourLADataPrepCharge = calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.
                Where(t => t.OrderId == i + 1).
                Select(y => new CalcResultOnePlusFourApportionmentDetailJson
                {
                    England = CurrencyConverter.ConvertToCurrency(y.EnglandTotal),
                    Scotland = CurrencyConverter.ConvertToCurrency(y.ScotlandTotal),
                    Wales = CurrencyConverter.ConvertToCurrency(y.WalesTotal),
                    NorthernIreland = CurrencyConverter.ConvertToCurrency(y.NorthernIrelandTotal),
                    Total = y.Total
                }).SingleOrDefault(),
                TotalOfonePlusFour = calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.
                Where(t => t.OrderId == i + 2).
                Select(y => new CalcResultOnePlusFourApportionmentDetailJson
                {
                    England = CurrencyConverter.ConvertToCurrency(y.EnglandTotal),
                    Scotland = CurrencyConverter.ConvertToCurrency(y.ScotlandTotal),
                    Wales = CurrencyConverter.ConvertToCurrency(y.WalesTotal),
                    NorthernIreland = CurrencyConverter.ConvertToCurrency(y.NorthernIrelandTotal),
                    Total = y.Total
                }).SingleOrDefault(),
                OnePlusFourApportionmentPercentages = calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.
                Where(t => t.OrderId == i + 3).
                Select(y => new CalcResultOnePlusFourApportionmentDetailJson
                {
                    England = $"{Math.Round(y.EnglandTotal, (int)DecimalPlaces.Eight).ToString()}%",
                    Scotland = $"{Math.Round(y.ScotlandTotal, (int)DecimalPlaces.Eight).ToString()}%",
                    Wales = $"{Math.Round(y.WalesTotal, (int)DecimalPlaces.Eight).ToString()}%",
                    NorthernIreland = $"{Math.Round(y.NorthernIrelandTotal, (int)DecimalPlaces.Eight).ToString()}%",
                    Total = y.Total
                }).SingleOrDefault()
            };
        }
    }
}
