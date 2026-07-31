using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Models;

public record BillingResult
{
    public required CalcResultDetail CalcResultDetail { get; init; }
    public required CalcResultLapcapData CalcResultLapcapData { get; init; }
    public required CalcResultCommsCost CalcResultCommsCostReportDetail { get; init; }
    public required CalcResultLateReportingTonnage CalcResultLateReportingTonnageData { get; init; }
    public required CalcResultParameterOtherCost CalcResultParameterOtherCost { get; init; }
    public required CalcResultOnePlusFourApportionment CalcResultOnePlusFourApportionment { get; init; }
    public required CalcResultLaDisposalCostData CalcResultLaDisposalCostData { get; init; }
    public required CalcResultPartialObligations CalcResultPartialObligations { get; init; }
    public required CalcResultProjectedProducers? CalcResultProjectedProducers { get; init; }
    public required CalcResultScaledupProducers? CalcResultScaledupProducers { get; init; }
    public required ImmutableList<CalcResultCancelledProducer> CalcResultCancelledProducers { get; init; }
    public required ImmutableList<CalcResultRejectedProducer> CalcResultRejectedProducers { get; init; }
    public required ProducerFees ProducerFees { get; init; }
    public required SelfManagedConsumerWaste Smcw { get; init; }
    public required ModulationResult? CalcResultModulation { get; init; }
}
