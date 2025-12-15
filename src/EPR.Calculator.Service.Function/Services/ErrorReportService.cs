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

            var pomIdsMissingFromReg = pomIds.Except(orgIds);

            foreach (var reg in pomIdsMissingFromReg)
            {
                var orgId = reg.Item1;

                //To preserve current behaviour of recording org level error for org/subsiduary - under discussion to be removed
                if (reg.SubsidiaryId != null && orgIds.Any(o => o.Item1 == orgId) && !errorReports.Exists(e => e.ProducerId == orgId && e.SubsidiaryId == null))
                {
                    errorReports.Add(CreateError(orgId, null, calculatorRunId, createdBy, ErrorTypes.MissingRegistrationData));
                }

                errorReports.Add(CreateError(orgId, reg.SubsidiaryId, calculatorRunId, createdBy, ErrorTypes.MissingRegistrationData));
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

            var regsWithMissingPoms = obligatedOrgIds.Except(pomIds).ToList();

            foreach (var reg in regsWithMissingPoms)
            {
                var orgId = reg.Item1;
                string leaverCode = orgDetails.FirstOrDefault(o => o.OrganisationId == orgId &&
                                           o.SubsidiaryId == reg.SubsidiaryId &&
                                           o.SubmitterId == reg.SubmitterId)?.StatusCode ?? string.Empty;

                // Check whether this reg 'should have pom' by seeing if they have previously submitted under a different entity
                if (producerIds.Any(p => p == ProducerId(reg.SubsidiaryId, orgId)))
                {
                    errorReports.Add(CreateError(orgId, reg.SubsidiaryId, calculatorRunId, createdBy, ErrorCodes.MissingPOMData, leaverCode.ToString()));
                }
            }

            return errorReports;
        }

        public List<ErrorReport> HandleObligatedErrors(IEnumerable<CalculatorRunOrganisationDataDetail> orgDetails, int calculatorRunId, string createdBy)
        {
            if (orgDetails == null) throw new ArgumentNullException(nameof(orgDetails));

            var obligatedErrors = orgDetails
                                    .Where(x => x.ObligationStatus == ObligationStates.Error)
                                    .Select(x => CreateError(x.OrganisationId, x.SubsidiaryId, calculatorRunId, createdBy, x.ErrorCode ?? string.Empty));

            return obligatedErrors.ToList();
        }

        public async Task<HashSet<(int OrgId, string? SubId)>> HandleErrors(
                                IEnumerable<CalculatorRunPomDataDetail> pomDetails,
                                IEnumerable<CalculatorRunOrganisationDataDetail> orgDetails,
                                int calculatorRunId,
                                string createdBy,
                                CancellationToken cancellationToken)
        {
            var obligatedErrors = HandleObligatedErrors(orgDetails, calculatorRunId, createdBy);
            var missingRegErrors = HandleMissingRegistrationData(pomDetails, orgDetails, calculatorRunId, createdBy);
            var missingPomErrors = HandleMissingPomData(pomDetails, orgDetails, calculatorRunId, createdBy);

            var calcErrors = obligatedErrors.Concat(missingRegErrors).Concat(missingPomErrors);

            var holdingRegErrors = calcErrors
                                    .GroupBy(x => x.ProducerId)
                                    .Select(x => CreateError(x.Key, null, calculatorRunId, createdBy, ErrorCodes.Empty));

            var allErrors = calcErrors.Concat(holdingRegErrors);

            await this.ErrorReportChunker.InsertRecords(allErrors);

            return allErrors.Select(e => (e.ProducerId, e.SubsidiaryId)).ToHashSet();
        }

        private ErrorReport CreateError(int orgId, string? subId, int calculatorRunId, string createdBy, string errorCode, string leaverCode = "")
        {
            return new ErrorReport
            {
                CalculatorRunId = calculatorRunId,
                ProducerId = orgId,
                SubsidiaryId = subId,
                ErrorCode = errorCode,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                LeaverCode = leaverCode
            };
        }
    }
}
