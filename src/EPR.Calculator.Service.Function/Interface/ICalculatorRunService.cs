using EPR.Calculator.Service.Common;

namespace EPR.Calculator.Service.Function.Interface
{
    /// <summary>
    /// Interface for starting the calculator process.
    /// </summary>
    public interface ICalculatorRunService
    {
        /// <summary>
        /// Interface method to start the calculator process.
        /// </summary>
        /// <param name="calculatorRunParameter">The parameters required to run the calculator.</param>
        /// <param name="runName">The name of the calculator run.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
        Task<bool> PrepareResultsFileAsync(CalculatorRunParameter calculatorRunParameter, string runName);
    }
}