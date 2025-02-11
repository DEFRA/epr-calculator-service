using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultScaledupProducer
    {
        public int ProducerId { get; set; }

        public string SubsidiaryId { get; set; }

        public string ProducerName { get; set; }

        public string Level { get; set; }

        public bool isSubtotalRow { get; set; } = false;

        public string SubmissonPeriodCode { get; set; }

        public int DaysInSubmissionPeriod { get; set; }

        public int DaysInWholePeriod { get; set; }

        public decimal ScaleupFactor { get; set; }

        public Dictionary<string, CalcResultScaledupProducerTonnage> ScaledupProducerTonnageByMaterial { get; set; } = new Dictionary<string, CalcResultScaledupProducerTonnage>();
    }
}
