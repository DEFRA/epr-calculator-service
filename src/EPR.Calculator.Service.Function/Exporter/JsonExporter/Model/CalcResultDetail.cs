using System;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.Model
{
    public class CalcResultDetailJson
    {
        public string RunName { get; set; } = string.Empty;
        public int RunId { get; set; }
        public string RunDate { get; set; } = string.Empty;
        public string RunBy { get; set; } = string.Empty;
        public string FinancialYear { get; set; } = string.Empty;
        public string RpdFileORG { get; set; } = string.Empty;
        public string RpdFileORGTimeStamp { get; set; } = string.Empty;
        public string RpdFilePOM { get; set; } = string.Empty;
        public string RpdFilePOMTimeStamp { get; set; } = string.Empty;
        public string LapcapFile { get; set; } = string.Empty;
        public string LapcapFileTimeStamp { get; set; } = string.Empty;
        public string LapcapFileUploader { get; set; } = string.Empty;
        public string ParametersFile { get; set; } = string.Empty;
        public string ParametersFileTimeStamp { get; set; } = string.Empty;
        public string ParametersFileUploader { get; set; } = string.Empty;
    }
}
