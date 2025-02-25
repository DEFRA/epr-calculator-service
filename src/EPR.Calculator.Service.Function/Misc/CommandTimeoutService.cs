namespace EPR.Calculator.Service.Function.Services
{
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    /// <summary>
    /// Service to set the command timeout for the database.
    /// </summary>
    /// <remarks>
    /// Setting the timeout isn't possible when testing using an in-memory database, so this service
    /// isolates the setting of the timeout from other services, allowing them to be more easily tested.
    /// </remarks>
    public class CommandTimeoutService : ICommandTimeoutService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandTimeoutService"/> class.
        /// </summary>
        /// <param name="configuration">
        /// An <see cref="IConfigurationService"/> providing access to the app's environment variables.
        /// </param>
        public CommandTimeoutService(IConfigurationService configuration)
            => this.Configuration = configuration;

        private IConfigurationService Configuration { get; init; }

        /// <inheritdoc/>
        public void SetCommandTimeout(DatabaseFacade database)
            => database.SetCommandTimeout(this.Configuration.CommandTimeout);
    }
}
