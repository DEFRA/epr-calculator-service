using Azure;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace EPR.Calculator.Service.Function.PreValidator
{
    public interface ICommand
    {
    }

    public class ErrorReport
    {
        public int ProducerId { get; set; }
        public string SubsidaryId { get; set; }
        public int CalculatorRunId { get; set; }
        public string LeaverCode { get; set; }
        public int ErrorTypeId { get; set; }
        public string SubmitterOrgId { get; set; }
    }

    public class PreValidationsCommand : ICommand
    {
        public int CalculatorRunId { set; get; }
    }

    public class PreValidationCommand : PreValidationsCommand
    {
        public IEnumerable<CalculatorRunOrganisationDataDetail> RunOrganisations { set; get; } = [];
        public IEnumerable<CalculatorRunPomDataDetail> RunPoms { set; get; } = [];
    }

    public class CommandResponse
    {
        public CommandResponse(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; }
        public IEnumerable<CalculatorRunOrganisationDataDetail> RunOrganisations { set; get; } = [];
        public IEnumerable<CalculatorRunPomDataDetail> RunPoms { set; get; } = [];
        public IEnumerable<ErrorReport> ErrorReports { set; get; } = [];
    }

    public interface ICommandHandler<TCommand, TCommandResponse>
    {
        Task<TCommandResponse> ExecuteAsync(TCommand command);
    }

    public interface IPreValidationCommandHandlerFactory<PreValidationCommand, CommandResponse>
    {
        Queue<ICommandHandler<PreValidationCommand, CommandResponse>> Create();
    }

    public class PreValidationsCommandHandler : ICommandHandler<PreValidationsCommand, CommandResponse>
    {
        private readonly IPreValidationCommandHandlerFactory<PreValidationCommand, CommandResponse> _factory;
        private readonly ApplicationDBContext _dbContext;
        public PreValidationsCommandHandler(IPreValidationCommandHandlerFactory<PreValidationCommand, CommandResponse> factory, ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
            _factory = factory;
        }

        public async Task<CommandResponse> ExecuteAsync(PreValidationsCommand validationsCommand)
        {
            IEnumerable<CalculatorRunOrganisationDataDetail> runOrganisations = await (from run in this._dbContext.CalculatorRuns.AsNoTracking()
                                                                                       join orgMaster in this._dbContext.CalculatorRunOrganisationDataMaster.AsNoTracking() on run.CalculatorRunOrganisationDataMasterId equals orgMaster.Id
                                                                                       join orgDetail in this._dbContext.CalculatorRunOrganisationDataDetails.AsNoTracking() on orgMaster.Id equals orgDetail.CalculatorRunOrganisationDataMasterId
                                                                                       where run.Id == validationsCommand.CalculatorRunId
                                                                                       select orgDetail).ToListAsync();

            IEnumerable<CalculatorRunPomDataDetail> runPoms = await (from run in this._dbContext.CalculatorRuns.AsNoTracking()
                                                                     join pomMaster in this._dbContext.CalculatorRunPomDataMaster.AsNoTracking() on run.CalculatorRunPomDataMasterId equals pomMaster.Id
                                                                     join pomDetail in this._dbContext.CalculatorRunPomDataDetails.AsNoTracking() on pomMaster.Id equals pomDetail.CalculatorRunPomDataMasterId
                                                                     where run.Id == validationsCommand.CalculatorRunId
                                                                     select pomDetail).ToListAsync();

            var commandQueue = _factory.Create();
            var errorReports = new List<ErrorReport>();
            while (commandQueue.Count > 0)
            {
                var handlerCommand = commandQueue.Dequeue();
                var validationCommand = new PreValidationCommand
                {
                    CalculatorRunId = validationsCommand.CalculatorRunId,
                    RunOrganisations = runOrganisations,
                    RunPoms = runPoms
                };
                var response = await handlerCommand.ExecuteAsync(validationCommand);
                if (response != null && response.IsSuccess)
                {
                    runOrganisations = response.RunOrganisations;
                    runPoms = response.RunPoms;
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
    }

    public class MissingPomAndOrgValidationCommandHandler : ICommandHandler<PreValidationCommand, CommandResponse>
    {
        public Task<CommandResponse> ExecuteAsync(PreValidationCommand command)
        {
            throw new NotImplementedException();
        }
    }

    public class InsolvencyValidationCommandHandler : ICommandHandler<PreValidationCommand, CommandResponse>
    {
        public Task<CommandResponse> ExecuteAsync(PreValidationCommand command)
        {
            throw new NotImplementedException();
        }
    }

    public class PreValidationCommandHandlerFactory : IPreValidationCommandHandlerFactory<PreValidationCommand, CommandResponse>
    {
        public Queue<ICommandHandler<PreValidationCommand, CommandResponse>> Create()
        {
            var queue = new Queue<ICommandHandler<PreValidationCommand, CommandResponse>>();
            queue.Enqueue(new MissingPomAndOrgValidationCommandHandler());
            queue.Enqueue(new InsolvencyValidationCommandHandler());
            return queue;
        }
    }
}
