using System;
using Newtonsoft.Json;
using EPR.Calculator.Service.Common;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.Service.Function
{
    /// <summary>
    /// Calculator Run Parameter
    /// </summary>
    public class CreateResultFileMessage : MessageBase
    {
        /// <summary>
        /// Gets or sets the relative year for the calculator run.
        /// </summary>
        public required RelativeYear RelativeYear { get; set; }

        /// <summary>
        /// Gets or sets the user who initiated the calculator run.
        /// </summary>
        public required string CreatedBy { get; set; }
    }
}
