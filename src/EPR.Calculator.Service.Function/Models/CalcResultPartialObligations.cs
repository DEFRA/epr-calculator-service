namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultPartialObligations
    {
        public CalcResultPartialObligationHeader? TitleHeader { get; set; }

        public ImmutableList<CalcResultPartialObligationHeader>? MaterialBreakdownHeaders { get; set; }

        public ImmutableList<CalcResultPartialObligationHeader>? ColumnHeaders { get; set; }

        public ImmutableList<CalcResultPartialObligation>? PartialObligations { get; set; }
    }
}
