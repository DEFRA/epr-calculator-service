using System.Diagnostics;
using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        /// <param name="logger">A logger instance.</param>
        /// <param name="commandTimeoutService">A service to set the database command timeout.</param>
        /// <param name="context">The database context.</param>
        [ActivatorUtilitiesConstructor]
        public DbLoadingChunkerService(
            IConfigurationService config,
            ILogger<DbLoadingChunkerService<TRecord>> logger,
            ICommandTimeoutService commandTimeoutService,
            ApplicationDBContext context)
            : this(logger, commandTimeoutService, context, config.DbLoadingChunkSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbLoadingChunkerService{TRecord}"/> class.
        /// </summary>
        /// <param name="logger">A logger instance.</param>
        /// <param name="commandTimeoutService">A service to set the database command timeout.</param>
        /// <param name="context">The database context.</param>
        /// <param name="chunkSize">The number of records to include in each chunk of records.</param>
        public DbLoadingChunkerService(
            ILogger<DbLoadingChunkerService<TRecord>> logger,
            ICommandTimeoutService commandTimeoutService,
            ApplicationDBContext context,
            int chunkSize)
        {
            Logger = logger;
            Context = context;
            Table = context.Set<TRecord>();
            ChunkSize = chunkSize;

            commandTimeoutService.SetCommandTimeout(context.Database);
        }

        private int ChunkSize { get; init; }

        private DbContext Context { get; set; }

        private ILogger<DbLoadingChunkerService<TRecord>> Logger { get; init; }

        private DbSet<TRecord> Table { get; set; }

        /// <inheritdoc/>
        public async Task InsertRecords(IEnumerable<TRecord> records)
        {
            Logger.LogTrace("Loading {RecordType} records in chunks of {ChunkSize}", typeof(TRecord).Name, ChunkSize);
            var chunkContents = new List<TRecord>();
            var chunkCount = 1;

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
            }

            await SaveChunk(chunkCount, chunkTimer, chunkContents);

            totalTimer.Stop();
            Logger.LogInformation("Total time taken to insert {RecordType} chunks: {Elapsed}", typeof(TRecord).Name, totalTimer.Elapsed.ToString("g"));
        }

        private async Task SaveChunk(int chunkCount, Stopwatch chunkTimer, IEnumerable<TRecord> chunkBuffer)
        {
            Table.AddRange(chunkBuffer);
            await Context.SaveChangesAsync();

            chunkTimer.Stop();
            Logger.LogTrace("Time to insert {RecordType} chunk {ChunkCount}: {Elapsed}", typeof(TRecord).Name, chunkCount, chunkTimer.Elapsed.ToString("g"));
            chunkTimer.Restart();
        }
    }
}