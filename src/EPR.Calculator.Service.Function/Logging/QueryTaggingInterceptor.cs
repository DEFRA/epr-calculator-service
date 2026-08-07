using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.Service.Function.Logging;

/// <summary>
///     EF Core command interceptor that tags every command not already tagged via
///     <c>TagWith</c> with its calling class/method, so the SQL text is traceable
///     back to code from Query Store or Extended Events.
/// </summary>
[ExcludeFromCodeCoverage]
public class QueryTaggingInterceptor : DbCommandInterceptor
{
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        TagCommand(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        TagCommand(command);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        TagCommand(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    private static void TagCommand(DbCommand command)
    {
        if (command.CommandText.StartsWith("--", StringComparison.Ordinal))
            return;

        var callSite = FindCallSite();
        if (callSite is not null)
            command.CommandText = $"-- {callSite}\n{command.CommandText}";
    }

    // Matches the identifier inside the first "<...>" of any compiler-generated async
    // state machine name: "<Method>d__3", "<Method>d__3`1" (generic), "<<Method>b__0>d" (lambda).
    private static readonly Regex AsyncStateMachineName = new(@"^<+([A-Za-z_][A-Za-z0-9_]*)>", RegexOptions.Compiled);

    // Generic materialization/utility helpers whose own name is never a useful call site -
    // walk past these to find the actual caller instead of stopping here.
    private static readonly string[] SkippedNamespaces =
    [
        "Microsoft.EntityFrameworkCore",
        "System",
        typeof(QueryTaggingInterceptor).Namespace!,
        typeof(EPR.Calculator.Service.Function.Utils.QueryableToImmutableExtensions).Namespace!,
    ];

    private static string? FindCallSite()
    {
        var stackTrace = new StackTrace(fNeedFileInfo: false);
        foreach (var frame in stackTrace.GetFrames())
        {
            var method = frame.GetMethod();
            var type = method?.DeclaringType;
            var ns = type?.Namespace;

            if (ns is null || SkippedNamespaces.Any(skipped => ns.StartsWith(skipped, StringComparison.Ordinal)))
                continue;

            var methodName = method!.Name;

            // An async lambda capturing variables nests as: real class -> closure display
            // class -> async state machine. Walk up through both generated layers to reach
            // the real class, capturing the method name the first time we see it.
            while (true)
            {
                var match = AsyncStateMachineName.Match(type!.Name);
                if (match.Success)
                {
                    methodName = match.Groups[1].Value;
                    if (type.DeclaringType is { } asyncOwner)
                    {
                        type = asyncOwner;
                        continue;
                    }

                    break;
                }

                if (type.Name.StartsWith("<>c", StringComparison.Ordinal) && type.DeclaringType is { } closureOwner)
                {
                    type = closureOwner;
                    continue;
                }

                break;
            }

            return $"{type.Name}.{methodName}";
        }

        return null;
    }
}
