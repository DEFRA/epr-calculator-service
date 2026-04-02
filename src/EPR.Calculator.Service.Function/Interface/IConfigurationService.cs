namespace EPR.Calculator.Service.Function.Interface
{
    public interface IConfigurationService
    {
        string DbConnectionString { get; }

        TimeSpan PrepareCalcResultsTimeout { get; }

        TimeSpan RpdStatusTimeout { get; }

        TimeSpan TransposeTimeout { get; }

        string ResultFileCSVContainerName { get; }

        string BlobConnectionString { get; }

        /// <summary>
        /// Gets the database command timout from the envirionment variables.
        /// </summary>
        TimeSpan CommandTimeout { get; }

        int DbLoadingChunkSize { get; }

        string BillingFileCSVBlobContainerName { get; }

        string BillingFileJsonBlobContainerName { get; }
    }
}