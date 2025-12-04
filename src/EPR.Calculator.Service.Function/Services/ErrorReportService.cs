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

        public List<ErrorReport> HandleMissingRegistrationData(
                                IEnumerable<CalculatorRunPomDataDetail> pomDetails,
                                IEnumerable<CalculatorRunOrganisationDataDetail> orgDetails,
                                int calculatorRunId,
                                string createdBy)
        {
            if (pomDetails == null) throw new ArgumentNullException(nameof(pomDetails));
            if (orgDetails == null) throw new ArgumentNullException(nameof(orgDetails));

            var errorReports = new List<ErrorReport>();

            var orgIds = orgDetails.Select(o => (o.OrganisationId, o.SubsidiaryId, o.SubmitterId)).ToHashSet();
            var pomIds = pomDetails.Select(p => (p.OrganisationId ?? 0, p.SubsidiaryId, p.SubmitterId)).ToHashSet();

            var pomIdsMissingFromOrg = pomIds.Except(orgIds);

            foreach (var m in pomIdsMissingFromOrg)
            {
                var orgId = m.Item1;

                //To preserve current behaviour of recording org level error for org/subsiduary - under discussion to be removed
                if (m.SubsidiaryId != null && orgIds.Any(o => o.Item1 == orgId) && !errorReports.Any(e => e.ProducerId == orgId && e.SubsidiaryId == null))
                {
                    errorReports.Add(CreateError(orgId, null, calculatorRunId, createdBy, ErrorTypes.MissingRegistrationData));
                }

                errorReports.Add(CreateError(orgId, m.SubsidiaryId, calculatorRunId, createdBy, ErrorTypes.MissingRegistrationData));
            }

            return errorReports;
        }

        public List<ErrorReport> HandleMissingPomData(IEnumerable<CalculatorRunPomDataDetail> pomDetails, IEnumerable<CalculatorRunOrganisationDataDetail> orgDetails, int calculatorRunId, string createdBy)
        {
            if (pomDetails == null) throw new ArgumentNullException(nameof(pomDetails));
            if (orgDetails == null) throw new ArgumentNullException(nameof(orgDetails));

            string ProducerId(string? subsidiaryId, int? organisationId)
            {
                var orgId = organisationId ?? 0;
                return subsidiaryId ?? orgId.ToString();
            }

            var errorReports = new List<ErrorReport>();

            var obligatedOrgIds = orgDetails.Where(x => ObligationStates.IsObligated(x.ObligationStatus)).Select(o => (o.OrganisationId, o.SubsidiaryId, o.SubmitterId)).ToHashSet();
            var pomIds = pomDetails.Select(p => (p.OrganisationId ?? 0, p.SubsidiaryId, p.SubmitterId)).ToHashSet();
            var producerIds = pomIds.Select(p => ProducerId(p.SubsidiaryId, p.Item1));

            var missingPoms = obligatedOrgIds.Except(pomIds).ToList();

            foreach (var pom in missingPoms)
            {
                var orgId = pom.Item1;
                string leaverCode = orgDetails.FirstOrDefault(o => o.OrganisationId == orgId &&
                                           o.SubsidiaryId == pom.SubsidiaryId &&
                                           o.SubmitterId == pom.SubmitterId)?.StatusCode ?? string.Empty;

                if (producerIds.Any(p => p == ProducerId(pom.SubsidiaryId, orgId)))
                {
                    errorReports.Add(CreateError(orgId, pom.SubsidiaryId, calculatorRunId, createdBy, ErrorTypes.MissingPOMData, leaverCode.ToString()));
                }
            }
            
            return errorReports;
        }

        public async Task<HashSet<(int OrgId, string? SubId)>> HandleErrors(
                                IEnumerable<CalculatorRunPomDataDetail> pomDetails,
                                IEnumerable<CalculatorRunOrganisationDataDetail> orgDetails,
                                int calculatorRunId,
                                string createdBy,
                                CancellationToken cancellationToken)
        {
            var missingRegErrors = HandleMissingRegistrationData(pomDetails, orgDetails, calculatorRunId, createdBy);
            var missingPomErrors = HandleMissingPomData(pomDetails, orgDetails, calculatorRunId, createdBy);

            var allErrors = missingRegErrors.Concat(missingPomErrors);
            await this.ErrorReportChunker.InsertRecords(allErrors);

             return allErrors.Select(e => (e.ProducerId, e.SubsidiaryId)).ToHashSet();
        }

        private ErrorReport CreateError(int orgId, string? subId, int calculatorRunId, string createdBy, ErrorTypes errorType, string leaverCode = "")
        {
            return new ErrorReport
            {
                CalculatorRunId = calculatorRunId,
                ProducerId = orgId,
                SubsidiaryId = subId,
                ErrorTypeId = (int)errorType,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                LeaverCode = leaverCode
            };
        }
    }
}
