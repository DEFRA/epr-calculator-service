using System;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalcResultDetailJson
    {
        public required string RunName { get; init; } = string.Empty;
        public required int RunId { get; init; }
        public required string RunDate { get; init; } = string.Empty;
        public required string RunBy { get; init; } = string.Empty;
        public required string FinancialYear { get; init; } = string.Empty;
        public required string RpdFileORG { get; init; } = string.Empty;
        public required string RpdFileORGTimeStamp { get; init; } = string.Empty;
        public required string RpdFilePOM { get; init; } = string.Empty;
        public required string RpdFilePOMTimeStamp { get; init; } = string.Empty;
        public required string LapcapFile { get; init; } = string.Empty;
        public required string LapcapFileTimeStamp { get; init; } = string.Empty;
        public required string LapcapFileUploader { get; init; } = string.Empty;
        public required string ParametersFile { get; init; } = string.Empty;
        public required string ParametersFileTimeStamp { get; init; } = string.Empty;
        public required string ParametersFileUploader { get; init; } = string.Empty;
    }
}
