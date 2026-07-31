namespace EPR.Calculator.Service.Function.Models;

public record CalcResultPartialObligation
{
    public required int ProducerId { get; init; }
    public required string? SubsidiaryId { get; init; }
    public required string? ProducerName { get; init; }
    public required string? TradingName { get; init; }
    public required string Level { get; init; }
    public required int SubmissionYear { get; init; }
    public required int DaysInSubmissionYear { get; init; }
    public required string? JoiningDate { get; init; }
    public required int? DaysObligated { get; init; }
    public required decimal ObligatedFactor { get; init; }

    // todo: should be required init
    public IReadOnlyDictionary<string, CalcResultPartialObligationTonnage> PartialObligationTonnageByMaterial { get; set; } = ImmutableDictionary<string, CalcResultPartialObligationTonnage>.Empty;
}
