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
        /// Maps a <see cref="CreateResultFileMessage"/> to a <see cref="CalculatorRunParameter"/>.
        /// </summary>
        /// <param name="createResultFileMessage">The result file message parameter to map.</param>
        /// <returns>The mapped calculator run parameter.</returns>
        CalculatorRunParameter Map(CreateResultFileMessage createResultFileMessage);

        /// <summary>
        /// Maps a <see cref="CreateBillingFileMessage"/> to a <see cref="CalculatorRunParameter"/>.
        /// </summary>
        /// <param name="createBillingFileMessage">The billing file message parameter to map.</param>
        /// <returns>The mapped calculator run parameter.</returns>
        BillingFileMessage Map(CreateBillingFileMessage createBillingFileMessage);
    }
}
