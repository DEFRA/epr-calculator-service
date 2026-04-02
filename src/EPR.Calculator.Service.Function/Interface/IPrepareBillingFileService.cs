namespace EPR.Calculator.Service.Function.Interface
{
    public interface IPrepareBillingFileService
    {
        Task<bool> PrepareBillingFileAsync(int calculatorRunId, string runName, string approvedBy);
    }
}
