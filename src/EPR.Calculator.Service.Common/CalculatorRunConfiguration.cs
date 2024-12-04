// <copyright file="CalculatorRunConfiguration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Common
{
    /// <summary>
    /// Represents the configuration settings required to run the calculator.
    /// </summary>
    public class CalculatorRunConfiguration
    {
        /// <summary>
        /// Gets or sets the URL of the pipeline.
        /// </summary>
        required public string PipelineUrl { get; set; }

        /// <summary>
        /// Gets or sets the name of the pipeline.
        /// </summary>
        required public string PipelineName { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of checks to perform.
        /// </summary>
        required public string MaxCheckCount { get; set; }

        /// <summary>
        /// Gets or sets the interval between checks.
        /// </summary>
        required public string CheckInterval { get; set; }
    }
}
