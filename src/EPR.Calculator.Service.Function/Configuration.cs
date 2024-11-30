// <copyright file="Configuration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function
{
    using System;
    using EPR.Calculator.Service.Common.AzureSynapse;
    using EPR.Calculator.Service.Function.Constants;

    public static class Configuration
    {
        public static string PipelineUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.PipelineUrl);

        public static string OrgDataPipelineName => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.OrgDataPipelineName);

        public static string PomDataPipelineName => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.PomDataPipelineName);

        public static string CheckInterval => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.CheckInterval);

        public static string MaxCheckCount => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.MaxCheckCount);

        public static Uri StatusEndpoint
            => new Uri(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.StatusUpdateEndpoint));

        public static string ExecuteRPDPipeline => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.ExecuteRPDPipeline);
    }
}
