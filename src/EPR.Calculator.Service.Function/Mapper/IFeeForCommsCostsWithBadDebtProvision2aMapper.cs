using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public interface IFeeForCommsCostsWithBadDebtProvision2aMapper
    {
        CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2a Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees);
    }
}
