using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CalcResultLADataPrepCostsWithBadDebtProvisionMapper : ICalcResultLADataPrepCostsWithBadDebtProvisionMapper
    {
        public CalcResultLADataPrepCostsWithBadDebtProvision Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            return new()
            {
                TotalProducerFeeForLADataPrepCostsWithoutBadDebtProvision = calcResultSummaryProducerDisposalFees.LaDataPrepCostsTotalWithoutBadDebtProvisionSection4,
                BadDebtProvisionFor4 = calcResultSummaryProducerDisposalFees.LaDataPrepCostsBadDebtProvisionSection4,
                TotalProducerFeeForLADataPrepCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.LaDataPrepCostsTotalWithBadDebtProvisionSection4,
                EnglandTotalForLADataPrepCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4,
                WalesTotalForLADataPrepCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4,
                ScotlandTotalForLADataPrepCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4,
                NorthernIrelandTotalForLADataPrepCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4,
            };
        }
    }
}
