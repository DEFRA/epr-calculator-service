// <copyright file="CalculatorRunParameterMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function.Mapper
{
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Function.Interface;

    /// <summary>
    /// Maps <see cref="CreateResultFileMessage"/> and <see cref="CreateBillingFileMessage"/> to <see cref="ICalculatorRunParameterMapper"/>.
    /// </summary>
    public class CalculatorRunParameterMapper : ICalculatorRunParameterMapper
    {
        /// <summary>
        /// Maps a <see cref="CreateResultFileMessage"/> to a <see cref="CalculatorRunParameter"/>.
        /// </summary>
        /// <param name="createResultFileMessage">The result file message to map.</param>
        /// <returns>The mapped result file message.</returns>
        public CalculatorRunParameter Map(CreateResultFileMessage createResultFileMessage)
        {
            return new CalculatorRunParameter()
            {
                FinancialYear = createResultFileMessage.FinancialYear,
                User = createResultFileMessage.CreatedBy,
                Id = createResultFileMessage.CalculatorRunId,
                MessageType = createResultFileMessage.MessageType,
            };
        }

        /// <summary>
        /// Maps a <see cref="CreateBillingFileMessage"/> to a <see cref="CalculatorRunParameter"/>.
        /// </summary>
        /// <param name="createBillingFileMessage">The billing file message to map.</param>
        /// <returns>The mapped billing file message.</returns>
        public BillingFileMessage Map(CreateBillingFileMessage createBillingFileMessage)
        {
            return new BillingFileMessage()
            {
                Id = createBillingFileMessage.CalculatorRunId,
                ApprovedBy = createBillingFileMessage.ApprovedBy,
                MessageType = createBillingFileMessage.MessageType,
            };
        }
    }
}
