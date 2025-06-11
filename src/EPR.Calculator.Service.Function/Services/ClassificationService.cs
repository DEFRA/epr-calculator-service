using System.Linq;
using System.Threading.Tasks;
using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    public class ClassificationService : IClassificationService
    {
        private readonly ApplicationDBContext Context;

        public ClassificationService(
            IDbContextFactory<ApplicationDBContext> context)
        {
            this.Context = context.CreateDbContext();
        }

        public async Task UpdateRunClassification(int runId, RunClassification runClassification)
        {
            var calculatorRun = this.Context.CalculatorRuns.FirstOrDefault(run => run.Id == runId);
            if (calculatorRun == null)
            {
                throw new RecordNotFoundException($"Calculator run id {runId} not found");
            }

            calculatorRun.CalculatorRunClassificationId = (int)runClassification;
            this.Context.CalculatorRuns.Update(calculatorRun);
            await this.Context.SaveChangesAsync();
        }
    }
}
