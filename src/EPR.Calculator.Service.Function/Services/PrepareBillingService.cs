using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Services
{
    public class PrepareBillingService : IPrepareBillingService
    {
        private readonly ICalcResultBuilder builder;

        public PrepareBillingService(ICalcResultBuilder builder) 
        { 
            this.builder = builder;
        }

        public async Task PrepareBilling([FromBody] CalcResultsRequestDto resultsRequestDto) 
        {
            resultsRequestDto.IsBilling = true;
            resultsRequestDto.OrganisationIds = new List<int> { 1, 2, 3, 4, 5 };
            var results = await this.builder.Build(resultsRequestDto);

            // Add code for Writing it to JSON file
        }
    }
}
