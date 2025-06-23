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
                TotalProducerFeeForLADisposalCostsWithoutBadDebtProvision = calcResultSummaryProducerDisposalFees.TotalProducerFeeforLADisposalCostswoBadDebtprovision,
                BadDebtProvisionForLADisposalCosts = calcResultSummaryProducerDisposalFees.BadDebtProvisionFor1,
                TotalProducerFeeForLADisposalCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.TotalProducerFeeforLADisposalCostswithBadDebtprovision,
                EnglandTotalForLADisposalCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvision,
                WalesTotalForLADisposalCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvision,
                ScotlandTotalForLADisposalCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvision,
                NorthernIrelandTotalForLADisposalCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvision,
            };
        }
    }
}
