﻿using EPR.Calculator.Service.Function.Converter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models
{
    public record CalcResultOnePlusFourApportionmentDetailJson
    {
        [JsonProperty(PropertyName = "england")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal England { get; set; }

        [JsonProperty(PropertyName = "wales")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal Wales { get; set; }

        [JsonProperty(PropertyName = "scotland")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal Scotland { get; set; }

        [JsonProperty(PropertyName = "northernIreland")]
        [JsonConverter(typeof(DecimalPrecisionConverter), 3)]
        public decimal NorthernIreland { get; set; }

        [JsonProperty(PropertyName = "total")]
        public string Total { get; set; } = string.Empty;
    }
}
