namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultLaDisposalCostData
    {
        public Dictionary<string, CalcResultLaDisposalCostDataDetail> ByMaterial { get; set; }

        // TODO derive
        public CalcResultLaDisposalCostDataDetail Total { get; set; }
    }
}
