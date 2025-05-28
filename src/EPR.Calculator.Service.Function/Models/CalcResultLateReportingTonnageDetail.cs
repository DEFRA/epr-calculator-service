namespace EPR.Calculator.Service.Function.Models
{
    public record CalcResultLateReportingTonnageDetail
    {
        required public string Name { get; init; }

        required public decimal TotalLateReportingTonnage { get; init; }
    }
}
