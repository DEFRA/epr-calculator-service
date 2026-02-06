// <copyright file="CalculatorParameter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using Newtonsoft.Json;
using EPR.Calculator.Service.Common;

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
        [JsonConverter(typeof(FinancialYearConverter))]
        public required FinancialYear FinancialYear { get; set; }

        /// <summary>
        /// Gets or sets the user who initiated the calculator run.
        /// </summary>
        public required string CreatedBy { get; set; }
    }
}

public class FinancialYearConverter : JsonConverter<FinancialYear>
{
    public override void WriteJson(JsonWriter writer, FinancialYear value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override FinancialYear ReadJson(JsonReader reader, Type objectType, FinancialYear existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var s = reader.Value?.ToString() ?? throw new JsonSerializationException("Expected a FinancialYear string");
        return new FinancialYear(s);
    }
}
