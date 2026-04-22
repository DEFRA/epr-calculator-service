namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultLapcapData
    {
        public string Name { get; set; } = string.Empty;

        public required IEnumerable<CalcResultLapcapDataDetail> CalcResultLapcapDataDetail { get; set; }
    }
}
