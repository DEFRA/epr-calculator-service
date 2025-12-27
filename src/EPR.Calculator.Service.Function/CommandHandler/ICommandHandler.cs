using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.CommandHandler
{
    public interface ICommandHandler<TCommand, TCommandResponse>
    {
        Task<TCommandResponse> ExecuteAsync(TCommand command);
    }
}
