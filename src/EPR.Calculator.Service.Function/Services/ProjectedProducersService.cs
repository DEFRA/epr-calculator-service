using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IProjectedProducersService
    {
        Task StoreProjectedProducers(int runId, List<CalcResultH2ProjectedProducer> h2ProjectedProducers);
    }

    public class ProjectedProducersService(
        ApplicationDBContext dbContext,
        IBulkOperations bulkOps)
        : IProjectedProducersService
    {
        public async Task StoreProjectedProducers(int runId, List<CalcResultH2ProjectedProducer> h2ProjectedProducers)
        {
            var existingAsProjected = await GetExistingAsProjected(runId);
            var missingProjected = MapH2ToProducerReportedMaterialProjected(existingAsProjected, h2ProjectedProducers);
            var allProjected = existingAsProjected.Concat(missingProjected).ToList();
            await bulkOps.BulkInsertAsync(dbContext, allProjected);
        }

        private async Task<List<ProducerReportedMaterialProjected>> GetExistingAsProjected(int runId) {
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
                    PackagingTonnage = (decimal)defaultRedTonnage,
                    PackagingTonnageRed = defaultRedTonnage,
                    PackagingTonnageAmber = 0,
                    PackagingTonnageGreen = 0,
                    PackagingTonnageRedMedical = 0,
                    PackagingTonnageAmberMedical = 0,
                    PackagingTonnageGreenMedical = 0
                };
            }

            bool HasDefaultedRedTonnage(CalcResultH2ProjectedProducerMaterialTonnage projectedTonnageByMaterial) =>
                projectedTonnageByMaterial.HouseholdTonnageDefaultedRed > 0 || projectedTonnageByMaterial.PublicBinTonnageDefaultedRed > 0 || projectedTonnageByMaterial.HouseholdDrinksContainerDefaultedRed > 0;

            var existingLookup = existingAsProjected.ToLookup(x => new { x.ProducerDetail!.ProducerId, x.ProducerDetail.SubsidiaryId, x.Material!.Code });

            return projectedH2Producers
                .Where(p => !p.IsSubtotal)
                .SelectMany(projectedProducer => projectedProducer.ProjectedTonnageByMaterial.Where(p => HasDefaultedRedTonnage(p.Value))
                    .SelectMany(materialTonnage => {
                        var key = new { projectedProducer.ProducerId, projectedProducer.SubsidiaryId, Code = materialTonnage.Key };
                        var maybeExisting = existingLookup[key].FirstOrDefault();

                        return maybeExisting != null ? new List<ProducerReportedMaterialProjected?>
                        {
                            GetMaybeNewProjected(maybeExisting.ProducerDetailId, maybeExisting.MaterialId, projectedProducer.SubmissionPeriodCode, PackagingTypes.Household, materialTonnage.Value.HouseholdTonnageDefaultedRed),
                            GetMaybeNewProjected(maybeExisting.ProducerDetailId, maybeExisting.MaterialId, projectedProducer.SubmissionPeriodCode, PackagingTypes.PublicBin, materialTonnage.Value.PublicBinTonnageDefaultedRed),
                            GetMaybeNewProjected(maybeExisting.ProducerDetailId, maybeExisting.MaterialId, projectedProducer.SubmissionPeriodCode, PackagingTypes.HouseholdDrinksContainers, materialTonnage.Value.HouseholdDrinksContainerDefaultedRed)
                        }.Where(p => p != null).Select(p => p!) : new List<ProducerReportedMaterialProjected>();
                    }
                )
            ).ToList();
        }
    }
}