﻿using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Builder.LaDisposalCost
{
    public interface ICalcRunLaDisposalCostBuilder
    {
        Task<CalcResultLaDisposalCostData> Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult);
    }
}
