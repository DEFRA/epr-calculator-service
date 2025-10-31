using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Analytics.Synapse.Artifacts.Models;
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
            var errorReports = await context.ErrorReports.AsNoTracking().Where(x => x.CalculatorRunId == resultsRequestDto.RunId).ToListAsync();

            var orgDetails = await  (from run in context.CalculatorRuns.AsNoTracking()
                join odm in context.CalculatorRunOrganisationDataMaster
                    on run.CalculatorRunOrganisationDataMasterId equals odm.Id
                join odd in context.CalculatorRunOrganisationDataDetails on odm.Id equals odd.CalculatorRunOrganisationDataMasterId
                                               where run.Id == resultsRequestDto.RunId
                select odd).ToListAsync();

            var result = new List<CalcResultErrorReport>();
            foreach (var er in errorReports)
            {
                var calcResultErrorReport = new CalcResultErrorReport
                {
                    Id = er.Id,
                    ProducerId = er.ProducerId,
                    SubsidiaryId = er.SubsidiaryId ?? CommonConstants.Hyphen,
                    LeaverCode = er.LeaverCode ?? CommonConstants.Hyphen,
                    ProducerName = string.Empty,
                    TradingName = string.Empty,
                    ErrorCodeText = string.Empty
                };
                var producer = GetProducer(er, orgDetails);
                calcResultErrorReport.ProducerName = producer?.OrganisationName ?? CommonConstants.Hyphen;
                calcResultErrorReport.TradingName = producer?.TradingName ?? CommonConstants.Hyphen;
                result.Add(calcResultErrorReport);
            }

            return result;
        }

        private CalculatorRunOrganisationDataDetail GetProducer(API.Data.DataModels.ErrorReport errorReport, IEnumerable<CalculatorRunOrganisationDataDetail> orgDetails)
        {
            if (errorReport.SubsidiaryId == null)
            {
                return orgDetails.FirstOrDefault(x => x.OrganisationId == errorReport.ProducerId && x.SubsidaryId == null);
            }
            else
            {
                return orgDetails.FirstOrDefault(x => x.OrganisationId == errorReport.ProducerId && x.SubsidaryId == errorReport.SubsidiaryId);
            }
        }
    }
}