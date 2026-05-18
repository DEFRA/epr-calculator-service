namespace EPR.Calculator.Service.Function.Models
{
    public record CalcResultLateReportingTonnageDetail
    {
        // TODO rename these Red, Amber, Green Total
        required public decimal TotalLateReportingTonnage { get; init; }

        required public decimal RedLateReportingTonnage { get; init; }

        required public decimal AmberLateReportingTonnage { get; init; }

        required public decimal GreenLateReportingTonnage { get; init; }
    }
}
