using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Features.BillingRun.FileExports;

/// <summary>
///     Encapsulates the metadata for the billing files that were written to blob storage.
/// </summary>
public record BillingFileExportResult
{
    public required CalculatorRunCsvFileMetadata CsvMetadata { get; init; }
    public required CalculatorRunBillingFileMetadata JsonMetadata { get; init; }
}