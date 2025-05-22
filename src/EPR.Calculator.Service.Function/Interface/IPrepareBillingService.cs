namespace EPR.Calculator.Service.Function.Interface
{
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Function.Dtos;
    using Microsoft.AspNetCore.Mvc;

    public interface IPrepareBillingService
    {
        Task PrepareBilling([FromBody] CalcResultsRequestDto resultsRequestDto);
    }
}