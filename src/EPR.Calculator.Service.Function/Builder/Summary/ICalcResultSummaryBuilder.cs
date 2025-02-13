﻿using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Builder.Summary
{
    public interface ICalcResultSummaryBuilder
    {
        Task<CalcResultSummary> Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult);
    }
}
