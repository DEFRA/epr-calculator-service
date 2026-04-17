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
        dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
        var newProducerDetails = new List<ProducerDetail>();
        var newProducerReportedMaterials = new List<ProducerReportedMaterial>();

        var materials = await dbContext.Material.ToListAsync(cancellationToken);

        var calculatorRun = await dbContext.CalculatorRuns
            .Where(x => x.Id == runContext.RunId)
            .SingleAsync(cancellationToken);

        var calculatorRunOrgDataDetails = await dbContext.CalculatorRunOrganisationDataDetails
            .Where(x => x.CalculatorRunOrganisationDataMasterId == calculatorRun.CalculatorRunOrganisationDataMasterId)
            .ToListAsync(cancellationToken);

        var calculatorRunPomDataDetails = await dbContext.CalculatorRunPomDataDetails
            .Where(x => x.CalculatorRunPomDataMasterId == calculatorRun.CalculatorRunPomDataMasterId)
            .ToListAsync(cancellationToken);

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
            }).ToList();

        var organisationDataMaster = await dbContext.CalculatorRunOrganisationDataMaster
            .SingleAsync(x => x.Id == calculatorRun.CalculatorRunOrganisationDataMasterId, cancellationToken);

        var organisationDataDetails = calculatorRunOrgDataDetails
            .Where(odd => odd.CalculatorRunOrganisationDataMasterId == organisationDataMaster.Id && ObligationStates.IsObligated(odd.ObligationStatus))
            .OrderBy(odd => odd.OrganisationName)
            .GroupBy(odd => new { odd.OrganisationId, odd.SubsidiaryId, odd.SubmitterId })
            .Select(odd => odd.First())
            .ToList();

        // Get the calculator run pom data master record based on the CalculatorRunPomDataMasterId
        var pomDataMaster = await dbContext.CalculatorRunPomDataMaster
            .SingleAsync(x => x.Id == calculatorRun.CalculatorRunPomDataMasterId, cancellationToken);

        foreach (var organisation in organisationDataDetails.Where(t => !string.IsNullOrWhiteSpace(t.OrganisationName)))
        {
            // Initialize the producerReportedMaterials
            var producerReportedMaterials = new List<ProducerReportedMaterial>();

            // Get the calculator run pom data details related to the calculator run pom data master
            var runPomDataDetailsForSubsidaryId = calculatorRunPomDataDetails.Where
            (pdd => pdd.CalculatorRunPomDataMasterId == pomDataMaster.Id &&
                    pdd.OrganisationId == organisation.OrganisationId &&
                    pdd.SubsidiaryId == organisation.SubsidiaryId &&
                    pdd.SubmitterId == organisation.SubmitterId
            ).ToList();

            // Proceed further only if there is any pom data based on the pom data master id and organisation id
            // TO DO: We have to record if there is no pom data in a separate table post Dec 2024
            if (IsRunPomDataDetailsExistsForSubsidiaryId(runPomDataDetailsForSubsidaryId))
            {
                var organisations = organisationDataDetails
                    .Where(odd => odd.OrganisationName == organisation.OrganisationName && odd.SubsidiaryId == organisation.SubsidiaryId);

                // Get the producer based on the latest submission period
                var producer = organisations.FirstOrDefault();

                // Proceed further only if the organisation is not null and organisation id not null
                // TO DO: We have to record if the organisation name is null in a separate table post Dec 2024
                if (producer != null)
                {
                    var producerDetail = new ProducerDetail
                    {
                        CalculatorRunId = runContext.RunId,
                        ProducerId = producer.OrganisationId,
                        TradingName = organisation.TradingName,
                        SubsidiaryId = producer.SubsidiaryId,
                        ProducerName = GetLatestProducerName(producer.OrganisationId, producer.SubsidiaryId, calculatorRunOrgDataDetails),
                        CalculatorRun = calculatorRun
                    };

                    // Add producer detail record to the database context
                    newProducerDetails.Add(producerDetail);

                    foreach (var material in materials)
                    {
                        var pomDataDetailsByMaterial = runPomDataDetailsForSubsidaryId
                            .Where(pdd => pdd.PackagingMaterial == material.Code)
                            .GroupBy(pdd => pdd.PackagingType);

                        foreach (var pom in pomDataDetailsByMaterial)
                        {
                            var packagingType = pom.FirstOrDefault()?.PackagingType;
                            var submissionPeriod = pom.FirstOrDefault()?.SubmissionPeriod;
                            var totalPackagingMaterialWeight = pom.Sum(x => x.PackagingMaterialWeight) ?? 0;

                            // Proceed further only if the packaging type and packaging material weight is not null
                            // TO DO: We have to record if the packaging type or packaging material weight is null in a separate table post Dec 2024
                            if (IsPackagingTypeAndPackagingMaterialWeightExists(packagingType, totalPackagingMaterialWeight))
                            {
                                var producerReportedMaterial = new ProducerReportedMaterial
                                {
                                    MaterialId = material.Id,
                                    Material = material,
                                    ProducerDetail = producerDetail,
                                    PackagingType = packagingType!,
                                    SubmissionPeriod = submissionPeriod!,
                                    PackagingTonnage = Math.Round((decimal)totalPackagingMaterialWeight / 1000, 3),
                                    PackagingTonnageRed = Math.Round((decimal)(pom.Where(x => x.RamRagRating == RagRating.Red).Sum(x => x.PackagingMaterialWeight) ?? 0) / 1000, 3),
                                    PackagingTonnageAmber = Math.Round((decimal)(pom.Where(x => x.RamRagRating == RagRating.Amber).Sum(x => x.PackagingMaterialWeight) ?? 0) / 1000, 3),
                                    PackagingTonnageGreen = Math.Round((decimal)(pom.Where(x => x.RamRagRating == RagRating.Green).Sum(x => x.PackagingMaterialWeight) ?? 0) / 1000, 3),
                                    PackagingTonnageRedMedical = Math.Round((decimal)(pom.Where(x => x.RamRagRating == RagRating.RedMedical).Sum(x => x.PackagingMaterialWeight) ?? 0) / 1000, 3),
                                    PackagingTonnageAmberMedical = Math.Round((decimal)(pom.Where(x => x.RamRagRating == RagRating.AmberMedical).Sum(x => x.PackagingMaterialWeight) ?? 0) / 1000, 3),
                                    PackagingTonnageGreenMedical = Math.Round((decimal)(pom.Where(x => x.RamRagRating == RagRating.GreenMedical).Sum(x => x.PackagingMaterialWeight) ?? 0) / 1000, 3)
                                };

                                // Populate the producer reported material list
                                producerReportedMaterials.Add(producerReportedMaterial);
                            }
                        }
                    }

                    // Add the list of producer reported materials to the database context
                    newProducerReportedMaterials.AddRange(producerReportedMaterials);
                }
            }
        }

        logger.LogInformation("Transpose produced {ProducerDetailCount} producer details and {ReportedMaterialCount} reported materials",
            newProducerDetails.Count, newProducerReportedMaterials.Count);

        await bulkOps.BulkInsertAsync(dbContext, newProducerDetails, cancellationToken);
        await bulkOps.BulkInsertAsync(dbContext, newProducerReportedMaterials, cancellationToken);
    }

    private static bool IsPackagingTypeAndPackagingMaterialWeightExists(string? packagingType, double? totalPackagingMaterialWeight)
    {
        return packagingType != null && totalPackagingMaterialWeight != null;
    }

    private static bool IsRunPomDataDetailsExistsForSubsidiaryId(List<CalculatorRunPomDataDetail> runPomDataDetailsForSubsidaryId)
    {
        return runPomDataDetailsForSubsidaryId.Count > 0;
    }

    private static string? GetLatestProducerName(int orgId, string? subsidiaryId, List<CalculatorRunOrganisationDataDetail> organisationsList)
    {
        var organisation = organisationsList.Find(t => t.OrganisationId == orgId && t.SubsidiaryId == subsidiaryId);
        return organisation?.OrganisationName;
    }
}