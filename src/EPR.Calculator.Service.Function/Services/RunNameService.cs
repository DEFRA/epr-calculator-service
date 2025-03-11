namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.Service.Function.Data;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Service to fetch the run name from the database.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="RunNameService"/> class.
    /// </remarks>
    /// <param name="configuration">The configuration object.</param>
    /// <param name="context">The context object.</param>
    /// <param name="telemetryLogger">The telemetry logger.</param>
    public class RunNameService(
        IDbContextFactory<ApplicationDBContext> context,
        ICalculatorTelemetryLogger telemetryLogger) : IRunNameService
    {
        private readonly ICalculatorTelemetryLogger telemetryLogger = telemetryLogger;

        private ApplicationDBContext Context { get; init; } = context.CreateDbContext();

        /// <summary>
        /// Gets the run name for the specified run ID.
        /// </summary>
        /// <param name="runId">The run ID.</param>
        /// <returns>The run name.</returns>
        public async Task<string?> GetRunNameAsync(int runId)
        {
            try
            {
                var run = await this.Context.CalculatorRuns
                    .SingleOrDefaultAsync(r => r.Id == runId);

                return run?.Name;
            }
            catch (Exception ex)
            {
                this.telemetryLogger.LogError(new ErrorMessage { Message = "Error fetching run name", Exception = ex });
                return null;
            }
        }
    }
}