using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IParameterService
    {
        public Task<Dictionary<string, decimal>> GetDefaultParameters(int runId);
    }
}
