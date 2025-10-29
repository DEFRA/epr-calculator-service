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

            // Build a lookup of organisation -> set of subsidiaryIds known for that organisation
            var orgToSubs = orgDetails
                .GroupBy(o => o.OrganisationId.GetValueOrDefault())
                .ToDictionary(
                    g => g.Key,
                    g => new HashSet<string>(
                        g.Select(o => o.SubsidaryId)
                         .Where(s => !string.IsNullOrWhiteSpace(s))!,
                        StringComparer.OrdinalIgnoreCase
                    )
                );

            var errorReports = new List<ErrorReport>();

            // Group incoming POMs by OrganisationId so we can apply org-level logic
            var pomsByOrg = pomDetails
                .GroupBy(p => p.OrganisationId)
                .ToList();

            foreach (var orgGroup in pomsByOrg)
            {
                var orgIdNullable = orgGroup.Key;
                var orgId = orgIdNullable.GetValueOrDefault();

                // Unique subsidiaries seen in POMs for this org (deduplicated)
                var uniqueSubsInPoms = orgGroup
                    .Select(p => p.SubsidaryId)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // Org does not exist at all -> everything for this org is an error
                if (!orgIdNullable.HasValue || !orgIds.Contains(orgId))
                {
                    // Add one error record per unique Org+Sub (deduplication)
                    var uniquePairs = orgGroup
                        .GroupBy(p => new { p.OrganisationId, p.SubsidaryId }, (key, grp) => new
                        {
                            OrganisationId = key.OrganisationId,
                            SubsidaryId = key.SubsidaryId
                        });

                    foreach (var pair in uniquePairs)
                    {
                        errorReports.Add(new ErrorReport
                        {
                            CalculatorRunId = calculatorRunId,
                            ProducerId = pair.OrganisationId.GetValueOrDefault(),
                            SubsidiaryId = pair.SubsidaryId,
                            ErrorTypeId = (int)ErrorTypes.MISSINGREGISTRATIONDATA,
                            CreatedBy = createdBy,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    continue;
                }

                // Org exists. Check whether any subsidiary in POMs for this org is missing under this org.
                orgToSubs.TryGetValue(orgIdNullable.GetValueOrDefault(), out var knownSubsForOrg);
                knownSubsForOrg ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // Subs that are present in POMs but NOT present under this org in orgDetails
                var missingSubs = uniqueSubsInPoms
                    .Where(sub => !knownSubsForOrg.Contains(sub))
                    .ToList();

                if (missingSubs.Any())
                {
                    // The org is considered problematic: add one org-level error (SubsidiaryId = null)
                    errorReports.Add(new ErrorReport
                    {
                        CalculatorRunId = calculatorRunId,
                        ProducerId = orgId,
                        SubsidiaryId = null,
                        ErrorTypeId = (int)ErrorTypes.MISSINGREGISTRATIONDATA,
                        CreatedBy = createdBy,
                        CreatedAt = DateTime.UtcNow
                    });

                    // Add one error per unique subsidiary seen in POMs for this org (including those that do exist)
                    foreach (var sub in uniqueSubsInPoms)
                    {
                        errorReports.Add(new ErrorReport
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

            var distinctErrors = errorReports
                .GroupBy(e => new { e.ProducerId, SubsidiaryId = e.SubsidiaryId ?? string.Empty })
                .Select(g => g.First())
                .ToList();

            if (!distinctErrors.Any()) return;

            await this.ErrorReportChunker.InsertRecords(distinctErrors);
        }
    }
}
