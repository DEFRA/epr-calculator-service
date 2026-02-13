using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.PreValidator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.CommandHandler
{
    public class PreValidationsCommandHandler : ICommandHandler<PreValidationsCommand, CommandResponse>
    {
        private readonly IPreValidationCommandHandlerFactory<PreValidationCommand, CommandResponse> _factory;
        private readonly ApplicationDBContext _dbContext;
        public PreValidationsCommandHandler(IPreValidationCommandHandlerFactory<PreValidationCommand, CommandResponse> factory,
            ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
            _factory = factory;
        }
        public async Task<CommandResponse> ExecuteAsync(PreValidationsCommand validationsCommand)
        {
            var calcRunId = validationsCommand.CalculatorRunId;

            var getOrgsTask = GetRunOrganisations(calcRunId);
            var getPomsTask = GetRunPoms(calcRunId);

            await Task.WhenAll(getOrgsTask, getPomsTask);

            IEnumerable<CalculatorRunOrganisationDataDetail> runOrganisations = getOrgsTask.Result;
            IEnumerable<CalculatorRunPomDataDetail> runPoms = getPomsTask.Result;

            var commandQueue = _factory.Create();
            var errorReports = new List<ErrorReport>();
            while (commandQueue.Count > 0)
            {
                var handlerCommandHandler = commandQueue.Dequeue();
                var validationCommand = new PreValidationCommand
                {
                    CalculatorRunId = validationsCommand.CalculatorRunId,
                    RunOrganisations = runOrganisations,
                    RunPoms = runPoms
                };
                var response = await handlerCommandHandler.ExecuteAsync(validationCommand);
                if (response != null && response.IsSuccess)
                {
                    runOrganisations = response.RunOrganisations; //.Where(x => x.IsValid);
                    runPoms = response.RunPoms; //.Where(x => x.IsValid);
                    errorReports.AddRange(response.ErrorReports);
                }
            }

            return new CommandResponse(true)
            {
                RunOrganisations = runOrganisations,
                RunPoms = runPoms,
                ErrorReports = errorReports
            };
        }

        private Task<List<CalculatorRunPomDataDetail>> GetRunPoms(int calcRunId)
        {
            return (from run in this._dbContext.CalculatorRuns.AsNoTracking()
                    join pomMaster in this._dbContext.CalculatorRunPomDataMaster.AsNoTracking() on run.CalculatorRunPomDataMasterId equals pomMaster.Id
                    join pomDetail in this._dbContext.CalculatorRunPomDataDetails.AsNoTracking() on pomMaster.Id equals pomDetail.CalculatorRunPomDataMasterId
                    where run.Id == calcRunId
                    select pomDetail).ToListAsync();
        }

        private Task<List<CalculatorRunOrganisationDataDetail>> GetRunOrganisations(int calcRunId)
        {
            return (from run in this._dbContext.CalculatorRuns.AsNoTracking()
                          join orgMaster in this._dbContext.CalculatorRunOrganisationDataMaster.AsNoTracking() on run.CalculatorRunOrganisationDataMasterId equals orgMaster.Id
                          join orgDetail in this._dbContext.CalculatorRunOrganisationDataDetails.AsNoTracking() on orgMaster.Id equals orgDetail.CalculatorRunOrganisationDataMasterId
                          where run.Id == calcRunId
                          select orgDetail).ToListAsync();
        }
    }
}
