using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services;

public interface IProjectedProducersService
{
    Task StoreProjectedProducers(int runId, List<CalcResultH2ProjectedProducer> h2ProjectedProducers, List<CalcResultH1ProjectedProducer> h1ProjectedProducers);
}

public class ProjectedProducersService(
    ApplicationDBContext dbContext,
    IBulkOperations bulkOps)
    : IProjectedProducersService
{
    public async Task StoreProjectedProducers(int runId, List<CalcResultH2ProjectedProducer> h2ProjectedProducers, List<CalcResultH1ProjectedProducer> h1ProjectedProducers)
    {
        var existingAsProjected = await GetExistingAsProjected(runId);
        var missingH2Projected = MapH2ToProducerReportedMaterialProjected(existingAsProjected, h2ProjectedProducers);
        var missingH1Projected = MapH1ToProducerReportedMaterialProjected(existingAsProjected, h1ProjectedProducers);
        var allProjected = existingAsProjected.Concat(missingH2Projected).Concat(missingH1Projected).ToList();
        await bulkOps.BulkInsertAsync(dbContext, allProjected);
    }

    private async Task<List<ProducerReportedMaterialProjected>> GetExistingAsProjected(int runId)
    {
        var existing = await dbContext.ProducerReportedMaterial
            .Include(prm => prm.ProducerDetail)
            .Include(prm => prm.Material)
            .Where(prm => prm.ProducerDetail!.CalculatorRunId == runId)
            .ToListAsync();

        return existing.Select(x => new ProducerReportedMaterialProjected
        {
            ProducerDetailId = x.ProducerDetailId,
            MaterialId = x.MaterialId,
            SubmissionPeriod = x.SubmissionPeriod,
            PackagingType = x.PackagingType,
            PackagingTonnage = x.PackagingTonnage,
            PackagingTonnageRed = x.PackagingTonnageRed,
            PackagingTonnageAmber = x.PackagingTonnageAmber,
            PackagingTonnageGreen = x.PackagingTonnageGreen,
            PackagingTonnageRedMedical = x.PackagingTonnageRedMedical,
            PackagingTonnageAmberMedical = x.PackagingTonnageAmberMedical,
            PackagingTonnageGreenMedical = x.PackagingTonnageGreenMedical,
            ProducerDetail = x.ProducerDetail,
            Material = x.Material
        }).ToList();
    }

    private List<ProducerReportedMaterialProjected> MapH2ToProducerReportedMaterialProjected(List<ProducerReportedMaterialProjected> existingAsProjected, List<CalcResultH2ProjectedProducer> projectedH2Producers)
    {
        ProducerReportedMaterialProjected? GetMaybeNewProjected(int producerDetailId, int materialId, string submissionPeriod, string packagingType, decimal? defaultRedTonnage)
        {
            if (defaultRedTonnage == null || defaultRedTonnage <= 0) return null;

            return new ProducerReportedMaterialProjected
            {
                ProducerDetailId = producerDetailId,
                MaterialId = materialId,
                SubmissionPeriod = submissionPeriod,
                PackagingType = packagingType,
                PackagingTonnage = 0,
                PackagingTonnageRed = defaultRedTonnage,
                PackagingTonnageAmber = 0,
                PackagingTonnageGreen = 0,
                PackagingTonnageRedMedical = 0,
                PackagingTonnageAmberMedical = 0,
                PackagingTonnageGreenMedical = 0
            };
        }

        bool HasDefaultedRedTonnage(CalcResultH2ProjectedProducerMaterialTonnage projectedTonnageByMaterial) =>
            projectedTonnageByMaterial.HouseholdTonnageWithoutRAM > 0 || projectedTonnageByMaterial.PublicBinTonnageWithoutRAM > 0 || projectedTonnageByMaterial.HouseholdDrinksContainerTonnageWithoutRAM > 0;

        var existingLookup = existingAsProjected.ToLookup(x => new { x.ProducerDetail!.ProducerId, x.ProducerDetail.SubsidiaryId, x.Material!.Code });

        return projectedH2Producers
            .Where(p => !p.IsSubtotal)
            .SelectMany(projectedProducer => projectedProducer.H2ProjectedTonnageByMaterial.Where(p => HasDefaultedRedTonnage(p.Value))
                .SelectMany(materialTonnage =>
                    {
                        var key = new { projectedProducer.ProducerId, projectedProducer.SubsidiaryId, Code = materialTonnage.Key };
                        var maybeExisting = existingLookup[key].FirstOrDefault();

                        return maybeExisting != null
                            ? new List<ProducerReportedMaterialProjected?>
                            {
                                GetMaybeNewProjected(maybeExisting.ProducerDetailId, maybeExisting.MaterialId, projectedProducer.SubmissionPeriodCode, PackagingTypes.Household, materialTonnage.Value.HouseholdTonnageWithoutRAM),
                                GetMaybeNewProjected(maybeExisting.ProducerDetailId, maybeExisting.MaterialId, projectedProducer.SubmissionPeriodCode, PackagingTypes.PublicBin, materialTonnage.Value.PublicBinTonnageWithoutRAM),
                                GetMaybeNewProjected(maybeExisting.ProducerDetailId, maybeExisting.MaterialId, projectedProducer.SubmissionPeriodCode, PackagingTypes.HouseholdDrinksContainers, materialTonnage.Value.HouseholdDrinksContainerTonnageWithoutRAM)
                            }.Where(p => p != null).Select(p => p!)
                            : new List<ProducerReportedMaterialProjected>();
                    }
                )
            ).ToList();
    }

    private List<ProducerReportedMaterialProjected> MapH1ToProducerReportedMaterialProjected(List<ProducerReportedMaterialProjected> existingAsProjected, List<CalcResultH1ProjectedProducer> projectedH1Producers)
    {
        ProducerReportedMaterialProjected? GetMaybeNewProjected(int producerDetailId, int materialId, string submissionPeriod, string packagingType, decimal? tonnageWithoutRam, RAMTonnage? original, RAMTonnage? projected)
        {
            if (tonnageWithoutRam <= 0 || original == null || projected == null) return null;

            return new ProducerReportedMaterialProjected
            {
                ProducerDetailId = producerDetailId,
                MaterialId = materialId,
                SubmissionPeriod = submissionPeriod,
                PackagingType = packagingType,
                PackagingTonnage = 0,
                PackagingTonnageRed = projected.RedTonnage - original.RedTonnage,
                PackagingTonnageAmber = projected.AmberTonnage - original.AmberTonnage,
                PackagingTonnageGreen = projected.GreenTonnage - original.GreenTonnage,
                PackagingTonnageRedMedical = projected.RedMedicalTonnage - original.RedMedicalTonnage,
                PackagingTonnageAmberMedical = projected.AmberMedicalTonnage - original.AmberMedicalTonnage,
                PackagingTonnageGreenMedical = projected.GreenMedicalTonnage - original.GreenMedicalTonnage
            };
        }

        bool HasTonnageWithoutRam(CalcResultH1ProjectedProducerMaterialTonnage projectedTonnageByMaterial) =>
            projectedTonnageByMaterial.HouseholdTonnageWithoutRAM > 0 || projectedTonnageByMaterial.PublicBinTonnageWithoutRAM > 0 || projectedTonnageByMaterial.HouseholdDrinksContainerTonnageWithoutRAM > 0;

        var existingLookup = existingAsProjected.ToLookup(x => new { x.ProducerDetail!.ProducerId, x.ProducerDetail.SubsidiaryId, x.Material!.Code });

        return projectedH1Producers
            .Where(p => !p.IsSubtotal)
            .SelectMany(projectedProducer => projectedProducer.H1ProjectedTonnageByMaterial.Where(p => HasTonnageWithoutRam(p.Value))
                .SelectMany(materialTonnage =>
                    {
                        var key = new { projectedProducer.ProducerId, projectedProducer.SubsidiaryId, Code = materialTonnage.Key };
                        var maybeExisting = existingLookup[key].FirstOrDefault();

                        if (maybeExisting == null) return new List<ProducerReportedMaterialProjected>();

                        var maybeNewProjected = (string packagingType, decimal? tonnageWithoutRam, RAMTonnage? orgRam, RAMTonnage? projRam) =>
                            GetMaybeNewProjected(maybeExisting.ProducerDetailId, maybeExisting.MaterialId, projectedProducer.SubmissionPeriodCode, packagingType, tonnageWithoutRam, orgRam, projRam);

                        return new List<ProducerReportedMaterialProjected?>
                        {
                            maybeNewProjected(PackagingTypes.Household, materialTonnage.Value.HouseholdTonnageWithoutRAM, materialTonnage.Value.HouseholdRAMTonnage, materialTonnage.Value.ProjectedHouseholdRAMTonnage),
                            maybeNewProjected(PackagingTypes.PublicBin, materialTonnage.Value.PublicBinTonnageWithoutRAM, materialTonnage.Value.PublicBinRAMTonnage, materialTonnage.Value.ProjectedPublicBinRAMTonnage),
                            maybeNewProjected(PackagingTypes.HouseholdDrinksContainers, materialTonnage.Value.HouseholdDrinksContainerTonnageWithoutRAM, materialTonnage.Value.HouseholdDrinksContainerRAMTonnage, materialTonnage.Value.ProjectedHouseholdDrinksContainerRAMTonnage)
                        }.Where(p => p != null).Select(p => p!);
                    }
                )
            ).ToList();
    }
}
