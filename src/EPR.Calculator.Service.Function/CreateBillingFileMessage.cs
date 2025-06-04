// <copyright file="CreateBillingFileMessage.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

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
        /// Gets or sets the user who approved the billing file.
        /// </summary>
        public required string ApprovedBy { get; set; }
    }
}
