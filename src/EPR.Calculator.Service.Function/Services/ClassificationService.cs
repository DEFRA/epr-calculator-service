using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Enums;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    public class ClassificationService : IClassificationService
    {
        private readonly ApplicationDBContext Context;

        public ClassificationService(
            IDbContextFactory<ApplicationDBContext> context)
        {
            Context = context.CreateDbContext();
        }

        public async Task UpdateRunClassification(int runId, RunClassification runClassification)
        {
            var calculatorRun = Context.CalculatorRuns.SingleOrDefault(run => run.Id == runId);
            if (calculatorRun == null)
            {
                throw new KeyNotFoundException($"Calculator run id {runId} not found");
            }

            calculatorRun.Classification = runClassification;
            Context.CalculatorRuns.Update(calculatorRun);
            await Context.SaveChangesAsync();
        }
    }
}
