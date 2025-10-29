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

            var orgToSubs = BuildOrgToSubsDictionary(orgDetails);

            var errorReports = new List<ErrorReport>();

            foreach (var orgGroup in pomDetails.GroupBy(p => p.OrganisationId))
            {
                if (!orgGroup.Key.HasValue || !orgIds.Contains(orgGroup.Key.Value))
                {
                    // Org missing → add errors per unique org+sub
                    errorReports.AddRange(CreateErrorsForMissingOrg(orgGroup, calculatorRunId, createdBy));
                    continue;
                }

                var orgId = orgGroup.Key.Value;
                orgToSubs.TryGetValue(orgId, out var knownSubsForOrg);
                knownSubsForOrg ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var uniqueSubsInPoms = orgGroup
                    .Select(p => p.SubsidaryId)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var missingSubs = uniqueSubsInPoms
                                .Where(sub => sub != null && !knownSubsForOrg.Contains(sub!))
                                .ToList();

                if (missingSubs.Any())
                {
                    errorReports.Add(CreateOrgLevelError(orgId, calculatorRunId, createdBy));
                    var safeSubs = uniqueSubsInPoms
                        .Where(s => !string.IsNullOrWhiteSpace(s))!
                        .Select(s => s!) 
                        .ToList();

                    errorReports.AddRange(CreateSubsidiaryErrors(orgId, safeSubs, calculatorRunId, createdBy));
                }

            }

            var distinctErrors = errorReports
                .GroupBy(e => new { e.ProducerId, SubsidiaryId = e.SubsidiaryId ?? string.Empty })
                .Select(g => g.First())
                .ToList();

            if (distinctErrors.Any())
                await this.ErrorReportChunker.InsertRecords(distinctErrors);
        }

        private static Dictionary<int, HashSet<string>> BuildOrgToSubsDictionary(IEnumerable<CalculatorRunOrganisationDataDetail> orgDetails)
        {
            return orgDetails
                .GroupBy(o => o.OrganisationId.GetValueOrDefault())
                .ToDictionary(
                    g => g.Key,
                    g => new HashSet<string>(
                        g.Select(o => o.SubsidaryId)
                         .Where(s => !string.IsNullOrWhiteSpace(s))!,
                        StringComparer.OrdinalIgnoreCase));
        }

        private static IEnumerable<ErrorReport> CreateErrorsForMissingOrg(
            IGrouping<int?, CalculatorRunPomDataDetail> orgGroup,
            int calculatorRunId,
            string createdBy)
        {
            return orgGroup
                .GroupBy(p => new { p.OrganisationId, p.SubsidaryId })
                .Select(p => new ErrorReport
                {
                    CalculatorRunId = calculatorRunId,
                    ProducerId = p.Key.OrganisationId.GetValueOrDefault(),
                    SubsidiaryId = p.Key.SubsidaryId,
                    ErrorTypeId = (int)ErrorTypes.MISSINGREGISTRATIONDATA,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
        }

        private static ErrorReport CreateOrgLevelError(int orgId, int calculatorRunId, string createdBy)
        {
            return new ErrorReport
            {
                CalculatorRunId = calculatorRunId,
                ProducerId = orgId,
                SubsidiaryId = null,
                ErrorTypeId = (int)ErrorTypes.MISSINGREGISTRATIONDATA,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static IEnumerable<ErrorReport> CreateSubsidiaryErrors(
            int orgId,
            IEnumerable<string> subs,
            int calculatorRunId,
            string createdBy)
        {
            return subs.Select(sub => new ErrorReport
            {
                CalculatorRunId = calculatorRunId,
                ProducerId = orgId,
                SubsidiaryId = sub,
                ErrorTypeId = (int)ErrorTypes.MISSINGREGISTRATIONDATA,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
