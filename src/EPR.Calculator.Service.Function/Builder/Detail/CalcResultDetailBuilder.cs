﻿namespace EPR.Calculator.Service.Function.Builder.Detail
{
    using System;
    using System.Threading.Tasks;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.EntityFrameworkCore;

    public class CalcResultDetailBuilder : ICalcResultDetailBuilder
    {
        private readonly ApplicationDBContext context;

        public CalcResultDetailBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<CalcResultDetail> Construct(CalcResultsRequestDto resultsRequestDto)
        {
            var calcResultDetails = await this.context.CalculatorRuns
                .Include(o => o.CalculatorRunOrganisationDataMaster)
                .Include(o => o.CalculatorRunPomDataMaster)
                .Include(o => o.DefaultParameterSettingMaster)
                .Include(x => x.LapcapDataMaster)
                .ToListAsync();

            var results = new CalcResultDetail();
            var calcResultDetail = calcResultDetails.Find(x => x.Id == resultsRequestDto.RunId);
            if (calcResultDetail != null)
            {
                results.RunId = calcResultDetail.Id;
                results.RunName = calcResultDetail.Name ?? string.Empty;
                results.RunBy = calcResultDetail.CreatedBy;
                results.RunDate = calcResultDetail.CreatedAt;
                results.FinancialYear = calcResultDetail.Financial_Year?? string.Empty;
                if (calcResultDetail.CalculatorRunOrganisationDataMaster != null)
                    results.RpdFileORG = calcResultDetail.CalculatorRunOrganisationDataMaster.CreatedAt.ToString(CalculationResults.DateFormat);
                if (calcResultDetail.CalculatorRunPomDataMaster != null)
                    results.RpdFilePOM = calcResultDetail.CalculatorRunPomDataMaster.CreatedAt.ToString(CalculationResults.DateFormat);
                if (calcResultDetail.LapcapDataMaster != null)
                    results.LapcapFile = FormatFileData(calcResultDetail.LapcapDataMaster.LapcapFileName, calcResultDetail.LapcapDataMaster.CreatedAt, calcResultDetail.LapcapDataMaster.CreatedBy);
                if (calcResultDetail.DefaultParameterSettingMaster != null)
                    results.ParametersFile = FormatFileData(calcResultDetail.DefaultParameterSettingMaster.ParameterFileName, calcResultDetail.DefaultParameterSettingMaster.CreatedAt, calcResultDetail.DefaultParameterSettingMaster.CreatedBy);
            }
            return results;
        }

        private static string FormatFileData(string fileName, DateTime createdAt, string createdBy)
        {
            return $"{fileName},{createdAt.ToString(CalculationResults.DateFormat)},{createdBy}";
        }
    }
}