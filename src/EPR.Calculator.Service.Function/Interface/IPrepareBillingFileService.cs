namespace EPR.Calculator.Service.Function.Interface
{
    public interface IPrepareBillingFileService
    {
        Task<bool> PrepareBillingFileAsync(BillingRunParams runParams);
    }
}