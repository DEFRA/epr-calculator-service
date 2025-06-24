using Newtonsoft.Json;
using System;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultDetailJson
    {
        [JsonProperty(PropertyName = "runName")]
        public required string RunName { get; init; } = string.Empty;

        [JsonProperty(PropertyName = "runId")]
        public required int RunId { get; init; }

        [JsonProperty(PropertyName = "runDate")]
        public required string RunDate { get; init; } = string.Empty;

        [JsonProperty(PropertyName = "runBy")]
        public required string RunBy { get; init; } = string.Empty;

        [JsonProperty(PropertyName = "financialYear")]
        public required string FinancialYear { get; init; } = string.Empty;

        [JsonProperty(PropertyName = "rpdFileORG")]
        public required string RpdFileORG { get; init; } = string.Empty;

        [JsonProperty(PropertyName = "rpdFileORGTimeStamp")]
        public required string RpdFileORGTimeStamp { get; init; } = string.Empty;

        [JsonProperty(PropertyName = "rpdFilePOM")]
        public required string RpdFilePOM { get; init; } = string.Empty;

        [JsonProperty(PropertyName = "rpdFilePOMTimeStamp")]
        public required string RpdFilePOMTimeStamp { get; init; } = string.Empty;

        [JsonProperty(PropertyName = "lapcapFile")]
        public required string LapcapFile { get; init; } = string.Empty;

        [JsonProperty(PropertyName = "lapcapFileTimeStamp")]
        public required string LapcapFileTimeStamp { get; init; } = string.Empty;

        [JsonProperty(PropertyName = "lapcapFileUploader")]
        public required string LapcapFileUploader { get; init; } = string.Empty;

        [JsonProperty(PropertyName = "parametersFile")]
        public required string ParametersFile { get; init; } = string.Empty;

        [JsonProperty(PropertyName = "parametersFileTimeStamp")]
        public required string ParametersFileTimeStamp { get; init; } = string.Empty;

        [JsonProperty(PropertyName = "parametersFileUploader")]
        public required string ParametersFileUploader { get; init; } = string.Empty;
    }
}
