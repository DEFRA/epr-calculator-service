// <copyright file="CalculatorRunParameterMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function.Mapper
{
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Function.Interface;

    /// <summary>
    /// Maps <see cref="CalculatorParameter"/> to <see cref="CalculatorRunParameter"/>.
    /// </summary>
    public class CalculatorRunParameterMapper : ICalculatorRunParameterMapper
    {
        /// <summary>
        /// Maps a <see cref="CalculatorParameter"/> to a <see cref="CalculatorRunParameter"/>.
        /// </summary>
        /// <param name="calculatorParameter">The calculator parameter to map.</param>
        /// <returns>The mapped calculator run parameter.</returns>
        public CalculatorRunParameter Map(CalculatorParameter calculatorParameter)
        {
            return new CalculatorRunParameter()
            {
                FinancialYear = calculatorParameter.FinancialYear,
                User = calculatorParameter.CreatedBy,
                Id = calculatorParameter.CalculatorRunId,
            };
        }
    }
}
