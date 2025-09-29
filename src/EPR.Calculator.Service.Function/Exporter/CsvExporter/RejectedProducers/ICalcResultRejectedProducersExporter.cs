using EPR.Calculator.Service.Function.Models;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.RejectedProducers
{
    public interface ICalcResultRejectedProducersExporter
    {
        public void Export(IEnumerable<CalcResultRejectedProducer> rejectedProducers, StringBuilder csvContent);
    }
}
