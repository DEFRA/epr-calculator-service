using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Features.Calculator.FileExports;

public record CalculatorFileExportResult
{
    public required CalculatorRunCsvFileMetadata CsvMetadata { get; init; }
}