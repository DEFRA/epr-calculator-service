namespace EPR.Calculator.Service.Function.Services
{
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Function.Data;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Service to fetch the run name from the database.
    /// </summary>
    public class RunNameService : IRunNameService
    {
        private readonly IConfigurationService configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RunNameService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration service.</param>
        /// <param name="context">The DB context.</param>
        public RunNameService(
            IConfigurationService configuration,
            IDbContextFactory<ApplicationDBContext> context)
        {
            this.configuration = configuration;
            this.Context = context.CreateDbContext();
        }

        private ApplicationDBContext Context { get; init; }

        /// <summary>
        /// Gets the run name for the specified run ID.
        /// </summary>
        /// <param name="runId">The run ID.</param>
        /// <returns>The run name.</returns>
        public async Task<string?> GetRunNameAsync(int runId)
        {
            var run = await this.Context.CalculatorRuns
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == runId);

            return run?.Name;
        }
    }
}