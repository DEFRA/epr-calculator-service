namespace EPR.Calculator.Service.Function.Interface
{
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Function.Enums;

    public interface IRpdStatusService
    {
        Task<RunClassification> UpdateRpdStatus(int runId, string? runName, string updatedBy, CancellationToken timeout);
    }
}