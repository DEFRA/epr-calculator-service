namespace EPR.Calculator.Service.Function.Models;

public record CalcResultScaledupProducerTonnage
{
    public required decimal ReportedHouseholdPackagingWasteTonnage { get; init; }
    public required decimal ReportedPublicBinTonnage { get; init; }
    public required decimal TotalReportedTonnage { get; init; }
    public required decimal ReportedSelfManagedConsumerWasteTonnage { get; init; }
    public required decimal NetReportedTonnage { get; init; }
    public required decimal ScaledupReportedHouseholdPackagingWasteTonnage { get; init; }
    public required decimal ScaledupReportedPublicBinTonnage { get; init; }
    public required decimal ScaledupTotalReportedTonnage { get; init; }
    public required decimal ScaledupReportedSelfManagedConsumerWasteTonnage { get; init; }
    public required decimal ScaledupNetReportedTonnage { get; init; }
    public required decimal HouseholdDrinksContainersTonnageGlass { get; init; }
    public required decimal ScaledupHouseholdDrinksContainersTonnageGlass { get; init; }
}
