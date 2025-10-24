using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Builder.ErrorReport
{
    public class CalcResultErrorReportBuilder : ICalcResultErrorReportBuilder
    {
        private readonly ApplicationDBContext context;

        public CalcResultErrorReportBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<CalcResultErrorReport>> ConstructAsync(CalcResultsRequestDto resultsRequestDto)
        {
            var result = await (
                from er in context.ErrorReport
                join et in context.ErrorType on er.ErrorTypeId equals et.Id
                join run in context.CalculatorRuns on er.CalculatorRunId equals run.Id
                join odm in context.CalculatorRunOrganisationDataMaster on run.CalculatorRunOrganisationDataMasterId equals odm.Id
                join odd in context.CalculatorRunOrganisationDataDetails on odm.Id equals odd.CalculatorRunOrganisationDataMasterId
                where run.Id == 123
                select new CalcResultErrorReport
                {
                    ProducerId = er.ProducerId,
                    SubsidiaryId = er.SubsidiaryId,
                    ProducerName = odd.OrganisationName,
                    TradingName = odd.TradingName,
                    LeaverCode = odd.LeaverCode,
                    ErrorCodeText = et.Name
                }
            ).AsNoTracking().ToListAsync();

            return result;
        }
    }
}
