using EPR.Calculator.Service.Function.Features.Common;

namespace EPR.Calculator.Service.Function.Logging;

public static class LoggerScopeExtensions
{
    public static IDisposable? BeginScopeWithContext(this ILogger logger, RunContext runContext)
    {
        return logger.BeginScope(new Dictionary<string, object>
        {
            { "RunType", runContext.RunType.ToString() },
            { "RunId", runContext.RunId },
            { "RunName", runContext.RunName }
        });
    }
}
