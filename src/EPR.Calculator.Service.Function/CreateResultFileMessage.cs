// <copyright file="CalculatorParameter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function
{
    /// <summary>
    /// Calculator Run Parameter
    /// </summary>
    public class CreateResultFileMessage : MessageBase
    {
        

        /// <summary>
        /// Gets or sets the financial year for the calculator run.
        /// </summary>
        public required string FinancialYear { get; set; }

        /// <summary>
        /// Gets or sets the user who initiated the calculator run.
        /// </summary>
        public required string CreatedBy { get; set; }
    }
}
