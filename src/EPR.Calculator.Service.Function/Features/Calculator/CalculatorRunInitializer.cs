using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Services.ExternalDataLoading;
using EPR.Calculator.Service.Function.Services.Telemetry;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Features.Calculator;

public interface ICalculatorRunInitializer
{
    Task Initialize(CalculatorRunContext runContext, CancellationToken cancellationToken);
}

public class CalculatorRunInitializer(
    ApplicationDBContext dbContext,
    IExternalDataLoader externalDataLoader,
    ICalculatorRunOrgData calculatorRunOrgData,
    ICalculatorRunPomData calculatorRunPomData,
    ITransposePomAndOrgDataService transposer,
    ITelemetryClient telemetry)
    : ICalculatorRunInitializer
{
    public async Task Initialize(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        await SetRunAsRunning(runContext, cancellationToken);

        await telemetry.TrackDuration("CalculatorRun.Initializer.InitialDataLoad", () =>
            externalDataLoader.LoadOrgAndPomData(runContext, cancellationToken));

        await telemetry.TrackDuration("CalculatorRun.Initializer.LoadOrgData", () =>
            calculatorRunOrgData.LoadOrgDataForCalcRun(runContext.RunId, runContext.RelativeYear, runContext.User, cancellationToken));

        await telemetry.TrackDuration("CalculatorRun.Initializer.LoadPomData", () =>
            calculatorRunPomData.LoadPomDataForCalcRun(runContext.RunId, runContext.RelativeYear, runContext.User, cancellationToken));

        await telemetry.TrackDuration("CalculatorRun.Initializer.TransposeData", () =>
            transposer.Transpose(runContext, cancellationToken));
    }

    private async Task SetRunAsRunning(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        var calcRun = await dbContext
            .CalculatorRuns
            .SingleAsync(run => run.Id == runContext.RunId, cancellationToken);

        calcRun.Classification = RunClassification.Running;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
