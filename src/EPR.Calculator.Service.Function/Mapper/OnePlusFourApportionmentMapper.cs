﻿using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using System;
using System.Linq;

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
                }).SingleOrDefault() ?? new CalcResultOnePlusFourApportionmentDetailJson(),

                FourLADataPrepCharge = calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.
                Where(t => t.OrderId == i + 1).
                Select(y => new CalcResultOnePlusFourApportionmentDetailJson
                {
                    England = CurrencyConverter.ConvertToCurrency(y.EnglandTotal),
                    Scotland = CurrencyConverter.ConvertToCurrency(y.ScotlandTotal),
                    Wales = CurrencyConverter.ConvertToCurrency(y.WalesTotal),
                    NorthernIreland = CurrencyConverter.ConvertToCurrency(y.NorthernIrelandTotal),
                    Total = y.Total
                }).SingleOrDefault() ?? new CalcResultOnePlusFourApportionmentDetailJson(),

                TotalOfonePlusFour = calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.
                Where(t => t.OrderId == i + 2).
                Select(y => new CalcResultOnePlusFourApportionmentDetailJson
                {
                    England = CurrencyConverter.ConvertToCurrency(y.EnglandTotal),
                    Scotland = CurrencyConverter.ConvertToCurrency(y.ScotlandTotal),
                    Wales = CurrencyConverter.ConvertToCurrency(y.WalesTotal),
                    NorthernIreland = CurrencyConverter.ConvertToCurrency(y.NorthernIrelandTotal),
                    Total = y.Total
                }).SingleOrDefault() ?? new CalcResultOnePlusFourApportionmentDetailJson(),

                OnePlusFourApportionmentPercentages = calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.
                Where(t => t.OrderId == i + 3).
                Select(y => new CalcResultOnePlusFourApportionmentDetailJson
                {
                    England = $"{Math.Round(y.EnglandTotal, (int)DecimalPlaces.Eight).ToString()}%",
                    Scotland = $"{Math.Round(y.ScotlandTotal, (int)DecimalPlaces.Eight).ToString()}%",
                    Wales = $"{Math.Round(y.WalesTotal, (int)DecimalPlaces.Eight).ToString()}%",
                    NorthernIreland = $"{Math.Round(y.NorthernIrelandTotal, (int)DecimalPlaces.Eight).ToString()}%",
                    Total = y.Total
                }).SingleOrDefault() ?? new CalcResultOnePlusFourApportionmentDetailJson()
            };
        }
    }
}
