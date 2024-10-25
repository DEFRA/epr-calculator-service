namespace EPR.Calculator.Service.Function
{
    using System;
    using EPR.Calculator.Service.Function.Constants;

    public static class Configuration
    {
        public static string PipelineUrl => Environment.GetEnvironmentVariable(Config.PipelineUrl);

        public static string GetOrgDataPipelineName => Environment.GetEnvironmentVariable(Config.GetOrgDataPipelineName);

        public static string GetPomDataPipelineName => Environment.GetEnvironmentVariable(Config.GetPomDataPipelineName);

        public static string CheckInterval => Environment.GetEnvironmentVariable(Config.CheckInterval);

        public static string MaxCheckCount => Environment.GetEnvironmentVariable(Config.MaxCheckCount);
    }
}
