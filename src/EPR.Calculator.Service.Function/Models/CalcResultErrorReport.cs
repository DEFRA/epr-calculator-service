namespace EPR.Calculator.Service.Function.Models;

public record CalcResultErrorReport
{
    public required int ProducerId { get; init; }
    public required string SubsidiaryId { get; init; }
    public required string ProducerName { get; init; }
    public required string TradingName { get; init; }
    public required string LeaverCode { get; init; }
    public required string ErrorCodeText { get; init; }
}
