using EPR.Calculator.Service.Function.Enums;

namespace EPR.Calculator.Service.Function.Interface
{
    public interface IRpdStatusService
    {
        Task<RunClassification> UpdateRpdStatus(int runId, string? runName, string createdBy, CancellationToken timeout);
    }
}