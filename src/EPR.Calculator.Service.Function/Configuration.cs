// <copyright file="Configuration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function
{
    using System;
    using EPR.Calculator.Service.Function.Constants;

    /// <summary>
    /// Provides configuration settings for the calculator service.
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// Gets the pipeline URL from environment variables.
        /// </summary>
        public static string PipelineUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.PipelineUrl);

        /// <summary>
        /// Gets the organization data pipeline name from environment variables.
        /// </summary>
        public static string OrgDataPipelineName => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.OrgDataPipelineName);

        /// <summary>
        /// Gets the POM data pipeline name from environment variables.
        /// </summary>
        public static string PomDataPipelineName => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.PomDataPipelineName);

        /// <summary>
        /// Gets the check interval from environment variables.
        /// </summary>
        public static string CheckInterval => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.CheckInterval);

        /// <summary>
        /// Gets the maximum check count from environment variables.
        /// </summary>
        public static string MaxCheckCount => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.MaxCheckCount);

        /// <summary>
        /// Gets the status update endpoint URI from environment variables.
        /// </summary>
        public static Uri StatusEndpoint => new Uri(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.StatusUpdateEndpoint));

        /// <summary>
        /// Gets the URI for the endpoint used to prepare calculation results from environment variables.
        /// </summary>
        public static Uri PrepareCalcResultEndPoint => new Uri(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.PrepareCalcResultEndPoint));


        /// <summary>
        /// Gets the flag indicating whether to execute the RPD pipeline from environment variables.
        /// </summary>
        public static string ExecuteRPDPipeline => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.ExecuteRPDPipeline);
    }
}
