using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultLaDisposalCostData
    {
        public required string Name { get; set; }
        public IEnumerable<CalcResultLaDisposalCostDataDetail> CalcResultLaDisposalCostDetails { get; set; } = new List<CalcResultLaDisposalCostDataDetail>();
    }
}
