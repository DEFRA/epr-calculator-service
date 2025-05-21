// <copyright file="BillingInstruction.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function
{
    public record BillingInstruction
    {
        /// <summary>
        /// Gets or sets the identifier for the calculator run.
        /// </summary>
        [JsonRequired]
        public int RunId { get; set; }

        /// <summary>
        /// Gets or sets the user who initiated the calculator run.
        /// </summary>
        [JsonRequired]
        public required string RaisedBy { get; set; }
    }
}
