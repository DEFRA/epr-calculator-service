using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IPrepareProducerDataInsertService
    {
        public Task<bool> InsertProducerDataToDatabase(CalcResult calcResult);
    }
}
