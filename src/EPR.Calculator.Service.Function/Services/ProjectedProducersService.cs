using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Interface;

namespace EPR.Calculator.Service.Function.Services
{
    // TODO rename this - not just projected producers
    // TODO Once this is persisted, everwhere needs to use this new data (and stop passing scaledUpProducers, partialObligations to GetReportedTonnage)
    public interface IProjectedProducersService
    {
        Task StoreProjectedProducers(int runId, List<ProducerDetail> producerDetails);
    }

    public class ProjectedProducersService : IProjectedProducersService
    {
        private readonly ApplicationDBContext dbContext;
        private IDbLoadingChunkerService<ProducerReportedMaterialProjected> chunker { get; init; }

        public ProjectedProducersService(ApplicationDBContext dbContext, IDbLoadingChunkerService<ProducerReportedMaterialProjected> chunker)
        {
            this.dbContext = dbContext;
            this.chunker = chunker;
        }

        public async Task StoreProjectedProducers(int runId, List<ProducerDetail> producerDetails)
        {
            var producerProjected =
                producerDetails.SelectMany(p =>
                    p.ProducerReportedMaterials.Select(rm =>
                        new ProducerReportedMaterialProjected
                        {
                            ProducerDetailId             = rm.ProducerDetailId,
                            MaterialId                   = rm.MaterialId,
                            SubmissionPeriod             = rm.SubmissionPeriod,
                            PackagingType                = rm.PackagingType,
                            PackagingTonnage             = rm.PackagingTonnage,
                            PackagingTonnageRed          = rm.PackagingTonnageRed,
                            PackagingTonnageAmber        = rm.PackagingTonnageAmber,
                            PackagingTonnageGreen        = rm.PackagingTonnageGreen,
                            PackagingTonnageRedMedical   = rm.PackagingTonnageRedMedical,
                            PackagingTonnageAmberMedical = rm.PackagingTonnageAmberMedical,
                            PackagingTonnageGreenMedical = rm.PackagingTonnageGreenMedical
                        }
                    )
                ).ToList();
            await chunker.InsertRecords(producerProjected);
        }
    }
}
