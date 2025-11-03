using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

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
            return await (
                from er in context.ErrorReports
                join et in context.ErrorTypes on er.ErrorTypeId equals et.Id
                join run in context.CalculatorRuns on er.CalculatorRunId equals run.Id
                join odm in context.CalculatorRunOrganisationDataMaster on run.CalculatorRunOrganisationDataMasterId equals odm.Id
                join odd in context.CalculatorRunOrganisationDataDetails on odm.Id equals odd.CalculatorRunOrganisationDataMasterId                
                where run.Id == resultsRequestDto.RunId && odd.OrganisationId == er.ProducerId
                select new CalcResultErrorReport
                {
                    Id = er.Id,
                    ProducerId = er.ProducerId,
                    SubsidiaryId = er.SubsidiaryId ?? CommonConstants.Hyphen,
                    ProducerName = odd.OrganisationName,
                    TradingName = odd.TradingName ?? CommonConstants.Hyphen,
                    LeaverCode = er.LeaverCode ?? CommonConstants.Hyphen,
                    ErrorCodeText = et.Name
                }
            ).AsNoTracking().Distinct().ToListAsync();
        }
    }
}