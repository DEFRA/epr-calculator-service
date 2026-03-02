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

#pragma warning disable CS8603 // Possible null reference return.
        /// <summary>
        /// Gets the status update endpoint URI from environment variables.
        /// </summary>
        public Uri StatusEndpoint => new(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.StatusUpdateEndpoint)!);

        /// <summary>
        /// Gets the URI for the endpoint used to prepare calculation results from environment variables.
        /// </summary>
        public Uri PrepareCalcResultEndPoint => new(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.PrepareCalcResultEndPoint)!);

        /// <summary>
        /// Gets the URI for the endpoint used to Transpose before calculation of results from environment variables.
        /// </summary>
        public Uri TransposeEndpoint => new(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.TransposeEndpoint)!);

        /// <summary>
        /// Gets the RPD status timeout from environment variables.
        /// </summary>
        public TimeSpan RpdStatusTimeout => ParseTimeSpan(
            Environment.GetEnvironmentVariable(EnvironmentVariableKeys.RpdStatusTimeout)!);

        /// <summary>
        /// Gets the calculator run timeout from environment variables.
        /// </summary>
        public TimeSpan PrepareCalcResultsTimeout => ParseTimeSpan(
            Environment.GetEnvironmentVariable(EnvironmentVariableKeys.PrepareCalcResultsTimeout)!);

        /// <summary>
        /// Gets the transpose timeout from environment variables.
        /// </summary>
        public TimeSpan TransposeTimeout => ParseTimeSpan(
            Environment.GetEnvironmentVariable(EnvironmentVariableKeys.TransposeTimeout)!);

        /// <summary>
        /// Gets the database connection string from environment variables.
        /// </summary>
        public string DbConnectionString => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.DbConnectionString);

        /// <summary>
        /// Gets the blob container name from environment variables.
        /// </summary>
        public string ResultFileCSVContainerName => Environment
            .GetEnvironmentVariable(EnvironmentVariableKeys.ResultFileCSVContainerName);

        public string BillingFileCSVBlobContainerName => Environment
            .GetEnvironmentVariable(EnvironmentVariableKeys.BillingFileCSVBlobContainerName);

        public string BillingFileJsonBlobContainerName => Environment
            .GetEnvironmentVariable(EnvironmentVariableKeys.BillingFileJsonBlobContainerName);

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
            Environment.GetEnvironmentVariable(EnvironmentVariableKeys.CommandTimeout)!);

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