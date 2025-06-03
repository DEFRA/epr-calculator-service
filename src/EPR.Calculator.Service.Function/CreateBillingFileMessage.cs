// <copyright file="CalculatorParameter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using EPR.Calculator.Service.Function.Interface;

namespace EPR.Calculator.Service.Function
{
    /// <summary>
    /// Calculator Run Parameter
    /// </summary>
    public class CreateBillingFileMessage : MessageBase
    {
        /// <summary>
        /// Gets or sets the identifier for the calculator run.
        /// </summary>
        public int RunId { get; set; }

        /// <summary>
        /// Gets or sets the financial year for the calculator run.
        /// </summary>
        public required string ApprovedBy { get; set; }
    }
}
