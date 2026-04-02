using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Interface
{
    public interface IRpdStatusDataValidator
    {
        RpdStatusValidation IsValidRun(CalculatorRun? calcRun, int runId, IEnumerable<CalculatorRunClassification> calculatorRunClassifications);

        RpdStatusValidation IsValidSuccessfulRun(int runId);
    }
}
