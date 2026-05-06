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
