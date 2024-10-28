// <copyright file="ICalculatorRunParameterMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function.Interface
{
    using EPR.Calculator.Service.Common;

    public interface ICalculatorRunParameterMapper
    {
        CalculatorRunParameter Map(CalculatorParameter calculatorParameter);
    }
}
