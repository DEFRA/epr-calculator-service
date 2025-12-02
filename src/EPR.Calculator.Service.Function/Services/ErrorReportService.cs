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

            var orgIds = orgDetails.Select(o => (o.OrganisationId, o.SubsidaryId, o.SubmitterId)).ToHashSet();
            var pomIds = pomDetails.Select(p => (p.OrganisationId, p.SubsidaryId, p.SubmitterId)).ToHashSet();

            var pomIdsMissingFromOrg = pomIds.Except(orgIds);

            foreach (var m in pomIdsMissingFromOrg)
            {
                var orgId = m.OrganisationId ?? 0;

                //To preserve current behaviour of recording org level error for org/subsiduary - under discussion to be removed
                if (m.SubsidaryId != null && orgIds.Any(o => o.Item1 == orgId) && !errorReports.Any(e => e.ProducerId == orgId && e.SubsidiaryId == null))
                {
                    errorReports.Add(CreateError(orgId, null, calculatorRunId, createdBy, ErrorTypes.MissingRegistrationData));
                }

                errorReports.Add(CreateError(orgId, m.SubsidaryId, calculatorRunId, createdBy, ErrorTypes.MissingRegistrationData));
            }

            return errorReports;
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

        public List<ErrorReport> HandleMissingPomData(IEnumerable<CalculatorRunPomDataDetail> pomDetails, IEnumerable<CalculatorRunOrganisationDataDetail> orgDetails, int calculatorRunId, string createdBy)
        {
            if (pomDetails == null) throw new ArgumentNullException(nameof(pomDetails));
            if (orgDetails == null) throw new ArgumentNullException(nameof(orgDetails));

            var errorReports = new List<ErrorReport>();

            var obligatedOrgIds = orgDetails.Where(x => ObligationStates.IsObligated(x.ObligationStatus)).Select(o => (o.OrganisationId, o.SubsidaryId, o.SubmitterId)).ToHashSet();
            var pomIds = pomDetails.Select(p => (p.OrganisationId, p.SubsidaryId, p.SubmitterId)).ToHashSet();

            var missingPoms = obligatedOrgIds.Except(pomIds).ToList();

            foreach (var pom in missingPoms)
            {
                string leaverCode = orgDetails.FirstOrDefault(o => o.OrganisationId == pom.OrganisationId &&
                                           o.SubsidaryId == pom.SubsidaryId &&
                                           o.SubmitterId == pom.SubmitterId)?.StatusCode ?? string.Empty;

                if (pomIds.Any(x => x.OrganisationId.ToString() == pom.SubsidaryId) && leaverCode == OrganisationStatusCodes.IndividualJoiningGroup)
                {
                    errorReports.Add(CreateError(pom.OrganisationId ?? 0, pom.SubsidaryId, calculatorRunId, createdBy, ErrorTypes.MissingPOMData, leaverCode.ToString()));
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
            var missingRegErrors = HandleMissingRegistrationData(pomDetails,
                                                orgDetails,
                                                calculatorRunId,
                                                createdBy);


            var missingPomErrors = HandleMissingPomData(pomDetails,
                                                orgDetails,
                                                calculatorRunId,
                                                createdBy);

            var allErrors = missingRegErrors.Concat(missingPomErrors);
            await this.ErrorReportChunker.InsertRecords(allErrors);

             return allErrors.Select(e => (e.ProducerId, e.SubsidiaryId)).ToHashSet();
        }
    }
}
