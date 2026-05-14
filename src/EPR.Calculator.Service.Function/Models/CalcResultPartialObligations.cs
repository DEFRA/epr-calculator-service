namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultPartialObligations
    {
        public ImmutableList<MaterialDetail>? Materials { get; set; }

        public ImmutableList<CalcResultPartialObligation>? PartialObligations { get; set; }
    }
}
