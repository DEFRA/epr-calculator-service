namespace EPR.Calculator.Service.Function.Interface
{
    using System;

    public interface IConfigurationService
    {
        string CheckInterval { get; }

        string DbConnectionString { get; }

        string ExecuteRPDPipeline { get; }

        string MaxCheckCount { get; }

        string OrgDataPipelineName { get; }

        string PipelineUrl { get; }

        string PomDataPipelineName { get; }

        Uri PrepareCalcResultEndPoint { get; }

        TimeSpan PrepareCalcResultsTimeout { get; }

        TimeSpan RpdStatusTimeout { get; }

        Uri StatusEndpoint { get; }

        Uri TransposeEndpoint { get; }

        TimeSpan TransposeTimeout { get; }

        string BlobContainerName{ get; }
    }
}