﻿// <copyright file="ICalculatorRunService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function.Interface
{
    using EPR.Calculator.Service.Common;

    public interface ICalculatorRunService
    {
        public void StartProcess(CalculatorRunParameter calculatorRunParameter);
    }
}
