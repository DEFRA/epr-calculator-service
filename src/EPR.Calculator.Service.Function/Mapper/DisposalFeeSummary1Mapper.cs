﻿using System.Collections.Generic;
using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class DisposalFeeSummary1Mapper : IDisposalFeeSummary1Mapper
    {
        public DisposalFeeSummary1 Map(CalcResultSummaryProducerDisposalFees summary)
        {
            var tonnageByLevel = GetTonnageByLevel().TryGetValue(summary.Level!, out var values);

            return new DisposalFeeSummary1
            {
                TotalProducerDisposalFeeWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(summary.TotalProducerDisposalFee),
                BadDebtProvision = CurrencyConverter.ConvertToCurrency(summary.BadDebtProvision),
                TotalProducerDisposalFeeWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(summary.TotalProducerDisposalFeeWithBadDebtProvision),
                EnglandTotal = CurrencyConverter.ConvertToCurrency(summary.EnglandTotal),
                WalesTotal = CurrencyConverter.ConvertToCurrency(summary.WalesTotal),
                ScotlandTotal = CurrencyConverter.ConvertToCurrency(summary.ScotlandTotal),
                NorthernIrelandTotal = CurrencyConverter.ConvertToCurrency(summary.NorthernIrelandTotal),
                TonnageChangeCount = values.Count,
                TonnageChangeAdvice = values.Advice
            };
        }

        private static Dictionary<string, (string Count, string Advice)> GetTonnageByLevel()
        {
            return new Dictionary<string, (string Count, string Advice)>
            {
                { CommonConstants.LevelOne.ToString(), (CommonConstants.DefaultMinValue.ToString(), string.Empty) },
                { CommonConstants.LevelTwo.ToString(), (CommonConstants.Hyphen.ToString(), CommonConstants.Hyphen.ToString()) },
            };
        }
    }
}
