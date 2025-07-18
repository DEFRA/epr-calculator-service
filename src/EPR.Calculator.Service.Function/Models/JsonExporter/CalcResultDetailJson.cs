using System.Text.Json.Serialization;
using System;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultDetailJson
    {
        [JsonPropertyName("runName")]
        public required string RunName { get; init; } = string.Empty;

        [JsonPropertyName("runId")]
        public required int RunId { get; init; }

        [JsonPropertyName("runDate")]
        public required string RunDate { get; init; } = string.Empty;

        [JsonPropertyName("runBy")]
        public required string RunBy { get; init; } = string.Empty;

        [JsonPropertyName("financialYear")]
        public required string FinancialYear { get; init; } = string.Empty;

        [JsonPropertyName("rpdFileORG")]
        public required string RpdFileORG { get; init; } = string.Empty;

        [JsonPropertyName("rpdFileORGTimeStamp")]
        public required string RpdFileORGTimeStamp { get; init; } = string.Empty;

        [JsonPropertyName("rpdFilePOM")]
        public required string RpdFilePOM { get; init; } = string.Empty;

        [JsonPropertyName("rpdFilePOMTimeStamp")]
        public required string RpdFilePOMTimeStamp { get; init; } = string.Empty;

        [JsonPropertyName("lapcapFile")]
        public required string LapcapFile { get; init; } = string.Empty;

        [JsonPropertyName("lapcapFileTimeStamp")]
        public required string LapcapFileTimeStamp { get; init; } = string.Empty;

        [JsonPropertyName("lapcapFileUploader")]
        public required string LapcapFileUploader { get; init; } = string.Empty;

        [JsonPropertyName("parametersFile")]
        public required string ParametersFile { get; init; } = string.Empty;

        [JsonPropertyName("parametersFileTimeStamp")]
        public required string ParametersFileTimeStamp { get; init; } = string.Empty;

        [JsonPropertyName("parametersFileUploader")]
        public required string ParametersFileUploader { get; init; } = string.Empty;
    }
}
