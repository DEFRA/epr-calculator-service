using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2a
{
    public interface ICalcResultCommsCostByMaterial2aJsonExporter
    {
        public string Export(CalcResultSummaryProducerDisposalFees calcResultSummaryProducerDisposalFees);
    }
}
