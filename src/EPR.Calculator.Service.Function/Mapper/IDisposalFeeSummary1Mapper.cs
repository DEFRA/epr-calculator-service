using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public interface IDisposalFeeSummary1Mapper
    {
        DisposalFeeSummary1 Map(CalcResultSummaryProducerDisposalFees summary);
    }
}
