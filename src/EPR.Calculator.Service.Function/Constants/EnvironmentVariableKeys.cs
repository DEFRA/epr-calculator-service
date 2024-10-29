// <copyright file="Config.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;

namespace EPR.Calculator.Service.Function.Constants
{
    public static class EnvironmentVariableKeys
    {
        public const string PipelineUrl = "PipelineUrl";
        public const string GetOrgDataPipelineName = "GetOrgDataPipelineName";
        public const string GetPomDataPipelineName = "GetPomDataPipelineName";
        public const string CheckInterval = "CheckInterval";
        public const string MaxCheckCount = "MaxCheckCount";

        /// <summary>The URL of the endpoint called to update the pipeline status in the database.</summary>
        public const string StatusUpdateEndpoint = "StatusUpdateEndpoint";
    }
}
