using EPR.Calculator.Service.Function.Features.CalculatorRun.FileExports;
using EPR.Calculator.Service.Function.Features.Common;

namespace EPR.Calculator.Service.Function.Features.CalculatorRun;

public sealed record CalculatorRunResult : RunResult
{
    public override bool Succeeded => true;
    public required CalculatorFileExportResult ExportResult { get; init; }
}
