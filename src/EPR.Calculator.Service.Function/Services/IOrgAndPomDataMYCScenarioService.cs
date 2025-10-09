using System.Threading;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IOrgAndPomDataMYCScenarioService
    {
        Task HandleMYCScenarios(int calculatorRunId, CancellationToken cancellationToken);
    }
}
