using EPR.Calculator.API.Data;
using System.Threading;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Services
{
    public class OrgAndPomDataMYCScenarioService : IOrgAndPomDataMYCScenarioService
    {
        private readonly ApplicationDBContext context;

        public OrgAndPomDataMYCScenarioService(
            ApplicationDBContext context)
        {
            this.context = context;
        }

        public Task HandleMYCScenarios(int calculatorRunId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
