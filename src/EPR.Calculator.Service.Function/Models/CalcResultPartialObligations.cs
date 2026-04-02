namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultPartialObligations
    {
        public CalcResultPartialObligationHeader? TitleHeader { get; set; }

        public IEnumerable<CalcResultPartialObligationHeader>? MaterialBreakdownHeaders { get; set; }

        public IEnumerable<CalcResultPartialObligationHeader>? ColumnHeaders { get; set; }

        public IEnumerable<CalcResultPartialObligation>? PartialObligations { get; set; }
    }
}
