using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Services
{
    public class ErrorReportService : IErrorReportService
    {
        private IDbLoadingChunkerService<ErrorReport> ErrorReportChunker { get; init; }

        public ErrorReportService(IDbLoadingChunkerService<ErrorReport> errorReportChunker)
        {
            ErrorReportChunker = errorReportChunker ?? throw new ArgumentNullException(nameof(errorReportChunker));
        }

        public async Task HandleUnmatchedPomAsync(
            IEnumerable<CalculatorRunPomDataDetail> pomDetails,
            IEnumerable<CalculatorRunOrganisationDataDetail> orgDetails,
            int calculatorRunId,
            string createdBy,
            CancellationToken cancellationToken)
        {
            if (pomDetails == null) throw new ArgumentNullException(nameof(pomDetails));
            if (orgDetails == null) throw new ArgumentNullException(nameof(orgDetails));

            var orgIds = new HashSet<int?>(
                orgDetails.Select(o => o.OrganisationId));

            var unmatched = pomDetails
                .Where(p => !orgIds.Contains(p.OrganisationId))
                .ToList();

            if (!unmatched.Any()) return;

            var errorReports = unmatched
                .Select(pom => new ErrorReport
                {
                    CalculatorRunId = calculatorRunId,
                    ProducerId = pom.OrganisationId.GetValueOrDefault(),
                    SubsidiaryId = pom.SubsidaryId,
                    ErrorTypeId = (int)ErrorTypes.UNKNOWN,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            await this.ErrorReportChunker.InsertRecords(errorReports);
        }
    }

}
