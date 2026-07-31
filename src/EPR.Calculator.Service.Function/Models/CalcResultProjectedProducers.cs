namespace EPR.Calculator.Service.Function.Models;

public record ProjectedProducersHeader
{
    public required string Name { get; init; }
    public int ColumnIndex { get; init; }
}

public record ProjectedProducersHeaders
{
    public required ProjectedProducersHeader TitleHeader { get; init; }
    public required ImmutableList<ProjectedProducersHeader> MaterialBreakdownHeaders { get; init; }
    public required ImmutableList<ProjectedProducersHeader> ColumnHeaders { get; init; }
}

public abstract record CalcResultProjectedProducer
{
    public required int ProducerId { get; init; }
    public required string? SubsidiaryId { get; init; }
    public required string Level { get; init; }
    public required string SubmissionPeriodCode { get; init; }
    public required bool IsSubtotal { get; init; }
    public abstract bool HasCompleteRamTonnage { get; }
}

public abstract record CalcResultProjectedProducer<T> : CalcResultProjectedProducer
    where T : CalcResultProjectedProducerMaterialTonnage
{
    public required ImmutableDictionary<string, T> ProjectedTonnageByMaterial { get; init; }
    public override bool HasCompleteRamTonnage => ProjectedTonnageByMaterial.All(m => !m.Value.IsWithoutRamTonnage());
}

public record CalcResultH1ProjectedProducer : CalcResultProjectedProducer<CalcResultH1ProjectedProducerMaterialTonnage>;

public record CalcResultH2ProjectedProducer : CalcResultProjectedProducer<CalcResultH2ProjectedProducerMaterialTonnage>;

public record CalcResultProjectedProducers
{
    public required ImmutableList<CalcResultH2ProjectedProducer> H2ProjectedProducers { get; init; }
    public required ImmutableList<CalcResultH1ProjectedProducer> H1ProjectedProducers { get; init; }
}
