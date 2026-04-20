using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    public class ParameterService : IParameterService
    {
        private readonly ApplicationDBContext dbContext;

        public ParameterService(IDbContextFactory<ApplicationDBContext> context)
        {
            this.dbContext = context.CreateDbContext();
        }

        public async Task<Dictionary<string, decimal>> GetDefaultParameters(int runId)
        {
            return await (
                from run in dbContext.CalculatorRuns
                join defaultMaster in dbContext.DefaultParameterSettings on run.DefaultParameterSettingMasterId equals
                    defaultMaster.Id
                join defaultDetail in dbContext.DefaultParameterSettingDetail on defaultMaster.Id equals defaultDetail
                    .DefaultParameterSettingMasterId
                join defaultTemplate in dbContext.DefaultParameterTemplateMasterList on defaultDetail
                    .ParameterUniqueReferenceId equals defaultTemplate.ParameterUniqueReferenceId
                where run.Id == runId
                select new { Key = defaultDetail.ParameterUniqueReferenceId, Value = defaultDetail.ParameterValue }
            ).ToDictionaryAsync(pair => pair.Key, pair => pair.Value);
        }
    }
}
