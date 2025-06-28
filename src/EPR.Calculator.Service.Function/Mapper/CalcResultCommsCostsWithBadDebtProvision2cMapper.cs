using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CalcResultCommsCostsWithBadDebtProvision2cMapper : ICalcResultCommsCostsWithBadDebtProvision2cMapper
    {
        public CalcResultsCommsCostsWithBadDebtProvision2c Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            return new CalcResultsCommsCostsWithBadDebtProvision2c
            {
                TotalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt),
                EnglandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCEnglandTotalWithBadDebt),
                NorthernIrelandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCNorthernIrelandTotalWithBadDebt),
                ScotlandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCScotlandTotalWithBadDebt),               
                BadDebProvisionFor2c = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCBadDebtProvision),
                WalesTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCWalesTotalWithBadDebt),
                TotalProducerFeeForCommsCostsByCountryWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCTotalProducerFeeForCommsCostsWithBadDebt)
            };
        }
    }
}
