using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1Mapper : ICalculationResultsProducerCalculationResultsFeeForLADisposalCosts1Mapper
    {
        public CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1 Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            return new CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1
            {
                TotalProducerFeeForLADisposalCostsWithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerFeeforLADisposalCostswoBadDebtprovision),
                BadDebtProvisionForLADisposalCosts = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.BadDebtProvisionFor1),
                TotalProducerFeeForLADisposalCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.TotalProducerFeeforLADisposalCostswithBadDebtprovision),
                EnglandTotalForLADisposalCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvision),
                WalesTotalForLADisposalCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvision),
                ScotlandTotalForLADisposalCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvision),
                NorthernIrelandTotalForLADisposalCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvision),
            };
        }
    }
}
