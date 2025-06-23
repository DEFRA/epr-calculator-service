// <copyright file="Configuration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function
{
    using System;
    using System.Globalization;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.EntityFrameworkCore.Diagnostics;

    /// <summary>
    /// Provides configuration settings for the calculator service.
    /// </summary>
    public class Configuration : IConfigurationService
    {
        /// <summary>
        /// The default value for calculator run and transpose timeouts.
        /// </summary>
        public const double DefaultTimeout = 24;

        public const int DefaultCheckInterval = 5;

        public const int DefaultMaxCheckCount = 10;

        /// <summary>
        /// Gets the pipeline URL from environment variables.
        /// </summary>
#pragma warning disable CS8603 // Possible null reference return.
        public string PipelineUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.PipelineUrl);

        /// <summary>
        /// Gets the organization data pipeline name from environment variables.
        /// </summary>
        public string OrgDataPipelineName => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.OrgDataPipelineName);

        /// <summary>
        /// Gets the POM data pipeline name from environment variables.
        /// </summary>
        public string PomDataPipelineName => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.PomDataPipelineName);

        /// <summary>
        /// Gets the check interval from environment variables.
        /// </summary>
        public int CheckInterval
        {
            get
            {
                var parseSuccess = int.TryParse(
                    Environment.GetEnvironmentVariable(EnvironmentVariableKeys.CheckInterval),
                    out int value);
                return parseSuccess ? value : DefaultCheckInterval;
            }
        }

        /// <summary>
        /// Gets the maximum check count from environment variables.
        /// </summary>
        public int MaxCheckCount
        {
            get
            {
                var parseSuccess = int.TryParse(
                    Environment.GetEnvironmentVariable(EnvironmentVariableKeys.MaxCheckCount),
                    out int value);
                return parseSuccess ? value : DefaultMaxCheckCount;
            }
        }

        /// <summary>
        /// Gets the status update endpoint URI from environment variables.
        /// </summary>
        public Uri StatusEndpoint => new Uri(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.StatusUpdateEndpoint));

        /// <summary>
        /// Gets the URI for the endpoint used to prepare calculation results from environment variables.
        /// </summary>
        public Uri PrepareCalcResultEndPoint => new Uri(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.PrepareCalcResultEndPoint));

        /// <summary>
        /// Gets the URI for the endpoint used to Transpose before calculation of results from environment variables.
        /// </summary>
        public Uri TransposeEndpoint => new Uri(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.TransposeEndpoint));

        /// <summary>
        /// Gets a value indicating whether to execute the RPD pipeline from environment variables.
        /// </summary>
        public bool ExecuteRPDPipeline
        {
            get
            {
                bool.TryParse(
                    Environment.GetEnvironmentVariable(EnvironmentVariableKeys.ExecuteRPDPipeline),
                    out bool value);
                return value;
            }
        }

        /// <summary>
        /// Gets the RPD status timeout from environment variables.
        /// </summary>
        public TimeSpan RpdStatusTimeout => ParseTimeSpan(
            Environment.GetEnvironmentVariable(EnvironmentVariableKeys.RpdStatusTimeout));

        /// <summary>
        /// Gets the calculator run timeout from environment variables.
        /// </summary>
        public TimeSpan PrepareCalcResultsTimeout => ParseTimeSpan(
            Environment.GetEnvironmentVariable(EnvironmentVariableKeys.PrepareCalcResultsTimeout));

        /// <summary>
        /// Gets the transpose timeout from environment variables.
        /// </summary>
        public TimeSpan TransposeTimeout => ParseTimeSpan(
            Environment.GetEnvironmentVariable(EnvironmentVariableKeys.TransposeTimeout));

        /// <summary>
        /// Gets the database connection string from environment variables.
        /// </summary>
        public string DbConnectionString => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.DbConnectionString);

        /// <summary>
        /// Gets the blob container name from environment variables.
        /// </summary>
        public string CalcResultBlobContainerName => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.CalcResultBlobContainerName);

        /// <summary>
        /// Gets the blob connection string from environment variables.
        /// </summary>
        public string BlobConnectionString => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.BlobConnectionString);

        /// <summary>
        /// Gets the instrumentation key from environment variables.
        /// </summary>
        public string InstrumentationKey => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.InstrumentationKey);

        /// <summary>
        /// Gets the command timeout from environment variables.
        /// </summary>
        public TimeSpan CommandTimeout => ParseTimeSpan(
            Environment.GetEnvironmentVariable(EnvironmentVariableKeys.CommandTimeout));

        public int DbLoadingChunkSize => int.Parse(
            Environment.GetEnvironmentVariable(EnvironmentVariableKeys.DbLoadingChunkSize) ?? "1000");

#pragma warning restore CS8603 // Possible null reference return.

        /// <summary>
        /// Parses a string value to a TimeSpan.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <returns>A TimeSpan parsed from the string value.</returns>
        private static TimeSpan ParseTimeSpan(string value)
        {
            if (double.TryParse(value, out double timeout))
            {
                return TimeSpan.FromMinutes(timeout);
            }

            return TimeSpan.FromHours(DefaultTimeout);
        }
    }
}
