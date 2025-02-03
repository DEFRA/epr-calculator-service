namespace EPR.Calculator.Service.Function.Misc
{
    using System;
    using EPR.Calculator.Service.Function.Interface;

    public class LocalDevelopmentConfiguration : IConfigurationService
    {
        public string CheckInterval => string.Empty;

        public string DbConnectionString => System.Configuration.ConfigurationManager.AppSettings["DbConnectionString"]
            ?? string.Empty;

        public string ExecuteRPDPipeline => string.Empty;

        public string MaxCheckCount => string.Empty;

        public string OrgDataPipelineName => string.Empty;

        public string PipelineUrl => string.Empty;

        public string PomDataPipelineName => string.Empty;

        public Uri PrepareCalcResultEndPoint => new Uri("http://localhost:5055/v1/internal/prepareCalcResults");

        public TimeSpan PrepareCalcResultsTimeout => TimeSpan.FromDays(1);

        public TimeSpan RpdStatusTimeout => TimeSpan.FromDays(1);

        public Uri StatusEndpoint => new Uri("http://localhost:5055/v1/internal/rpdStatus");

        public Uri TransposeEndpoint => new Uri("http://localhost/v1/internal/transposeBeforeCalcResults");

        public TimeSpan TransposeTimeout => TimeSpan.FromDays(1);

        /// <inheritdoc/>
        public string BlobContainerName => "TestBlobContainer";

        public string BlobConnectionString
            => System.Configuration.ConfigurationManager.AppSettings["BlobStorage:ConnectionString"]
            ?? string.Empty;
    }
}
