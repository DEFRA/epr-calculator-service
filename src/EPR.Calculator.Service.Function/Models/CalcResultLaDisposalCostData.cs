namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultLaDisposalCostData
    {
        public Dictionary<MaterialDetail, CalcResultLaDisposalCostDataDetail> ByMaterial { get; set; }

        public CalcResultLaDisposalCostDataDetail Total { get; set; }
    }
}
