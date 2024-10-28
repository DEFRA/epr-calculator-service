// <copyright file="CalculatorRunConfiguration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Common
{
    public class CalculatorRunConfiguration
    {
        required public string PipelineUrl { get; set; }

        required public string PipelineName { get; set; }

        required public string MaxCheckCount { get; set; }

        required public string CheckInterval { get; set; }
    }
}
