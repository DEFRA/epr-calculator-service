using EPR.Calculator.API.Data.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        Task<List<(int ProducerId, string? SubsidiaryId)>> HandleMissingRegistrationData(
            IEnumerable<CalculatorRunPomDataDetail> pomDetails,
            IEnumerable<CalculatorRunOrganisationDataDetail> orgDetails,
            int calculatorRunId,
            string createdBy,
            CancellationToken cancellationToken);

        Task<List<(int ProducerId, string? SubsidiaryId)>> HandleMissingPomData(
            IEnumerable<CalculatorRunPomDataDetail> pomDetails,
            IEnumerable<CalculatorRunOrganisationDataDetail> orgDetails,
            int calculatorRunId,
            string createdBy,
            CancellationToken cancellationToken);

    }

}
