using System.Collections.Immutable;

namespace EPR.Calculator.Service.Function.Models;

public record CalcResultCancelledProducersResponse
{
    public string? TitleHeader { get; set; }
    public ImmutableArray<CalcResultCancelledProducersDto> CancelledProducers { get; set; } = [];
}

public record CalcResultCancelledProducersDto
{
    public string? ProducerId_Header { get; init; }
    public string? ProducerName_Header { get; init; }
    public string? TradingName_Header { get; init; }
    public int ProducerId { get; init; }
    public string? SubsidiaryIdValue { get; init; }
    public string? ProducerOrSubsidiaryNameValue { get; init; }
    public string? TradingNameValue { get; init; }
    public LastTonnage? LastTonnage { get; init; }
    public LatestInvoice? LatestInvoice { get; init; }
}

public record LastTonnage
{
    public string? LastTonnage_Header { get; init; }
    public string? Aluminium_Header { get; init; }
    public string? FibreComposite_Header { get; init; }
    public string? Glass_Header { get; init; }
    public string? PaperOrCard_Header { get; init; }
    public string? Plastic_Header { get; init; }
    public string? Steel_Header { get; init; }
    public string? Wood_Header { get; init; }
    public string? OtherMaterials_Header { get; init; }
    public decimal? AluminiumValue { get; init; }
    public decimal? FibreCompositeValue { get; init; }
    public decimal? GlassValue { get; init; }
    public decimal? PaperOrCardValue { get; init; }
    public decimal? PlasticValue { get; init; }
    public decimal? SteelValue { get; init; }
    public decimal? WoodValue { get; init; }
    public decimal? OtherMaterialsValue { get; init; }
}

public record LatestInvoice
{
    public string? LatestInvoice_Header { get; init; }
    public string? CurrentYearInvoicedTotalToDate_Header { get; init; }
    public string? RunNumber_Header { get; init; }
    public string? RunName_Header { get; init; }
    public string? BillingInstructionId_Header { get; init; }
    public decimal? CurrentYearInvoicedTotalToDateValue { get; init; }
    public string? RunNumberValue { get; init; }
    public string? RunNameValue { get; init; }
    public string? BillingInstructionIdValue { get; init; }
}