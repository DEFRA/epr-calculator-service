using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class FeeForCommsCostsWithBadDebtProvision2bMapper : IFeeForCommsCostsWithBadDebtProvision2bMapper
    {
        public CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2b Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            return new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2b
            {
                TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision = calcResultSummaryProducerDisposalFees.TotalProducerFeeWithoutBadDebtFor2bComms,
                BadDebtProvisionFor2bComms = calcResultSummaryProducerDisposalFees.BadDebtProvisionFor2bComms,
                TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision = calcResultSummaryProducerDisposalFees.TotalProducerFeeWithBadDebtFor2bComms,
                EnglandTotalWithBadDebtProvision = calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtFor2bComms,
                WalesTotalWithBadDebtProvision = calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtFor2bComms,
                ScotlandTotalWithBadDebtProvision = calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtFor2bComms,
                NorthernIrelandTotalWithBadDebtProvision = calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtFor2bComms
            };
        }
    }
}