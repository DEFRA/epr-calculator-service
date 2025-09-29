using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CalcResultCommsCostsWithBadDebtProvision2CMapper : ICalcResultCommsCostsWithBadDebtProvision2CMapper
    {
        public CalcResultsCommsCostsWithBadDebtProvision2C Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            return new CalcResultsCommsCostsWithBadDebtProvision2C
            {
                TotalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt),
                EnglandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCEnglandTotalWithBadDebt),
                NorthernIrelandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCNorthernIrelandTotalWithBadDebt),
                ScotlandTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCScotlandTotalWithBadDebt),               
                BadDebtProvisionFor2c = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCBadDebtProvision),
                WalesTotalWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCWalesTotalWithBadDebt),
                TotalProducerFeeForCommsCostsByCountryWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TwoCTotalProducerFeeForCommsCostsWithBadDebt)
            };
        }
    }
}
