using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<List<(int ProducerId, string? SubsidiaryId)>> HandleUnmatchedPomAsync(
                                IEnumerable<CalculatorRunPomDataDetail> pomDetails,
                                IEnumerable<CalculatorRunOrganisationDataDetail> orgDetails,
                                int calculatorRunId,
                                string createdBy,
                                CancellationToken cancellationToken)
        {
            if (pomDetails == null) throw new ArgumentNullException(nameof(pomDetails));
            if (orgDetails == null) throw new ArgumentNullException(nameof(orgDetails));

            var errorReports = new List<ErrorReport>();

            var orgIds = orgDetails.Select(o => (o.OrganisationId, o.SubsidaryId, o.SubmitterId)).ToHashSet();
            var pomIds = pomDetails.Select(p => (p.OrganisationId, p.SubsidaryId, p.SubmitterId)).ToHashSet();

            var pomIdsMissingFromOrg = pomIds.Except(orgIds); 

            foreach(var m in pomIdsMissingFromOrg) {
                var orgId = m.OrganisationId??0;

                //To preserve current behaviour of recording org level error for org/subsiduary - under discussion to be removed
                if(m.SubsidaryId != null && orgIds.Any(o => o.Item1 == orgId) && !errorReports.Any(e => e.ProducerId == orgId && e.SubsidiaryId == null)) {
                    errorReports.Add(CreateError(orgId, null, calculatorRunId, createdBy));
                }

                errorReports.Add(CreateError(orgId, m.SubsidaryId, calculatorRunId, createdBy));
            }

            if (!errorReports.Any())
                return new List<(int, string?)>();

            await this.ErrorReportChunker.InsertRecords(errorReports);

            return errorReports
                .Select(e => (e.ProducerId, e.SubsidiaryId))
                .ToList();
        }

        private ErrorReport CreateError(int orgId, string? subId, int calculatorRunId, string createdBy)
        {
            return new ErrorReport
            {
                CalculatorRunId = calculatorRunId,
                ProducerId = orgId,
                SubsidiaryId = subId,
                ErrorTypeId = (int) ErrorTypes.MissingRegistrationData,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
