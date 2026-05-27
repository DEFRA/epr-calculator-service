using EPR.Calculator.Service.Function.Features.BillingRun.FileExports;
using EPR.Calculator.Service.Function.Features.Common;

namespace EPR.Calculator.Service.Function.Features.BillingRun;

public sealed record BillingRunResult : RunResult
{
    public override bool Succeeded => true;
    public required BillingFileExportResult ExportResult { get; init; }
}
