using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.CommandHandler
{
    public interface IPreValidationCommandHandlerFactory<PreValidationCommand, CommandResponse>
    {
        Queue<ICommandHandler<PreValidationCommand, CommandResponse>> Create();
    }
}
