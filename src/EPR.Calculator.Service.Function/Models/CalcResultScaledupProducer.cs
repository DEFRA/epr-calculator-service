using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultScaledupProducer
    {
        public required int ProducerId { get; set; }

        public required string SubsidiaryId { get; set; }

        public required string ProducerName { get; set; }

        public string Level { get; set; }

        public bool isTotalRow { get; set; } = false;

        public required string SubmissonPeriodCode { get; set; }

        public required int DaysInSubmissionPeriod { get; set; }

        public required int DaysInWholePeriod { get; set; }

        public required decimal ScaleupFactor { get; set; }

        public Dictionary<string, CalcResultScaledupProducerTonnage> ScaledupProducerTonnageByMaterial { get; set; } = new Dictionary<string, CalcResultScaledupProducerTonnage>();
    }
}
