namespace EPR.Calculator.Service.Function.Misc
{
    using System;
    using System.Configuration;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.Extensions.Configuration;

#pragma warning disable S1075
    public class LocalDevelopmentConfiguration : IConfigurationService
    {
        public LocalDevelopmentConfiguration(IConfiguration configuration) => this.Configuration = configuration;

        public int CheckInterval => this.Configuration.GetValue(nameof(this.CheckInterval), 5);

        public string DbConnectionString
            => Configuration.GetValue("DbConnectionString", string.Empty);

        public bool ExecuteRPDPipeline => this.Configuration.GetValue(nameof(this.ExecuteRPDPipeline), false);

        public int MaxCheckCount => this.Configuration.GetValue(nameof(this.MaxCheckCount), 10);

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
        public string ResultFileCSVContainerName => Configuration.GetValue("ResultFileCSVContainerName", string.Empty);

        /// <inheritdoc/>
        public string BillingFileCSVBlobContainerName => Configuration.GetValue("BillingFileCSVBlobContainerName", string.Empty);

        /// <inheritdoc/>
        public string BillingFileJsonBlobContainerName => Configuration.GetValue("BillingFileJsonBlobContainerName", string.Empty);

        public string BlobConnectionString
            => Configuration.GetValue("BlobConnectionString", string.Empty);

        /// <summary>
        /// Gets the configuration file that's passed using dependancy injection.
        /// </summary>
        private IConfiguration Configuration { get; }

        public string InstrumentationKey => Configuration.GetValue("InstrumentationKey", string.Empty);

        public TimeSpan CommandTimeout
        {
            get
            {
                double.TryParse(
                    this.Configuration.GetValue("CommandTimeout", string.Empty),
                    out double timeoutInMinutes);
                return TimeSpan.FromMinutes(timeoutInMinutes);
            }
        }

        public int DbLoadingChunkSize
            => this.Configuration.GetValue(EnvironmentVariableKeys.DbLoadingChunkSize, 1000);
    }
#pragma warning restore S1075
}
