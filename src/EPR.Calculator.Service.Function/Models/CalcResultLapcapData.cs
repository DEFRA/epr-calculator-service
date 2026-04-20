namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultLapcapData
    {
        public string Name { get; set; } = string.Empty;

        // TODO replace with Dictionary<material, CalcResultLapcapDataDetails>
        public required IEnumerable<CalcResultLapcapDataDetails> CalcResultLapcapDataDetails { get; set; }
    }
}
