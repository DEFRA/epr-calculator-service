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

            var orgIds = new HashSet<int>(orgDetails.Select(o => o.OrganisationId).OfType<int>());

            var unmatchedPoms = pomDetails
                .Where(p =>
                {
                    bool producerMissing = !p.OrganisationId.HasValue || !orgIds.Contains(p.OrganisationId.Value);

                    bool subsidiaryMissing = !string.IsNullOrWhiteSpace(p.SubsidaryId) &&
                        !orgDetails.Any(o =>
                            o.OrganisationId == p.OrganisationId &&
                            string.Equals(o.SubsidaryId, p.SubsidaryId, StringComparison.OrdinalIgnoreCase)
                        );

                    return producerMissing || subsidiaryMissing;
                })
                .GroupBy(p => new { p.OrganisationId, p.SubsidaryId })
                .Select(g => g.First())
                .ToList();

            if (!unmatchedPoms.Any()) return;

            var errorReports = unmatchedPoms
                .Select(pom => new ErrorReport
                {
                    CalculatorRunId = calculatorRunId,
                    ProducerId = pom.OrganisationId.GetValueOrDefault(),
                    SubsidiaryId = pom.SubsidaryId,
                    ErrorTypeId = (int)ErrorTypes.MISSINGREGISTRATIONDATA,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            await this.ErrorReportChunker.InsertRecords(errorReports);
        }
    }

}
