using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class FeeForCommsCostsWithBadDebtProvision2aMapper : IFeeForCommsCostsWithBadDebtProvision2aMapper
    {
        public CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2a Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            return new CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2a
            {
                TotalProducerFeeForCommsCostsWithoutBadDebtProvision = calcResultSummaryProducerDisposalFees.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision,
                BadDebProvisionFor2a = calcResultSummaryProducerDisposalFees.BadDebtProvisionFor2A,
                TotalProducerFeeForCommsCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision,
                EnglandTotalWithBadDebtProvision = calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvision2A,
                NorthernIrelandTotalWithBadDebtProvision = calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvision2A,
                ScotlandTotalWithBadDebtProvision = calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvision2A,
                WalesTotalWithBadDebtProvision = calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvision2A,
                PercentageOfProducerTonnageVsAllProducers = calcResultSummaryProducerDisposalFees.PercentageofProducerReportedTonnagevsAllProducers,
            };
        }
    }
}
