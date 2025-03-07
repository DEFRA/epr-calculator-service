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

        private ApplicationDBContext Context { get; init; }

        public RunNameService(
            IConfigurationService configuration,
            IDbContextFactory<ApplicationDBContext> context)
        {
            this.configuration = configuration;
            this.Context = context.CreateDbContext();
        }

        /*public async Task<string?> GetRunNameAsync1(int runId)
        {
            string? runName = null;
            var connectionString = this.configuration.DbConnectionString;

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT RunName FROM Runs WHERE RunId = @RunId", connection))
                {
                    command.Parameters.AddWithValue("@RunId", runId);
                    runName = (string?)await command.ExecuteScalarAsync();
                }
            }

            return runName;
        }*/

        /// <summary>
        /// Gets the run name for the specified run ID.
        /// </summary>
        /// <param name="runId">The run ID.</param>
        /// <returns>The run name.</returns>
        public async Task<string?> GetRunNameAsync(int runId)
        {
            var run = await this.Context.CalculatorRuns
                .FirstOrDefaultAsync(r => r.Id == runId);

            return run?.Name;
        }
    }
}