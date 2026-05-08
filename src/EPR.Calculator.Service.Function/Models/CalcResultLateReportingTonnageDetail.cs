namespace EPR.Calculator.Service.Function.Models;

public record CalcResultLateReportingTonnageDetail
{
    public required string Name { get; init; }

    public required decimal TotalLateReportingTonnage { get; init; }

    public required decimal RedLateReportingTonnage { get; init; }

    public required decimal AmberLateReportingTonnage { get; init; }

    public required decimal GreenLateReportingTonnage { get; init; }
}