using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class SAOperatingCostsWithBadDebtProvisionMapper : ISAOperatingCostsWithBadDebtProvisionMapper
    {
        public CalcResultSAOperatingCostsWithBadDebtProvision Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            return new CalcResultSAOperatingCostsWithBadDebtProvision
            {
                TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision = calcResultSummaryProducerDisposalFees.Total3SAOperatingCostswithBadDebtprovision,
                BadDebProvisionFor3 = calcResultSummaryProducerDisposalFees.BadDebtProvisionFor3,
                TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision = calcResultSummaryProducerDisposalFees.Total3SAOperatingCostwoBadDebtprovision,
                EnglandTotalForSAOperatingCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvision3,
                ScotlandTotalForSAOperatingCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvision3,
                WalesTotalForSAOperatingCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvision3,
                NorthernIrelandTotalForSAOperatingCostsWithBadDebtProvision = calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvision3
            };
        }
    }
}
