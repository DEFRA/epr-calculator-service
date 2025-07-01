using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CalcResultLADataPrepCostsWithBadDebtProvision4Mapper : ICalcResultLADataPrepCostsWithBadDebtProvision4Mapper
    {
        public FeeForLADataPrepCostsWithBadDebtProvision_4 Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            return new FeeForLADataPrepCostsWithBadDebtProvision_4
            {
                TotalProducerFeeForLADataPrepCostsWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.LaDataPrepCostsTotalWithoutBadDebtProvisionSection4),
                BadDebtProvisionFor4 = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.LaDataPrepCostsBadDebtProvisionSection4),
                TotalProducerFeeForLADataPrepCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.LaDataPrepCostsTotalWithBadDebtProvisionSection4),
                EnglandTotalForLADataPrepCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4),
                WalesTotalForLADataPrepCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4),
                ScotlandTotalForLADataPrepCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4),
                NorthernIrelandTotalForLADataPrepCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4)
            };
        }
    }
}
