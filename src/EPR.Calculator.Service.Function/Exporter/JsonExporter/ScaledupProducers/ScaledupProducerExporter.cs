using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.ScaledupProducers
{
    public record ScaledupProducers
    {
        public required string name {  get; set; }

        public required IEnumerable<ProducerSubmission> producerSubmissions { get; set; }
    }

    public record ProducerSubmission
    {
        public int producerId { get; set; }

        public required string subsidiaryId { get; set; }

        public string? producerName { get; set; }

        public string? tradingName { get; set; }

        public string level { get; set; }

        public required string submissionPeriodCode { get; set; }

        public int daysInSubmissionPeriod { get; set; }

        public int daysInWholePeriod { get; set; }

        public decimal scaleUpFactor { get; set; }

        public required IEnumerable<MaterialBreakdown> materialBreakdown {  get; set; }
    }

    public record MaterialBreakdown
    {
        public required string materialName { get; set; }

        [JsonConverter(typeof(RoundingJsonConverter), 3)]
        public decimal reportedHouseholdPackagingWasteTonnage { get; set; }

        public decimal reportedPublicBinTonnage { get; set; }

        public decimal totalReportedTonnage { get; set; }

        public decimal reportedSelfManagedConsumerWasteTonnage { get; set; }

        public decimal netReportedTonnage { get; set; }

        public decimal scaledUpReportedHouseholdPackagingWasteTonnage { get; set; }

        public decimal scaledUpReportedPublicBinTonnage { get; set; }

        public decimal scaledUpTotalReportedTonnage { get; set; }

        public decimal scaledUpReportedSelfManagedConsumerWasteTonnage { get; set; }

        public decimal scaledUpNetReportedTonnage { get; set; }
    }
}
