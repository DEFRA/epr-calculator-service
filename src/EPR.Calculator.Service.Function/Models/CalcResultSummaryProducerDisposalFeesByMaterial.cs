namespace EPR.Calculator.Service.Function.Models;

public class CalcResultSummaryProducerDisposalFeesByMaterial
    : CalcResultSummaryProducerMaterialBase
{
    public decimal ManagedConsumerWasteTonnage { get; set; }

    public decimal NetReportedTonnage { get; set; }

    public decimal PricePerTonne { get; set; }

    public decimal ProducerDisposalFee { get; set; }

    public decimal ProducerDisposalFeeWithBadDebtProvision { get; set; }

    public decimal? PreviousInvoicedTonnage { get; set; }

    public decimal? TonnageChange { get; set; }
}