using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Features.Calculator.FileExports;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Services.Telemetry;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Features.Calculator;

public interface ICalculatorRunFinalizer
{
    /// <summary>
    ///     Persists any required state changes to the database, then marks the calculator run as
    ///     <see cref="RunClassification.Unclassified" />.
    /// </summary>
    Task FinalizeAsCompleted(CalculatorRunContext runContext, CalcResult calcResult, CalculatorFileExportResult exportResult, CancellationToken cancellationToken);

    /// <summary>
    ///     Marks the calculator run as <see cref="RunClassification.Errored" />.
    /// </summary>
    Task FinalizeAsErrored(CalculatorRunContext runContext, CancellationToken cancellationToken);
}

[ExcludeFromCodeCoverage(Justification = "Not unit testable with in-memory database given its transactional nature")]
public class CalculatorRunFinalizer(
    ApplicationDBContext dbContext,
    IBillingInstructionService billingInstructionService,
    IProducerInvoiceNetTonnageService producerInvoiceNetTonnageService,
    ITelemetryClient telemetry,
    ILogger<CalculatorRunFinalizer> logger)
    : ICalculatorRunFinalizer
{
    public async Task FinalizeAsCompleted(CalculatorRunContext runContext, CalcResult calcResult, CalculatorFileExportResult exportResult, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await telemetry.TrackDuration("CalculatorRun.Finalizer.CreateBillingInstructions", () =>
                billingInstructionService.CreateBillingInstructions(runContext, calcResult));

            await telemetry.TrackDuration("CalculatorRun.Finalizer.CreateProducerInvoiceNetTonnage", () =>
                producerInvoiceNetTonnageService.CreateProducerInvoiceNetTonnage(runContext, calcResult));

            await telemetry.TrackDuration("CalculatorRun.Finalizer.SaveExportMetadata", () =>
                SaveExportMetadata(exportResult, cancellationToken));

            await telemetry.TrackDuration("CalculatorRun.Finalizer.SaveRunCompleted", () =>
                SaveRunCompleted(runContext, cancellationToken));

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Rolling back transaction");
            await transaction.RollbackAsync(CancellationToken.None);
            throw new RunFinalizeException(runContext.RunType, runContext.RunId, ex);
        }
    }

    public async Task FinalizeAsErrored(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        try
        {
            var calcRun = await dbContext
                .CalculatorRuns
                .SingleAsync(run => run.Id == runContext.RunId, cancellationToken);

            calcRun.Classification = RunClassification.Errored;

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to mark calculation run as failed");
        }
    }

    private async Task SaveExportMetadata(CalculatorFileExportResult exportResult, CancellationToken cancellationToken)
    {
        dbContext.CalculatorRunCsvFileMetadata.Add(exportResult.CsvMetadata);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SaveRunCompleted(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        var calcRun = await dbContext
            .CalculatorRuns
            .SingleAsync(run => run.Id == runContext.RunId, cancellationToken);

        calcRun.Classification = RunClassification.Unclassified;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
