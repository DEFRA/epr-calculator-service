using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultScaledupProducers
    {
        public CalcResultScaledupProducerHeader TitleHeader { get; set; }

        public IEnumerable<CalcResultScaledupProducerHeader> MaterialBreakdownHeaders { get; set; }

        public IEnumerable<CalcResultScaledupProducerHeader> ColumnHeaders { get; set; }

        required public IEnumerable<CalcResultScaledupProducer> ScaledupProducers { get; set; }
    }
}
