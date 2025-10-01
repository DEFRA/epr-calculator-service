using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public interface IFeeForSaSetUpCostsWithBadDebtProvision5Mapper
    {
        FeeForSaSetUpCostsWithBadDebtProvision5 Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees);
    }
}
