namespace EPR.Calculator.Service.Function.Models
{
    public record CalcResultLateReportingTonnageDetail
    {
        required public string Name { get; init; }

        required public decimal TotalLateReportingTonnage { get; init; }

        required public decimal RedLateReportingTonnage { get; init; }

        required public decimal AmberLateReportingTonnage { get; init; }

        required public decimal GreenLateReportingTonnage { get; init; }
    }
}
