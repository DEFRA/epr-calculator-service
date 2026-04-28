using Microsoft.EntityFrameworkCore;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Services;
using static EPR.Calculator.Service.Function.Builder.LaDisposalCost.CalcRunLaDisposalCostBuilder;

namespace EPR.Calculator.Service.Function.Services
{
    public interface ILevelledProducerService
    {
        Task<List<ProducerReportedMaterialsForSubmissionPeriod>> GetProducers(int runId, List<MaterialDetail> materials);
    }

    /*
    public class ProducerReportedMaterialsForSubmissionPeriod
    {
        public int ProducerId { get; }
        public string? SubsidiaryId { get; }
        public string SubmissionPeriod { get; }
        public List<ProducerReportedMaterial> ReportedMaterials { get; }

        public ProducerReportedMaterialsForSubmissionPeriod(int producerId, string? subsidiaryId, string submissionPeriod, List<ProducerReportedMaterial> reportedMaterials)
        {
            ProducerId = producerId;
            SubsidiaryId = subsidiaryId;
            SubmissionPeriod = submissionPeriod;
            ReportedMaterials = reportedMaterials;
        }
    }*/


    public class LevelledProducerService : ILevelledProducerService
    {
        private readonly ApplicationDBContext dbContext;

        public LevelledProducerService(IDbContextFactory<ApplicationDBContext> context)
        {
            this.dbContext = context.CreateDbContext();
        }

        public async Task<List<ProducerReportedMaterialsForSubmissionPeriod>> GetProducers(int runId, List<MaterialDetail> materials)
        {
            return await (
                from run in dbContext.CalculatorRuns.AsNoTracking()
                join pd in dbContext.ProducerDetail.AsNoTracking() on run.Id equals pd.CalculatorRunId
                join prm in dbContext.ProducerReportedMaterial.AsNoTracking() on pd.Id equals prm.ProducerDetailId
                where pd.CalculatorRunId == runId
                group prm by new { pd.ProducerId, pd.SubsidiaryId, prm.SubmissionPeriod } into prms
                select new ProducerReportedMaterialsForSubmissionPeriod(prms.Key.ProducerId, prms.Key.SubsidiaryId, prms.Key.SubmissionPeriod, prms.ToList())
            ).ToListAsync();
        }
    }
}
