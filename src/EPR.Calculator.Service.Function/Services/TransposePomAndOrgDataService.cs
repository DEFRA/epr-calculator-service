using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.Services;

public interface ITransposePomAndOrgDataService
{
    Task Transpose(CalculatorRunContext runContext, CancellationToken cancellationToken);
}

/// <summary>
///     Service for transposing POM and organization data.
/// </summary>
[ExcludeFromCodeCoverage]
public class TransposePomAndOrgDataService(
    ApplicationDBContext dbContext,
    IBulkOperations bulkOps,
    IErrorReportService errorReportService,
    ILogger<TransposePomAndOrgDataService> logger)
    : ITransposePomAndOrgDataService
{
    [SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high")]
    public async Task Transpose(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        var calculatorRun = await dbContext.CalculatorRuns
            .AsNoTracking()
            .Where(x => x.Id == runContext.RunId
                        && x.CalculatorRunOrganisationDataMaster != null
                        && x.CalculatorRunPomDataMaster != null)
            .SingleAsync(cancellationToken);

        var materials = await dbContext.Material
            .AsNoTracking()
            .ToImmutableArrayAsync(cancellationToken);

        var calculatorRunOrgDataDetails = await dbContext.CalculatorRunOrganisationDataDetails
            .AsNoTracking()
            .Where(x => x.CalculatorRunOrganisationDataMasterId == calculatorRun.CalculatorRunOrganisationDataMasterId)
            .ToImmutableArrayAsync(cancellationToken);

        var calculatorRunPomDataDetails = await dbContext.CalculatorRunPomDataDetails
            .AsNoTracking()
            .Where(x => x.CalculatorRunPomDataMasterId == calculatorRun.CalculatorRunPomDataMasterId)
            .ToImmutableArrayAsync(cancellationToken);

        var unmatchedSet = await errorReportService.HandleErrors(
            calculatorRunPomDataDetails,
            calculatorRunOrgDataDetails,
            runContext.RunId,
            runContext.User,
            runContext.RelativeYear,
            cancellationToken);

        calculatorRunPomDataDetails = calculatorRunPomDataDetails
            .Where(p =>
            {
                var orgId = p.OrganisationId.GetValueOrDefault();
                var subId = p.SubsidiaryId;
                return !unmatchedSet.Contains((orgId, subId));
            })
            .ToImmutableArray();

        var organisationDataDetails = calculatorRunOrgDataDetails
            .Where(odd => ObligationStates.IsObligated(odd.ObligationStatus)
                && !string.IsNullOrWhiteSpace(odd.OrganisationName))
            .GroupBy(odd => new { odd.OrganisationId, odd.SubsidiaryId, odd.SubmitterId })
            .Select(odd => odd.OrderByDescending(o => o.HasH2).First())
            .ToImmutableArray();

        var newProducerDetails = new List<ProducerDetail>();

        foreach (var organisation in organisationDataDetails)
        {
            // Get the producer based on the latest submission period
            var producer = organisationDataDetails
                .First(odd => odd.OrganisationName == organisation.OrganisationName && odd.SubsidiaryId == organisation.SubsidiaryId);

            // Get the calculator run pom data details related to the calculator run pom data master
            var subsidiaryPomsByMaterial = calculatorRunPomDataDetails
                .Where(pdd =>
                    pdd.OrganisationId == organisation.OrganisationId &&
                    pdd.SubsidiaryId == organisation.SubsidiaryId &&
                    pdd.SubmitterId == organisation.SubmitterId)
                .GroupBy(pdd => pdd.PackagingMaterial!)
                .ToImmutableDictionary(grp => grp.Key,
                    grp => grp.ToImmutableArray(),
                    StringComparer.OrdinalIgnoreCase);

            if (subsidiaryPomsByMaterial.Count == 0)
                continue;

            // ⚠️ Only set scalar FK columns (e.g. CalculatorRunId, MaterialId) on the entities below.
            // Navigation properties to existing rows (CalculatorRun, Material) are intentionally left
            // unset so that the IncludeGraph bulk insert below does not try to re-insert them.
            var producerDetail = new ProducerDetail
            {
                CalculatorRunId = runContext.RunId,
                ProducerId = producer.OrganisationId,
                TradingName = organisation.TradingName,
                SubsidiaryId = producer.SubsidiaryId,
                ProducerName = producer.OrganisationName
            };

            foreach (var material in materials)
            {
                if(!subsidiaryPomsByMaterial.TryGetValue(material.Code, out var subsidiaryPomsForMaterial))
                    continue;

                foreach (var poms in subsidiaryPomsForMaterial.GroupBy(p => p.PackagingType))
                {
                    var packagingType = poms.First().PackagingType!;
                    var submissionPeriod = poms.First().SubmissionPeriod!;
                    var totalPackagingMaterialWeight = poms.Sum(x => x.PackagingMaterialWeight)!;

                    producerDetail.ProducerReportedMaterials.Add(new ProducerReportedMaterial
                    {
                        MaterialId = material.Id,
                        PackagingType = packagingType, // IncludeGraph in bulk insert below will propagate the parent's Id.
                        SubmissionPeriod = submissionPeriod,
                        PackagingTonnage = Math.Round((decimal)totalPackagingMaterialWeight / 1000, 3),
                        PackagingTonnageRed = Math.Round((decimal)(poms.Where(x => x.RamRagRating == RagRating.Red).Sum(x => x.PackagingMaterialWeight) ?? 0) / 1000, 3),
                        PackagingTonnageAmber = Math.Round((decimal)(poms.Where(x => x.RamRagRating == RagRating.Amber).Sum(x => x.PackagingMaterialWeight) ?? 0) / 1000, 3),
                        PackagingTonnageGreen = Math.Round((decimal)(poms.Where(x => x.RamRagRating == RagRating.Green).Sum(x => x.PackagingMaterialWeight) ?? 0) / 1000, 3),
                        PackagingTonnageRedMedical = Math.Round((decimal)(poms.Where(x => x.RamRagRating == RagRating.RedMedical).Sum(x => x.PackagingMaterialWeight) ?? 0) / 1000, 3),
                        PackagingTonnageAmberMedical = Math.Round((decimal)(poms.Where(x => x.RamRagRating == RagRating.AmberMedical).Sum(x => x.PackagingMaterialWeight) ?? 0) / 1000, 3),
                        PackagingTonnageGreenMedical = Math.Round((decimal)(poms.Where(x => x.RamRagRating == RagRating.GreenMedical).Sum(x => x.PackagingMaterialWeight) ?? 0) / 1000, 3)
                    });
                }
            }

            newProducerDetails.Add(producerDetail);
        }

        var totalReportedMaterials = newProducerDetails.Sum(p => p.ProducerReportedMaterials.Count);
        logger.LogInformation("Transpose produced {ProducerDetailCount} producer details and {ReportedMaterialCount} reported materials",
            newProducerDetails.Count, totalReportedMaterials);

        // Must include graph for EF navigation properties to be set on the inserted entities.
        await bulkOps.BulkInsertAsync(dbContext, newProducerDetails, cfg => cfg.IncludeGraph = true, cancellationToken);
    }
}
