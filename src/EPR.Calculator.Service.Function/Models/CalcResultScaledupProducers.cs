namespace EPR.Calculator.Service.Function.Models
{
    using System.Collections.Generic;

    public class CalcResultScaledupProducers
    {
        public CalcResultScaledupProducerHeader? TitleHeader { get; set; }

        public IEnumerable<CalcResultScaledupProducerHeader>? MaterialBreakdownHeaders { get; set; }

        public IEnumerable<CalcResultScaledupProducerHeader>? ColumnHeaders { get; set; }

        public IEnumerable<CalcResultScaledupProducer>? ScaledupProducers { get; set; }
    }
}
