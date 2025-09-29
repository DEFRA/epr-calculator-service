using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class SAOperatingCostsWithBadDebtProvisionMapper : ISAOperatingCostsWithBadDebtProvisionMapper
    {
        public CalcResultSAOperatingCostsWithBadDebtProvision Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            var costs = calcResultSummaryProducerDisposalFees.SchemeAdministratorOperatingCosts;
            return new CalcResultSAOperatingCostsWithBadDebtProvision
            {
                TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.TotalProducerFeeWithoutBadDebtProvision ?? 0),
                BadDebtProvisionFor3 = CurrencyConverter.ConvertToCurrency(costs?.BadDebtProvision ?? 0),
                TotalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.TotalProducerFeeWithBadDebtProvision ?? 0),
                EnglandTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.EnglandTotalWithBadDebtProvision ?? 0),
                WalesTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.WalesTotalWithBadDebtProvision ?? 0),
                ScotlandTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.ScotlandTotalWithBadDebtProvision ?? 0),
                NorthernIrelandTotalForSAOperatingCostsWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(costs?.NorthernIrelandTotalWithBadDebtProvision ?? 0)
            };
        }
    }
}
