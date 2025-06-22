using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class FeeForCommsCostsWithBadDebtProvision2bMapper : IFeeForCommsCostsWithBadDebtProvision2bMapper
    {
        public CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2b Map(CalcResultSummaryProducerDisposalFees source)
        {
            return new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2b
            {
                TotalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision = source.TotalProducerFeeWithoutBadDebtFor2bComms,
                BadDebtProvisionFor2bComms = source.BadDebtProvisionFor2bComms,
                TotalProducerFeeForCommsCostsUKWideWithBadDebtProvision = source.TotalProducerFeeWithBadDebtFor2bComms,
                EnglandTotalWithBadDebtProvision = source.EnglandTotalWithBadDebtFor2bComms,
                WalesTotalWithBadDebtProvision = source.WalesTotalWithBadDebtFor2bComms,
                ScotlandTotalWithBadDebtProvision = source.ScotlandTotalWithBadDebtFor2bComms,
                NorthernIrelandTotalWithBadDebtProvision = source.NorthernIrelandTotalWithBadDebtFor2bComms
            };
        }
    }
}