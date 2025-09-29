using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class FeeForCommsCostsWithBadDebtProvision2aMapper : IFeeForCommsCostsWithBadDebtProvision2aMapper
    {
        public CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2a Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var costs = calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoA;
            return new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2a
            {
                TotalProducerFeeForCommsCostsWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.TotalProducerFeeWithoutBadDebtProvision),
                BadDebtProvisionFor2a = CurrencyConverter.ConvertToCurrency(costs.BadDebtProvision),
                TotalProducerFeeForCommsCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.TotalProducerFeeWithBadDebtProvision),
                EnglandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.EnglandTotalWithBadDebtProvision),
                WalesTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.WalesTotalWithBadDebtProvision),
                ScotlandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.ScotlandTotalWithBadDebtProvision),
                NorthernIrelandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.NorthernIrelandTotalWithBadDebtProvision),
                PercentageOfProducerTonnageVsAllProducers = $"{Math.Round(calcResultSummaryProducerDisposalFees.PercentageofProducerReportedTonnagevsAllProducers, (int)DecimalPlaces.Eight).ToString()}%"
            };
        }
    }
}
