// <copyright file="ICalculatorRunService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function.Interface
{
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.AzureSynapse;

    public interface ICalculatorRunService
    {
        /// <param name="azureSynapseRunner">An <see cref="IAzureSynapseRunner"/>.</param>
        public void StartProcess(CalculatorRunParameter calculatorRunParameter, IAzureSynapseRunner azureSynapseRunner);
    }
}
