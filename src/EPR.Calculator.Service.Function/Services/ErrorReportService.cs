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
            return pomDetails
                .DistinctBy(x => (x.OrganisationId, x.SubsidiaryId, x.SubmitterId))
                .GroupBy(x => x.OrganisationId)
                .SelectMany(group =>
                {
                    var reg = orgDetails.Where(p => p.OrganisationId == group.Key);
                    var missing = group.Any(o => !reg.Any(p => p.SubsidiaryId == o.SubsidiaryId && p.SubmitterId == o.SubmitterId));

                    return missing
                        ? group.Select(x => CreateError(x.OrganisationId ?? 0, x.SubsidiaryId, calculatorRunId, createdBy, ErrorCodes.MissingRegistrationData, leaverCode: null))
                        : Enumerable.Empty<ErrorReport>();
                })
                .ToList();
        }

        public List<ErrorReport> HandleMissingPomData(IEnumerable<CalculatorRunPomDataDetail> pomDetails, IEnumerable<CalculatorRunOrganisationDataDetail> orgDetails, int calculatorRunId, string createdBy)
        {
            if (pomDetails == null) throw new ArgumentNullException(nameof(pomDetails));
            if (orgDetails == null) throw new ArgumentNullException(nameof(orgDetails));

            var distinctPoms = pomDetails.DistinctBy(x => (x.OrganisationId, x.SubsidiaryId, x.SubmitterId));

            return orgDetails
                .Where(o => ObligationStates.IsObligated(o.ObligationStatus))
                .Where(o => !distinctPoms.Any(p => p.OrganisationId == o.OrganisationId && p.SubsidiaryId == o.SubsidiaryId && p.SubmitterId == o.SubmitterId))
                .SelectMany(reg =>
                    // Check whether this reg 'should have pom' by seeing if they have previously submitted under a different entity
                    distinctPoms.Any(p => (reg.SubsidiaryId ?? reg.OrganisationId.ToString()) == (p.SubsidiaryId ?? p.OrganisationId?.ToString()))
                        ? new [] { CreateError(reg.OrganisationId, reg.SubsidiaryId, calculatorRunId, createdBy, ErrorCodes.MissingPOMData, reg.StatusCode) }
                        : Array.Empty<ErrorReport>()
                ).ToList();
        }

        public List<ErrorReport> HandleObligatedErrors(IEnumerable<CalculatorRunOrganisationDataDetail> orgDetails, int calculatorRunId, string createdBy)
        {
            if (orgDetails == null) throw new ArgumentNullException(nameof(orgDetails));

            var obligatedErrors = orgDetails
                                    .Where(x => x.ObligationStatus == ObligationStates.Error)
                                    .Select(x => CreateError(x.OrganisationId, x.SubsidiaryId, calculatorRunId, createdBy, x.ErrorCode, leaverCode: x.StatusCode));

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
                                    .Where(x => !x.Any(y => string.IsNullOrEmpty(y.SubsidiaryId)))
                                    .Select(x => CreateError(x.Key, null, calculatorRunId, createdBy, ErrorCodes.Empty, leaverCode: null));

            var allErrors = calcErrors.Concat(holdingRegErrors);

            await this.ErrorReportChunker.InsertRecords(allErrors);

            return allErrors.Select(e => (e.ProducerId, e.SubsidiaryId)).ToHashSet();
        }

        private ErrorReport CreateError(int orgId, string? subId, int calculatorRunId, string createdBy, string? errorCode, string? leaverCode)
        {
            return new ErrorReport
            {
                CalculatorRunId = calculatorRunId,
                ProducerId = orgId,
                SubsidiaryId = subId,
                ErrorCode = errorCode ?? string.Empty,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                LeaverCode = leaverCode ?? string.Empty
            };
        }
    }
}
