namespace EPR.Calculator.Service.Function.Models
{
    public record CalcResultLateReportingTonnageDetail
    {
        required public decimal Total { get; init; }

        required public decimal Red { get; init; }

        required public decimal Amber { get; init; }

        required public decimal Green { get; init; }
    }
}
