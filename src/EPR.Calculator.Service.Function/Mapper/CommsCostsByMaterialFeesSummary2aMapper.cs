using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CommsCostsByMaterialFeesSummary2aMapper : ICommsCostsByMaterialFeesSummary2aMapper
    {
        public CalcResultSummaryCommsCostsByMaterialFeesSummary2a Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            return new CalcResultSummaryCommsCostsByMaterialFeesSummary2a
            {
                EnglandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvision2A),
                NorthernIrelandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvision2A),
                ScotlandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvision2A),
                TotalProducerFeeForCommsCostsWithBadDebtProvision2a = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision),
                TotalBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.BadDebtProvisionFor2A),
                WalesTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvision2A),
                TotalProducerFeeForCommsCostsWithoutBadDebtProvision2a = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision)
            };

        }
    }
}
