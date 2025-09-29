using System;
using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CalcResultLADataPrepCostsWithBadDebtProvision4Mapper : ICalcResultLADataPrepCostsWithBadDebtProvision4Mapper
    {
        public FeeForLADataPrepCostsWithBadDebtProvision_4 Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var costs = calcResultSummaryProducerDisposalFees.LocalAuthorityDataPreparationCosts;
            return new FeeForLADataPrepCostsWithBadDebtProvision_4
            {
                TotalProducerFeeForLADataPrepCostsWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.TotalProducerFeeWithoutBadDebtProvision ?? 0),
                BadDebtProvisionFor4 = CurrencyConverter.ConvertToCurrency(costs?.BadDebtProvision ?? 0),
                TotalProducerFeeForLADataPrepCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision ?? 0),
                EnglandTotalForLADataPrepCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.EnglandTotalWithBadDebtProvision ?? 0),
                WalesTotalForLADataPrepCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.WalesTotalWithBadDebtProvision ?? 0),
                ScotlandTotalForLADataPrepCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.ScotlandTotalWithBadDebtProvision ?? 0),
                NorthernIrelandTotalForLADataPrepCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.NorthernIrelandTotalWithBadDebtProvision ?? 0)
            };
        }
    }
}
