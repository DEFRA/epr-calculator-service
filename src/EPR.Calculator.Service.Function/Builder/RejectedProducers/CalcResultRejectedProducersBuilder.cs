using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.RejectedProducers
{
    public class CalcResultRejectedProducersBuilder : ICalcResultRejectedProducersBuilder
    {
        public async Task<IEnumerable<CalcResultRejectedProducer>> Construct(CalcResultsRequestDto resultsRequestDto)
        {
            var result = new List<CalcResultRejectedProducer>();

            return result;
        }
    }
}
