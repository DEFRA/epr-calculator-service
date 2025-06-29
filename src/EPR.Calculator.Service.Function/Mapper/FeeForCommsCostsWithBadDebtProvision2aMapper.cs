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
            return new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2a
            {
                TotalProducerFeeForCommsCostsWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision),
                BadDebProvisionFor2a = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.BadDebtProvisionFor2A),
                TotalProducerFeeForCommsCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision),
                EnglandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvision2A),
                NorthernIrelandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvision2A),
                ScotlandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvision2A),
                WalesTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvision2A),
                PercentageOfProducerTonnageVsAllProducers = $"{Math.Round(calcResultSummaryProducerDisposalFees.PercentageofProducerReportedTonnagevsAllProducers, (int)DecimalPlaces.Eight).ToString()}%"
            };
        }
    }
}
