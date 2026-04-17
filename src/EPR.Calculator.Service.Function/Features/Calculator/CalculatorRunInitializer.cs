using System.Diagnostics;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Services.DataLoading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.Features.Calculator;

public interface ICalculatorRunInitializer
{
    Task Initialize(CalculatorRunContext runContext, CancellationToken cancellationToken);
}

public class CalculatorRunInitializer(
    ApplicationDBContext dbContext,
    IDataLoader dataLoader,
    ICalculatorRunOrgData calculatorRunOrgData,
    ICalculatorRunPomData calculatorRunPomData,
    ITransposePomAndOrgDataService transposer,
    ILogger<ICalculatorRunInitializer> logger)
    : ICalculatorRunInitializer
{
    public async Task Initialize(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        await SaveRunRunning(runContext, cancellationToken);
        await dataLoader.LoadData(runContext, cancellationToken);
        await TransposeStagedData(runContext, cancellationToken);
    }

    private async Task SaveRunRunning(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        var calcRun = await dbContext
            .CalculatorRuns
            .SingleAsync(run => run.Id == runContext.RunId, cancellationToken);

        calcRun.CalculatorRunClassificationId = RunClassificationStatusIds.RUNNINGID;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task TransposeStagedData(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var sw = Stopwatch.StartNew();
            await calculatorRunOrgData.LoadOrgDataForCalcRun(runContext.RunId, runContext.RelativeYear, runContext.User, cancellationToken);
            logger.LogDebug($"{nameof(calculatorRunOrgData.LoadOrgDataForCalcRun)} Completed. Elapsed:{{Elapsed}}", sw.Elapsed);

            sw.Restart();
            await calculatorRunPomData.LoadPomDataForCalcRun(runContext.RunId, runContext.RelativeYear, runContext.User, cancellationToken);
            logger.LogDebug($"{nameof(calculatorRunPomData.LoadPomDataForCalcRun)} Completed. Elapsed:{{Elapsed}}", sw.Elapsed);

            sw.Restart();
            await transposer.Transpose(runContext, cancellationToken);
            logger.LogDebug($"{nameof(transposer.Transpose)} Completed. Elapsed:{{Elapsed}}", sw.Elapsed);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}