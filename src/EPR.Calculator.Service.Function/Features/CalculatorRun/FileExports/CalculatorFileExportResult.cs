using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Features.CalculatorRun.FileExports;

public record CalculatorFileExportResult
{
    public required CalculatorRunCsvFileMetadata CsvMetadata { get; init; }
}
