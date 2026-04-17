using System.Diagnostics;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Features.Calculator.FileExports;
using EPR.Calculator.Service.Function.Services.Telemetry;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.Features.Calculator;

public interface ICalculatorRunProcessor
{
    Task<bool> Process(CalculatorRunContext runContext, CancellationToken cancellationToken);
}

public class CalculatorRunProcessor(
    ICalculatorRunInitializer initializer,
    ICalculatorRunFinalizer finalizer,
    ICalcResultBuilder resultBuilder,
    ICalculatorFileExporter fileExporter,
    ITelemetryClient telemetryClient,
    ILogger<CalculatorRunProcessor> logger)
    : ICalculatorRunProcessor
{
    public async Task<bool> Process(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        try
        {
            // ⚠️ Database mutations can happen throughout many of these calls
            // This behaviour is different to BillingRunProcessor (which only does so in the finalizer).
            var sw = Stopwatch.StartNew();
            await initializer.Initialize(runContext, cancellationToken);
            logger.LogDebug($"{nameof(initializer.Initialize)} Completed. Elapsed:{{Elapsed}}", sw.Elapsed);

            // This reads the required data to memory and builds the CalcResult object.
            // For CalculatorRunContext, it causes external state mutations.
            sw.Restart();
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
            // ⚠️ For calculator run exceptions, the database state may have mutated.
            // It IS NOT safe to retry in this scenario.
            var type = ex is OperationCanceledException ? "Timeout" : "Exception";
            logger.LogError(ex, "Calculator run failed due to {ExceptionType}", type);
            telemetryClient.Track(TelemetryEvents.ErrorException(ex, runContext));
            await finalizer.FinalizeAsFailed(runContext, cancellationToken);
            return false;
        }
    }
}