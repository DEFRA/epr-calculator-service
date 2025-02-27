namespace EPR.Calculator.Service.Function.Builder.Summary.TonnageVsAllProducer.cs
{
    public record TotalPackagingTonnagePerRun
    {
        public int ProducerId { get; set; }
        public required string SubsidiaryId { get; set; }
        public decimal TotalPackagingTonnage { get; set; }
    }
}
