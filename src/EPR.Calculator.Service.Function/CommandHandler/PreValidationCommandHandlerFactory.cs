using EPR.Calculator.Service.Function.PreValidator;
using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.CommandHandler
{
    public class PreValidationCommandHandlerFactory : IPreValidationCommandHandlerFactory<PreValidationCommand, CommandResponse>
    {
        public Queue<ICommandHandler<PreValidationCommand, CommandResponse>> Create()
        {
            var queue = new Queue<ICommandHandler<PreValidationCommand, CommandResponse>>();
            return queue;
        }
    }
}
