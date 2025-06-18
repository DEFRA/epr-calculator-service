using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter
{
    public interface ICalcResultSummaryProducerDisposalFeesExporter
    {
        /// <summary>
        /// Exports the disposal fees summary to a JSON string.
        /// </summary>
        /// <param name="summary">The disposal fees summary.</param>
        /// <returns>The data in JSON format.</returns>
        string Export(CalcResultSummaryProducerDisposalFees summary);
    }
}