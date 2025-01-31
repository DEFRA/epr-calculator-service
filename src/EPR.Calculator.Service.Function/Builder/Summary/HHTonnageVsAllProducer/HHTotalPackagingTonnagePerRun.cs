namespace EPR.Calculator.Service.Function.Builder.Summary.HHTonnageVsAllProducer;

public record HHTotalPackagingTonnagePerRun
{
    public int ProducerId { get; set; }
    public required string SubsidiaryId { get; set; }
    public decimal TotalPackagingTonnage { get; set; }
}