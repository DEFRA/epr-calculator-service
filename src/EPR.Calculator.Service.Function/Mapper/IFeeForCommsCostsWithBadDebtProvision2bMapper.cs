using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public interface IFeeForCommsCostsWithBadDebtProvision2bMapper
    {
        CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2b Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees);
    }
}