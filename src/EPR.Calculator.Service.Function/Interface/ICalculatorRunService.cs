// <copyright file="ICalculatorRunService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function.Interface
{
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.AzureSynapse;
    using System.Threading.Tasks;

    public interface ICalculatorRunService
    {
        public Task<bool> StartProcess(CalculatorRunParameter calculatorRunParameter);
    }
}
