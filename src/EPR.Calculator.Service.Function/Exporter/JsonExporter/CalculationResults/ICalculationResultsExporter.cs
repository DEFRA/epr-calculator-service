using EPR.Calculator.Service.Function.Models;
using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.CalcResult
{
    public interface ICalculationResultsExporter
    {
        /// <summary>
        /// Exports the calculation results summary and producer calculations to a JSON string.
        /// </summary>
        /// <param name="summary">The calculation result summary.</param>
        /// <param name="producerCalculations">The producer calculations.</param>
        /// <returns></returns>
        string Export(CalcResultSummary summary, IEnumerable<object> producerCalculations);
    }
}