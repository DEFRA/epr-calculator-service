using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.Model
{
    public record CalcResultScaledupProducerJson
    {
        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("producerSubmissions")]
        public required IEnumerable<ProducerSubmission> ProducerSubmissions { get; set; }
    }

    public record ProducerSubmission
    {
        [JsonProperty("producerId")]
        public int ProducerId { get; set; }

        [JsonProperty("subsidiaryId")]
        public string? SubsidiaryId { get; set; }

        [JsonProperty("producerName")]
        public string? ProducerName { get; set; }

        [JsonProperty("tradingName")]
        public string? TradingName { get; set; }

        [JsonProperty("level")]
        public int? Level { get; set; }

        [JsonProperty("submissionPeriodCode")]
        public string? SubmissionPeriodCode { get; set; }

        [JsonProperty("daysInSubmissionPeriod")]
        public int DaysInSubmissionPeriod { get; set; }

        [JsonProperty("daysInWholePeriod")]
        public int DaysInWholePeriod { get; set; }

        [JsonProperty("scaleUpFactor")]
        public decimal ScaleUpFactor { get; set; }

        [JsonProperty("materialBreakdown")]
        public required IEnumerable<MaterialBreakdown> MaterialBreakdown { get; set; }
    }

    public record MaterialBreakdown
    {
        [JsonProperty("materialName")]
        public required string MaterialName { get; set; }

        [JsonProperty("reportedHouseholdPackagingWasteTonnage")]
        public decimal ReportedHouseholdPackagingWasteTonnage { get; set; }

        [JsonProperty("reportedPublicBinTonnage")]
        public decimal ReportedPublicBinTonnage { get; set; }

        [JsonProperty("totalReportedTonnage")]
        public decimal TotalReportedTonnage { get; set; }

        [JsonProperty("reportedSelfManagedConsumerWasteTonnage")]
        public decimal ReportedSelfManagedConsumerWasteTonnage { get; set; }

        [JsonProperty("netReportedTonnage")]
        public decimal NetReportedTonnage { get; set; }

        [JsonProperty("scaledUpReportedHouseholdPackagingWasteTonnage")]
        public decimal ScaledUpReportedHouseholdPackagingWasteTonnage { get; set; }

        [JsonProperty("scaledUpReportedPublicBinTonnage")]
        public decimal ScaledUpReportedPublicBinTonnage { get; set; }

        [JsonProperty("scaledUpTotalReportedTonnage")]
        public decimal ScaledUpTotalReportedTonnage { get; set; }

        [JsonProperty("scaledUpReportedSelfManagedConsumerWasteTonnage")]
        public decimal ScaledUpReportedSelfManagedConsumerWasteTonnage { get; set; }

        [JsonProperty("scaledUpNetReportedTonnage")]
        public decimal ScaledUpNetReportedTonnage { get; set; }
    }
}
