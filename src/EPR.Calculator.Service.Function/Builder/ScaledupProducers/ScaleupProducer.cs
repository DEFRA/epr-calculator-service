﻿namespace EPR.Calculator.Service.Function.Builder.ScaledupProducers
{
    public class ScaleupProducer
    {
        public int OrganisationId { get; set; }
        public decimal ScaleupFactor { get; set; }
        public string SubmissionPeriod { get; set; }
        public int DaysInSubmissionPeriod { get; set; }
        public int DaysInWholePeriod { get; set; }
    }
}
