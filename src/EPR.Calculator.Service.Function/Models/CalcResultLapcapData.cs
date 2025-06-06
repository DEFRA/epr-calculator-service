using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Models
{
    public record CalcResultLapcapData
    {
        public string Name { get; set; } = string.Empty;

        public required IEnumerable<CalcResultLapcapDataDetails> CalcResultLapcapDataDetails { get; set; }
    }
}
