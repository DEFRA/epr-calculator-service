namespace EPR.Calculator.Service.Function.Interface
{
    using System.Collections.Generic;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Models;

    public interface IRpdStatusDataValidator
    {
        RpdStatusValidation IsValidRun(CalculatorRun? calcRun, int runId, IEnumerable<CalculatorRunClassification> calculatorRunClassifications);

        RpdStatusValidation IsValidSuccessfulRun(int runId);
    }
}
