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
                TotalProducerFeeForLADataPrepCostsWithoutBadDebtProvision = calcResultSummaryProducerDisposalFees.LaDataPrepCostsTotalWithoutBadDebtProvisionSection4,
                BadDebtProvisionFor4 = calcResultSummaryProducerDisposalFees.LaDataPrepCostsBadDebtProvisionSection4,
                TotalProducerFeeForLADataPrepCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.LaDataPrepCostsTotalWithBadDebtProvisionSection4,
                EnglandTotalForLADataPrepCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4,
                WalesTotalForLADataPrepCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4,
                ScotlandTotalForLADataPrepCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4,
                NorthernIrelandTotalForLADataPrepCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4
            };
        }
    }
}
