namespace EPR.Calculator.Service.Function.Interface
{
    using EPR.Calculator.Service.Function.Constants;
    using System;

    public interface IConfigurationService
    {
        int CheckInterval { get; }

        string DbConnectionString { get; }

        bool ExecuteRPDPipeline { get; }

        int MaxCheckCount { get; }

        string OrgDataPipelineName { get; }

        string PipelineUrl { get; }

        string PomDataPipelineName { get; }

        Uri PrepareCalcResultEndPoint { get; }

        TimeSpan PrepareCalcResultsTimeout { get; }

        TimeSpan RpdStatusTimeout { get; }

        Uri StatusEndpoint { get; }

        Uri TransposeEndpoint { get; }

        TimeSpan TransposeTimeout { get; }

        string ResultFileCSVContainerName { get; }

        string BlobConnectionString { get; }

        string InstrumentationKey { get; }

        /// <summary>
        /// Gets the database command timout from the envirionment variables.
        /// </summary>
        TimeSpan CommandTimeout { get; }

        int DbLoadingChunkSize { get; }

        string BillingFileCSVBlobContainerName { get; }

        string BillingFileJsonBlobContainerName { get; }
    }
}