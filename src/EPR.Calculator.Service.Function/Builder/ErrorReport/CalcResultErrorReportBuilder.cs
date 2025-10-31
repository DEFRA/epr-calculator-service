using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
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
            var orgDetails = await (from run in context.CalculatorRuns.AsNoTracking()
                                    join odm in context.CalculatorRunOrganisationDataMaster
                                        on run.CalculatorRunOrganisationDataMasterId equals odm.Id
                                    join odd in context.CalculatorRunOrganisationDataDetails on odm.Id equals odd.CalculatorRunOrganisationDataMasterId
                                    where run.Id == resultsRequestDto.RunId
                                    select odd).ToListAsync();
            bool isTradingName = false;
            var query =
                from er in context.ErrorReports
                join run in context.CalculatorRuns on er.CalculatorRunId equals run.Id

                join et in context.ErrorTypes on er.ErrorTypeId equals et.Id
                join odm in context.CalculatorRunOrganisationDataMaster
                    on run.CalculatorRunOrganisationDataMasterId equals odm.Id
                join odd in context.CalculatorRunOrganisationDataDetails
                    on new { OrgId = (int?)er.ProducerId, MasterId = odm.Id }
                    equals new { OrgId = odd.OrganisationId, MasterId = odd.CalculatorRunOrganisationDataMasterId }
                    into oddGroup
                from oddLeft in oddGroup.DefaultIfEmpty()
                where run.Id == resultsRequestDto.RunId
                select new CalcResultErrorReport
                {
                    Id = er.Id,
                    ProducerId = er.ProducerId,
                    SubsidiaryId = er.SubsidiaryId ?? CommonConstants.Hyphen,
                    ProducerName = oddLeft == null ? CommonConstants.Hyphen :
                                (GetOrgOrTradingName(er.ProducerId, er.SubsidiaryId, orgDetails, isTradingName) ?? CommonConstants.Hyphen),
                    TradingName = oddLeft == null ? CommonConstants.Hyphen :
                                (GetOrgOrTradingName(er.ProducerId, er.SubsidiaryId, orgDetails, !isTradingName) ?? CommonConstants.Hyphen),
                    LeaverCode = er.LeaverCode ?? CommonConstants.Hyphen,
                    ErrorCodeText = et.Name
                };

            var results = await query
                .AsNoTracking()
                .GroupBy(x => x.Id)
                .Select(g => g.First())
                .ToListAsync();

            return results;
        }

        private string? GetOrgOrTradingName(
                int producerId,
                string? subsidiaryId,
                List<CalculatorRunOrganisationDataDetail> orgDetails,
                bool isTradingName)
        {
            var detail = orgDetails
                .FirstOrDefault(x => x.OrganisationId == producerId && x.SubsidaryId == subsidiaryId);

            if (detail == null) return null;

            return isTradingName ? detail.TradingName : detail.OrganisationName;
        }
    }
}
