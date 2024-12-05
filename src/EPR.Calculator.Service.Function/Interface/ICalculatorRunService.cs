// <copyright file="ICalculatorRunService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function.Interface
{
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Common;

    /// <summary>
    /// Interface for starting the calculator process.
    /// </summary>
    public interface ICalculatorRunService
    {
        /// <summary>
        /// Interface method to start the calculator process.
        /// </summary>
        /// <param name="calculatorRunParameter">The parameters required to run the calculator.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
        Task<bool> StartProcess(CalculatorRunParameter calculatorRunParameter);
    }
}
