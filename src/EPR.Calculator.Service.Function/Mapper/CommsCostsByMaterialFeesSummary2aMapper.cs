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
            var costs = calcResultSummaryProducerDisposalFees.CommunicationCostsSectionTwoA;
            return new CalcResultSummaryCommsCostsByMaterialFeesSummary2a
            {
                TotalProducerFeeForCommsCostsWithoutBadDebtProvision2a = CurrencyConverter.ConvertToCurrency(costs.TotalProducerFeeWithoutBadDebtProvision),
                TotalBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.BadDebtProvision),
                TotalProducerFeeForCommsCostsWithBadDebtProvision2a = CurrencyConverter.ConvertToCurrency(costs.TotalProducerFeeWithBadDebtProvision),
                EnglandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.EnglandTotalWithBadDebtProvision),
                WalesTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.WalesTotalWithBadDebtProvision),
                ScotlandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.ScotlandTotalWithBadDebtProvision),
                NorthernIrelandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs.NorthernIrelandTotalWithBadDebtProvision),                
            };
        }
    }
}
