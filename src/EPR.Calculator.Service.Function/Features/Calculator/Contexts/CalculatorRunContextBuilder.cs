using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Messaging;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Features.Calculator.Contexts;

/// <summary>
///     Creates <see cref="CalculatorRunContext">CalculatorRunContexts</see> for use with the
///     <see cref="CalculatorRunProcessor" />.
/// </summary>
public interface ICalculatorRunContextBuilder
{
    /// <summary>
    ///     Creates a valid <see cref="CalculatorRunContext" /> for the specified <see cref="CreateResultFileMessage" />.
    /// </summary>
    /// <exception cref="RunContextException">
    ///     If the underlying <see cref="CalculatorRun" /> is not in a valid state for calculator run processing.
    /// </exception>
    Task<CalculatorRunContext> CreateContext(CreateResultFileMessage message, CancellationToken cancellationToken);
}

/// <inheritdoc />
public class CalculatorRunContextBuilder(
    ApplicationDBContext dbContext,
    TimeProvider timeProvider)
    : ICalculatorRunContextBuilder
{
    /// <inheritdoc />
    public async Task<CalculatorRunContext> CreateContext(CreateResultFileMessage message, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var run = await GetCalculatorRunAsync(message.CalculatorRunId, cancellationToken);

        return new CalculatorRunContext
        {
            RunId = run.Id,
            RunName = run.Name.Trim(),
            ProcessingStartedAt = now,
            RelativeYear = run.RelativeYear,
            User = message.CreatedBy
        };
    }

    private async Task<CalculatorRun> GetCalculatorRunAsync(int runId, CancellationToken ct)
    {
        var run = await dbContext
            .CalculatorRuns
            .AsNoTracking()
            .SingleOrDefaultAsync(r => r.Id == runId, ct);

        if (run == null)
        {
            throw new RunContextException(RunType.Calculator, runId, "Run not found");
        }

        var validationResult = await new CalculatorRunValidator().ValidateAsync(run, ct);

        if (!validationResult.IsValid)
        {
            throw new RunContextException(RunType.Calculator, runId, validationResult.ToString("|"));
        }

        return run;
    }
}