﻿// <copyright file="EnvironmentVariableKeys.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace EPR.Calculator.Service.Function.Constants
{
    /// <summary>
    /// Contains keys for environment variables used in the calculator service.
    /// </summary>
    public static class EnvironmentVariableKeys
    {
        /// <summary>The URL of the synapse pipeline.</summary>
        public const string PipelineUrl = "PipelineUrl";

        /// <summary>The name of the organization data pipeline.</summary>
        public const string OrgDataPipelineName = "GetOrgDataPipelineName";

        /// <summary>The name of the POM data pipeline.</summary>
        public const string PomDataPipelineName = "GetPomDataPipelineName";

        /// <summary>The interval for checking the pipeline status.</summary>
        public const string CheckInterval = "CheckInterval";

        /// <summary>The maximum number of checks to perform.</summary>
        public const string MaxCheckCount = "MaxCheckCount";

        /// <summary>The URL of the endpoint called to update the pipeline status in the database.</summary>
        public const string StatusUpdateEndpoint = "StatusUpdateEndpoint";

        /// <summary>The URL of the endpoint called to prepare the result file in blob storage.</summary>
        public const string PrepareCalcResultEndPoint = "PrepareCalcResultEndPoint";

        /// <summary>The key for executing the RPD pipeline.</summary>
        public const string ExecuteRPDPipeline = "ExecuteRPDPipeline";
    }
}
