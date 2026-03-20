using System.Diagnostics;
using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Interface;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EPR.Calculator.Service.Function.Services
{
    /// <summary>
    /// Inserts records to the database, divided into chunks to avoid timeouts
    /// from inserting large numbers of records at once.
    /// </summary>
    /// <typeparam name="TRecord">The type of the records to be inserted.</typeparam>
    public class DbLoadingChunkerService<TRecord> : IDbLoadingChunkerService<TRecord>
        where TRecord : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbLoadingChunkerService{TRecord}"/> class.
        /// </summary>
        /// <param name="config">The application config settings.</param>
        /// <param name="telemetryClient">A telemetry client.</param>
        /// <param name="commandTimeoutService">A service to set the database command timeout.</param>
        /// <param name="context">The database context.</param>
        [ActivatorUtilitiesConstructor]
        public DbLoadingChunkerService(
            IConfigurationService config,
            TelemetryClient telemetryClient,
            ICommandTimeoutService commandTimeoutService,
            ApplicationDBContext context)
            : this(telemetryClient, commandTimeoutService, context, config.DbLoadingChunkSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbLoadingChunkerService{TRecord}"/> class.
        /// </summary>
        /// <param name="telemetryClient">A telemetry client.</param>
        /// /// <param name="commandTimeoutService">A service to set the database command timeout.</param>
        /// <param name="context">The database context.</param>
        /// <param name="chunkSize">The number of records to include in each chunk of records.</param>
        public DbLoadingChunkerService(
            TelemetryClient telemetryClient,
            ICommandTimeoutService commandTimeoutService,
            ApplicationDBContext context,
            int chunkSize)
        {
            TelemetryClient = telemetryClient;
            Context = context;
            Table = context.Set<TRecord>();
            ChunkSize = chunkSize;

            commandTimeoutService.SetCommandTimeout(context.Database);
        }

        private int ChunkSize { get; init; }

        private DbContext Context { get; set; }

        private TelemetryClient TelemetryClient { get; init; }

        private DbSet<TRecord> Table { get; set; }

        /// <inheritdoc/>
        public async Task InsertRecords(IEnumerable<TRecord> records)
        {
            Console.WriteLine($"Loading {typeof(TRecord).Name} records in chunks of {ChunkSize}.");
            TelemetryClient.TrackTrace($"Loading {typeof(TRecord).Name} records in chunks of {ChunkSize}.");
            var chunkContents = new List<TRecord>();
            var recordCount = 1;
            var chunkCount = 1;
            var currentChunkRecordCount = 1;

            var totalTimer = new Stopwatch();
            var chunkTimer = new Stopwatch();
            totalTimer.Start();
            chunkTimer.Start();

            foreach (var record in records)
            {
                chunkContents.Add(record);
                if (chunkContents.Count >= ChunkSize)
                {
                    await SaveChunk(chunkCount, chunkTimer, chunkContents);
                    chunkContents.Clear();
                    chunkCount++;
                }

                recordCount++;
                currentChunkRecordCount++;
            }

            await SaveChunk(chunkCount, chunkTimer, chunkContents);

            totalTimer.Stop();
            Console.WriteLine($"Total time taken to insert chunks: {totalTimer.Elapsed}");
            TelemetryClient.TrackTrace($"Total time taken to insert chunks: {totalTimer.Elapsed}");
        }

        private async Task SaveChunk(int chunkCount, Stopwatch chunkTimer, IEnumerable<TRecord> chunkBuffer)
        {
            Table.AddRange(chunkBuffer);
            await Context.SaveChangesAsync();

            chunkTimer.Stop();
            Console.WriteLine($"Time to insert chunk {chunkCount}: {chunkTimer.Elapsed}");
            TelemetryClient.TrackTrace($"Time to insert chunk {chunkCount}: {chunkTimer.Elapsed}");
            chunkTimer.Restart();
        }
    }
}