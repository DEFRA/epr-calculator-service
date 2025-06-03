// <copyright file="ICalculatorRunParameterMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function.Interface
{
    using EPR.Calculator.Service.Common;

    /// <summary>
    /// Interface for mapping calculator parameters to calculator run message.
    /// </summary>
    public interface ICalculatorRunMessageMapper
    {
        /// <summary>
        /// Maps a <see cref="CreateResultFileMessage"/> to a <see cref="CalculatorRunMessage"/>.
        /// </summary>
        /// <param name="resultMessageQueue">The result message queue to map.</param>
        /// <returns>The mapped calculator run parameter.</returns>
        CalculatorRunParameter Map(CreateResultFileMessage resultMessageQueue);

        /// <summary>
        /// Maps a <see cref="CreateBillingFileMessage"/> to a <see cref="CalculatorRunMessage"/>.
        /// </summary>
        /// <param name="billingMessageQueue">The billing message queue to map.</param>
        /// <returns>The mapped calculator run parameter.</returns>
        BillingFileMessage Map(CreateBillingFileMessage billingMessageQueue);
    }
}
