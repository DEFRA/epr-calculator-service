using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public interface ICalcResultLADataPrepCostsWithBadDebtProvision4Mapper
    {
        FeeForLADataPrepCostsWithBadDebtProvision_4 Map(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees);
    }
}
