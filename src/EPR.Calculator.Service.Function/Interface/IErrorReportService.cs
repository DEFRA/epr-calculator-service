using EPR.Calculator.API.Data.DataModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Interface
{
    public interface IErrorReportService
    {
        Task<HashSet<(int OrgId, string? SubId)>> HandleErrors(
                                IEnumerable<CalculatorRunPomDataDetail> pomDetails,
                                IEnumerable<CalculatorRunOrganisationDataDetail> orgDetails,
                                int calculatorRunId,
                                string createdBy,
                                CancellationToken cancellationToken);
    }
}
