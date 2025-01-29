// <copyright file="Configuration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;

namespace EPR.Calculator.Service.Function.Interface
{
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
    }
}