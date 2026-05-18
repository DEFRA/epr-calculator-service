namespace EPR.Calculator.Service.Function.Models
{
    public record CalcResultLateReportingTonnage
    {
        required public Dictionary<string, CalcResultLateReportingTonnageDetail> LateReportingTonnageByMaterial { get; init; }
        public CalcResultLateReportingTonnageDetail LateReportingTonnageTotal { get; set; }
    }
}
