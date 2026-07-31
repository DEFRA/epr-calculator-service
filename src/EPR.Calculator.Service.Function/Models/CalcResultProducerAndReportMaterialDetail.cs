using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Models;

public record CalcResultProducerAndReportMaterialDetail
{
    public required ProducerDetail ProducerDetail { get; init; }
    public required ProducerMaterialPackaging ProducerMaterialPackaging { get; init; }
}
