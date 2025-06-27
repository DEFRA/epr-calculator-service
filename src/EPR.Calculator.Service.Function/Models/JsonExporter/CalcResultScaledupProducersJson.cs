using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record CalcResultScaledupProducersJson
    {
        [JsonProperty(PropertyName = "name")]
        public string? Name { get; set; }

        [JsonProperty(PropertyName = "producerSubmissions")]
        public IEnumerable<ProducerSubmission>? ProducerSubmissions { get; set; }
    }

    public record ProducerSubmission
    {
        [JsonProperty(PropertyName = "producerId")]
        public int ProducerId { get; set; }

        [JsonProperty(PropertyName = "subsidiaryId")]
        public string? SubsidiaryId { get; set; }

        [JsonProperty(PropertyName = "producerName")]
        public string? ProducerName { get; set; }

        [JsonProperty(PropertyName = "tradingName")]
        public string? TradingName { get; set; }

        [JsonProperty(PropertyName = "level")]
        public int? Level { get; set; }

        [JsonProperty(PropertyName = "submissionPeriodCode")]
        public string? SubmissionPeriodCode { get; set; }

        [JsonProperty(PropertyName = "daysInSubmissionPeriod")]
        public int DaysInSubmissionPeriod { get; set; }

        [JsonProperty(PropertyName = "daysInWholePeriod")]
        public int DaysInWholePeriod { get; set; }

        [JsonProperty(PropertyName = "scaleUpFactor")]
        public decimal ScaleUpFactor { get; set; }

        [JsonProperty(PropertyName = "materialBreakdown")]
        public required IEnumerable<MaterialBreakdown> MaterialBreakdown { get; set; }
    }

    public record MaterialBreakdown
    {
        [JsonProperty(PropertyName = "materialName")]
        public string? MaterialName { get; set; }

        [JsonProperty(PropertyName = "reportedHouseholdPackagingWasteTonnage")]
        public decimal ReportedHouseholdPackagingWasteTonnage { get; set; }

        [JsonProperty(PropertyName = "reportedPublicBinTonnage")]
        public decimal ReportedPublicBinTonnage { get; set; }

        [JsonProperty(PropertyName = "householdDrinksContainersTonnageGlass", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? HouseholdDrinksContainersTonnageGlass { get; set; }

        [JsonProperty(PropertyName = "totalReportedTonnage")]
        public decimal TotalReportedTonnage { get; set; }

        [JsonProperty(PropertyName = "reportedSelfManagedConsumerWasteTonnage")]
        public decimal ReportedSelfManagedConsumerWasteTonnage { get; set; }

        [JsonProperty(PropertyName = "netReportedTonnage")]
        public decimal NetReportedTonnage { get; set; }

        [JsonProperty(PropertyName = "scaledUpReportedHouseholdPackagingWasteTonnage")]
        public decimal ScaledUpReportedHouseholdPackagingWasteTonnage { get; set; }

        [JsonProperty(PropertyName = "scaledUpReportedPublicBinTonnage")]
        public decimal ScaledUpReportedPublicBinTonnage { get; set; }

        [JsonProperty(PropertyName = "scaledUpHouseholdDrinksContainersTonnageGlass", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? ScaledUpHouseholdDrinksContainersTonnageGlass { get; set; }

        [JsonProperty(PropertyName = "scaledUpTotalReportedTonnage")]
        public decimal ScaledUpTotalReportedTonnage { get; set; }

        [JsonProperty(PropertyName = "scaledUpReportedSelfManagedConsumerWasteTonnage")]
        public decimal ScaledUpReportedSelfManagedConsumerWasteTonnage { get; set; }

        [JsonProperty(PropertyName = "scaledUpNetReportedTonnage")]
        public decimal ScaledUpNetReportedTonnage { get; set; }
    }
}
