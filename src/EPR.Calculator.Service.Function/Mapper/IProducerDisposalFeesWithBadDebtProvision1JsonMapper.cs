using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Mapper
{
    public interface IProducerDisposalFeesWithBadDebtProvision1JsonMapper
    {
        ProducerDisposalFeesWithBadDebtProvision1 Map(Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> producerDisposalFeesByMaterial);
    }
}
