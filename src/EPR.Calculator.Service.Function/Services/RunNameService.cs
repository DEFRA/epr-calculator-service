namespace EPR.Calculator.Service.Function.Services
{
    using System.Threading.Tasks;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Service to fetch the run name from the database.
    /// </summary>
    public class RunNameService : IRunNameService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RunNameService"/> class.
        /// </summary>
        /// <param name="context">The context object.</param>
        public RunNameService(IDbContextFactory<ApplicationDBContext> context)
        {
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
                .SingleOrDefaultAsync(r => r.Id == runId);

            return run?.Name;
        }
    }
}