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
                TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.SchemeAdministratorOperatingCostsSection.TotalProducerFeeWithBadDebtProvision),
                BadDebProvisionFor3 = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.SchemeAdministratorOperatingCostsSection.BadDebtProvision),
                TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.SchemeAdministratorOperatingCostsSection.TotalProducerFeeWithoutBadDebtProvision),
                EnglandTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.SchemeAdministratorOperatingCostsSection.EnglandTotalWithBadDebtProvision),
                WalesTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.SchemeAdministratorOperatingCostsSection.WalesTotalWithBadDebtProvision),
                ScotlandTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.SchemeAdministratorOperatingCostsSection.ScotlandTotalWithBadDebtProvision),
                NorthernIrelandTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.SchemeAdministratorOperatingCostsSection.NorthernIrelandTotalWithBadDebtProvision)
            };
        }
    }
}
