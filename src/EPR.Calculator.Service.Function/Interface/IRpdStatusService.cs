namespace EPR.Calculator.Service.Function.Interface
{
    using EPR.Calculator.Service.Function.Enums;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRpdStatusService
    {
        Task<RunClassification> UpdateRpdStatus(int runId, string runName, string updatedBy, bool isPomSuccessful, CancellationToken timeout);
    }
}