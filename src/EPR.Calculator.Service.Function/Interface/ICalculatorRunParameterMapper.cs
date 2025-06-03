// <copyright file="ICalculatorRunParameterMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function.Interface
{
    using EPR.Calculator.Service.Common;

    /// <summary>
    /// Interface for mapping calculator parameters to calculator run parameters.
    /// </summary>
    public interface ICalculatorRunParameterMapper
    {
        /// <summary>
        /// Maps a <see cref="CalculatorParameter"/> to a <see cref="CalculatorRunParameter"/>.
        /// </summary>
        /// <param name="calculatorParameter">The calculator parameter to map.</param>
        /// <returns>The mapped calculator run parameter.</returns>
        CalculatorRunParameter Map(CreateResultFileMessage calculatorParameter);

        /// <summary>
        /// Maps a <see cref="CalculatorParameter"/> to a <see cref="CalculatorRunParameter"/>.
        /// </summary>
        /// <param name="calculatorParameter">The calculator parameter to map.</param>
        /// <returns>The mapped calculator run parameter.</returns>
        BillingFileMessage Map(CreateBillingFileMessage calculatorParameter);
    }
}
