using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Features.Calculator.FileExports;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.Features.Calculator;

public interface ICalculatorRunFinalizer
{
    /// <summary>
    ///     Persists any required state changes to the database, then marks the calculator run as completed.
    /// </summary>
    Task FinalizeAsCompleted(CalculatorRunContext runContext, CalcResult calcResult, CalculatorFileExportResult exportResult, CancellationToken cancellationToken);

    /// <summary>
    ///     Marks the calculator run as failed.
    /// </summary>
    Task FinalizeAsFailed(CalculatorRunContext runContext, CancellationToken cancellationToken);
}

/// <inheritdoc />
[ExcludeFromCodeCoverage(Justification = "Not unit testable with in-memory database given its transactional nature")]
public class CalculatorRunFinalizer(
    ApplicationDBContext dbContext,
    IBillingInstructionService billingInstructionService,
    IProducerInvoiceNetTonnageService producerInvoiceNetTonnageService,
    ILogger<CalculatorRunFinalizer> logger)
    : ICalculatorRunFinalizer
{
    /// <inheritdoc />
    public async Task FinalizeAsCompleted(CalculatorRunContext runContext, CalcResult calcResult, CalculatorFileExportResult exportResult, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await SaveProducerData(runContext, calcResult);
            await SaveExportMetadata(exportResult, cancellationToken);
            await SaveRunCompleted(runContext, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Rolling back transaction");
            await transaction.RollbackAsync(CancellationToken.None);
            throw new RunFinalizeException(runContext.RunType, runContext.RunId, ex);
        }
    }

    /// <inheritdoc />
    public async Task FinalizeAsFailed(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        try
        {
            var calcRun = await dbContext
                .CalculatorRuns
                .SingleAsync(run => run.Id == runContext.RunId, cancellationToken);

            calcRun.CalculatorRunClassificationId = RunClassificationStatusIds.ERRORID;

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to mark billing run as failed");
        }
    }

    private async Task SaveExportMetadata(CalculatorFileExportResult exportResult, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        dbContext.CalculatorRunCsvFileMetadata.Add(exportResult.CsvMetadata);

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogDebug($"{nameof(SaveExportMetadata)} Completed. Elapsed:{{Elapsed}}", sw.Elapsed);
    }

    private async Task SaveProducerData(CalculatorRunContext runContext, CalcResult calcResult)
    {
        var sw = Stopwatch.StartNew();

        await billingInstructionService.CreateBillingInstructions(runContext, calcResult);
        logger.LogDebug($"{nameof(billingInstructionService.CreateBillingInstructions)} Completed. Elapsed:{{Elapsed}}", sw.Elapsed);

        await producerInvoiceNetTonnageService.CreateProducerInvoiceNetTonnage(runContext, calcResult);
        logger.LogDebug($"{nameof(producerInvoiceNetTonnageService.CreateProducerInvoiceNetTonnage)} Completed. Elapsed:{{Elapsed}}", sw.Elapsed);
    }

    private async Task SaveRunCompleted(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        var calcRun = await dbContext
            .CalculatorRuns
            .SingleAsync(run => run.Id == runContext.RunId, cancellationToken);

        // todo: 😒 set billing status
        calcRun.CalculatorRunClassificationId = RunClassificationStatusIds.UNCLASSIFIEDID;

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogDebug($"{nameof(SaveRunCompleted)} Completed. Elapsed:{{Elapsed}}", sw.Elapsed);
    }
}