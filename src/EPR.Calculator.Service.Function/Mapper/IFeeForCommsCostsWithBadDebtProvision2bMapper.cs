using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public interface IFeeForCommsCostsWithBadDebtProvision2BMapper
    {
        CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2B Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees);
    }
}