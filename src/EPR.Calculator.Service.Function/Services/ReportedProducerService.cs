using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IReportedProducerService
    {
        Task<List<ProducerDetail>> GetProducers(int runId);
    }

    public class ReportedProducerService : IReportedProducerService
    {
        private readonly ApplicationDBContext dbContext;

        public ReportedProducerService(IDbContextFactory<ApplicationDBContext> context)
        {
            this.dbContext = context.CreateDbContext();
        }

        public async Task<List<ProducerDetail>> GetProducers(int runId)
        {
            return await dbContext.ProducerDetail.AsNoTracking()
                .Where(pd => pd.CalculatorRunId == runId)
                .Include(pd => pd.ProducerReportedMaterials).ThenInclude(prm => prm.Material)
                .ToListAsync();
        }
    }
}
