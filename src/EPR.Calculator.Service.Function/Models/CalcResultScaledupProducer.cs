namespace EPR.Calculator.Service.Function.Models;

public record ScaledupPomEntry(int MaterialId, string PackagingType, decimal Tonnage, decimal ScaledTonnage)
{
    public static ScaledupPomEntry Zero => new(0, "", 0, 0);
}

public record CalcResultScaledupProducer
{
    public required int ProducerId { get; init; }
    public required string? SubsidiaryId { get; init; }
    public required string? ProducerName { get; init; }
    public required string? TradingName { get; init; }
    public required string Level { get; init; }
    public required bool IsSubtotalRow { get; init; }
    public required string SubmissionPeriodCode { get; init; }
    public required int DaysInSubmissionPeriod { get; init; }
    public required int DaysInWholePeriod { get; init; }
    public required decimal ScaleupFactor { get; init; }

    // todo: should be required init
    public ImmutableList<ScaledupPomEntry> PomData { get; set; } = [];

    // todo: should be required init
    public ImmutableDictionary<string, CalcResultScaledupProducerTonnage> ScaledupProducerTonnageByMaterial { get; set; } = ImmutableDictionary<string, CalcResultScaledupProducerTonnage>.Empty;
}
