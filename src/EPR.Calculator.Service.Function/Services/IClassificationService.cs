using EPR.Calculator.API.Data.Enums;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IClassificationService
    {
        public Task UpdateRunClassification(int runId, RunClassification runClassification);
    }
}
