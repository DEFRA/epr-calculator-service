namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultScaledupProducers
    {
        public CalcResultScaledupProducerHeader? TitleHeader { get; set; }

        public ImmutableList<CalcResultScaledupProducerHeader>? MaterialBreakdownHeaders { get; set; }

        public ImmutableList<CalcResultScaledupProducerHeader>? ColumnHeaders { get; set; }

        public ImmutableList<CalcResultScaledupProducer>? ScaledupProducers { get; set; }
    }
}
