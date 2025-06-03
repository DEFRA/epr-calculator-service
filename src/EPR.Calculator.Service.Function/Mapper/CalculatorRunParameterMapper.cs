// <copyright file="CalculatorRunParameterMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function.Mapper
{
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Function.Interface;

    /// <summary>
    /// Maps <see cref="CreateResultFileMessage"/> to <see cref="ICalculatorRunParameterMapper"/>.
    /// </summary>
    public class CalculatorRunParameterMapper : ICalculatorRunParameterMapper
    {
        /// <summary>
        /// Maps a <see cref="CreateResultFileMessage"/> to a <see cref="CalculatorRunParameter"/>.
        /// </summary>
        /// <param name="calculatorParameter">The calculator parameter to map.</param>
        /// <returns>The mapped calculator run parameter.</returns>
        public CalculatorRunParameter Map(CreateResultFileMessage calculatorParameter)
        {
            return new CalculatorRunParameter()
            {
                FinancialYear = calculatorParameter.FinancialYear,
                User = calculatorParameter.CreatedBy,
                Id = calculatorParameter.CalculatorRunId,
            };
        }

        public BillingFileMessage Map(CreateBillingFileMessage billingMessageQueue)
        {
            return new BillingFileMessage()
            {
                Id = billingMessageQueue.CalculatorRunId,
                ApprovedBy = billingMessageQueue.ApprovedBy,
                MessageType = billingMessageQueue.MessageType,
            };
        }
    }
}
