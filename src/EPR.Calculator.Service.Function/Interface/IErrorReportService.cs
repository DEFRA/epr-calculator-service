using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.Service.Function.Interface
{
    public interface IErrorReportService
    {
        Task<HashSet<(int OrgId, string? SubId)>> HandleErrors(
                                IEnumerable<CalculatorRunPomDataDetail> pomDetails,
                                IEnumerable<CalculatorRunOrganisationDataDetail> orgDetails,
                                int calculatorRunId,
                                string createdBy,
                                RelativeYear relativeYear,
                                CancellationToken cancellationToken);
    }
}
