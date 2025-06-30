using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record CalcResultScaledupProducersJson
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("producerSubmissions")]
        public IEnumerable<ProducerSubmission>? ProducerSubmissions { get; set; }
    }

    public record ProducerSubmission
    {
        [JsonPropertyName("producerId")]
        public int ProducerId { get; set; }

        [JsonPropertyName("subsidiaryId")]
        public string? SubsidiaryId { get; set; }

        [JsonPropertyName("producerName")]
        public string? ProducerName { get; set; }

        [JsonPropertyName("tradingName")]
        public string? TradingName { get; set; }

        [JsonPropertyName("level")]
        public int? Level { get; set; }

        [JsonPropertyName("submissionPeriodCode")]
        public string? SubmissionPeriodCode { get; set; }

        [JsonPropertyName("daysInSubmissionPeriod")]
        public int DaysInSubmissionPeriod { get; set; }

        [JsonPropertyName("daysInWholePeriod")]
        public int DaysInWholePeriod { get; set; }

        [JsonPropertyName("scaleUpFactor")]
        public decimal ScaleUpFactor { get; set; }

        [JsonPropertyName("materialBreakdown")]
        public required IEnumerable<MaterialBreakdown> MaterialBreakdown { get; set; }
    }

    public record MaterialBreakdown
    {
        [JsonPropertyName("materialName")]
        public string? MaterialName { get; set; }

        [JsonPropertyName("reportedHouseholdPackagingWasteTonnage")]
        public decimal ReportedHouseholdPackagingWasteTonnage { get; set; }

        [JsonPropertyName("reportedPublicBinTonnage")]
        public decimal ReportedPublicBinTonnage { get; set; }

        [JsonPropertyName("householdDrinksContainersTonnageGlass")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? HouseholdDrinksContainersTonnageGlass { get; set; }

        [JsonPropertyName("totalReportedTonnage")]
        public decimal TotalReportedTonnage { get; set; }

        [JsonPropertyName("reportedSelfManagedConsumerWasteTonnage")]
        public decimal ReportedSelfManagedConsumerWasteTonnage { get; set; }

        [JsonPropertyName("netReportedTonnage")]
        public decimal NetReportedTonnage { get; set; }

        [JsonPropertyName("scaledUpReportedHouseholdPackagingWasteTonnage")]
        public decimal ScaledUpReportedHouseholdPackagingWasteTonnage { get; set; }

        [JsonPropertyName("scaledUpReportedPublicBinTonnage")]
        public decimal ScaledUpReportedPublicBinTonnage { get; set; }

        [JsonPropertyName("scaledUpHouseholdDrinksContainersTonnageGlass")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? ScaledUpHouseholdDrinksContainersTonnageGlass { get; set; }

        [JsonPropertyName("scaledUpTotalReportedTonnage")]
        public decimal ScaledUpTotalReportedTonnage { get; set; }

        [JsonPropertyName("scaledUpReportedSelfManagedConsumerWasteTonnage")]
        public decimal ScaledUpReportedSelfManagedConsumerWasteTonnage { get; set; }

        [JsonPropertyName("scaledUpNetReportedTonnage")]
        public decimal ScaledUpNetReportedTonnage { get; set; }
    }
}
