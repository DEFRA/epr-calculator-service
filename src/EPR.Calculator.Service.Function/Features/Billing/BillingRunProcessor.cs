using System.Diagnostics;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Billing.FileExports;
using EPR.Calculator.Service.Function.Services.Telemetry;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.Features.Billing;

public interface IBillingRunProcessor
{
    Task<bool> Process(BillingRunContext runContext, CancellationToken cancellationToken);
}

public class BillingRunProcessor(
    ICalcResultBuilder resultBuilder,
    IBillingFileExporter fileExporter,
    IBillingRunFinalizer finalizer,
    ITelemetryClient telemetryClient,
    ILogger<BillingRunProcessor> logger)
    : IBillingRunProcessor
{
    public async Task<bool> Process(BillingRunContext runContext, CancellationToken cancellationToken)
    {
        try
        {
            // This reads the required data to memory and builds the CalcResult object.
            // For BillingRunContext, it does not cause any external state mutations.
            var sw = Stopwatch.StartNew();
            var calcResult = await resultBuilder.BuildAsync(runContext);
            logger.LogDebug($"{nameof(resultBuilder.BuildAsync)} Completed. Elapsed:{{Elapsed}}", sw.Elapsed);

            // This writes the CSV/JSON files to blob storage.
            // It does not mutate the database state (handled in the finalizer).
            sw.Restart();
            var exportResult = await fileExporter.Export(runContext, calcResult, cancellationToken);
            logger.LogDebug($"{nameof(fileExporter.Export)} Completed. Elapsed:{{Elapsed}}", sw.Elapsed);

            // This mutates the state of various database entities to reflect the completed run.
            sw.Restart();
            await finalizer.FinalizeAsCompleted(runContext, calcResult, exportResult, cancellationToken);
            logger.LogDebug($"{nameof(finalizer.FinalizeAsCompleted)} Completed. Elapsed:{{Elapsed}}", sw.Elapsed);

            return true;
        }
        catch (Exception ex)
        {
            // ⚠️ For billing run exceptions, the database state should NOT have mutated, except for files
            // written to blob storage (which will become orphaned).
            // It should be safe to retry in this scenario.
            var type = ex is OperationCanceledException ? "Timeout" : "Exception";
            logger.LogError(ex, "Billing run failed due to {ExceptionType}", type);
            telemetryClient.Track(TelemetryEvents.ErrorException(ex, runContext));
            await finalizer.FinalizeAsErrored(runContext, cancellationToken);
            return false;
        }
    }
}