using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class TotalProducerFeeWithBadDebtProvisionFor2con_1_2a_2b_2cMapper : ITotalProducerFeeWithBadDebtProvisionFor2Con12A2B2CMapper
    {
        public TotalProducerFeeWithBadDebtProvisionFor2Con12A2B2CMapper Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees)
        {
            return new TotalProducerFeeWithBadDebtProvisionFor2Con12A2B2CMapper
            {
                TotalFeeWithBadDebtProvision = CurrencyConverter.ConvertToCurrency(calcResultSummaryProducerDisposalFees.ProducerTotalOnePlus2A2B2CWithBadDeptProvision),
                ProducerPercentageOfOverallProducerCost = $"{calcResultSummaryProducerDisposalFees.ProducerOverallPercentageOfCostsForOnePlus2A2B2C.ToString("F8")}%"
            };

        }
    }
}
