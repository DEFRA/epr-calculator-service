using EPR.Calculator.Service.Common.Utils;
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
                TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.Total3SAOperatingCostswithBadDebtprovision),
                BadDebtProvisionFor3 = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.BadDebtProvisionFor3),
                TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.Total3SAOperatingCostwoBadDebtprovision),
                EnglandTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.EnglandTotalWithBadDebtProvision3),
                ScotlandTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.ScotlandTotalWithBadDebtProvision3),
                WalesTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.WalesTotalWithBadDebtProvision3),
                NorthernIrelandTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.NorthernIrelandTotalWithBadDebtProvision3)
            };
        }
    }
}
