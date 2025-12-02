using EPR.Calculator.API.Data.DataModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Interface
{
    public interface IErrorReportService
    {
        /// <summary>
        /// Finds POM records that don't have matching organisation records, creates ErrorReport entries,
        /// and inserts them (via the provided chunker/service).
        /// </summary>
        List<ErrorReport> HandleMissingRegistrationData(
            IEnumerable<CalculatorRunPomDataDetail> pomDetails,
            IEnumerable<CalculatorRunOrganisationDataDetail> orgDetails,
            int calculatorRunId,
            string createdBy);

        List<ErrorReport> HandleMissingPomData(
            IEnumerable<CalculatorRunPomDataDetail> pomDetails,
            IEnumerable<CalculatorRunOrganisationDataDetail> orgDetails,
            int calculatorRunId,
            string createdBy);

        Task<HashSet<(int OrgId, string? SubId)>> HandleErrors(
                                IEnumerable<CalculatorRunPomDataDetail> pomDetails,
                                IEnumerable<CalculatorRunOrganisationDataDetail> orgDetails,
                                int calculatorRunId,
                                string createdBy,
                                CancellationToken cancellationToken);
    }
}
