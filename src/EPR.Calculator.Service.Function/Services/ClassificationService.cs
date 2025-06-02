using System;
using System.Linq;
using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Enums;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    public class ClassificationService : IClassificationService
    {
        private ApplicationDBContext Context;

        public ClassificationService(
            IDbContextFactory<ApplicationDBContext> context)
        {
            this.Context = context.CreateDbContext();
        }

        public async void UpdateRunClassification(int runId, RunClassification runClassification)
        {
            try
            {
                var calculatorRun = this.Context.CalculatorRuns.FirstOrDefault(run => run.Id == runId);
                if (calculatorRun == null)
                {
                    throw new Exception("run id");
                }

                calculatorRun.CalculatorRunClassificationId = (int)runClassification;
                this.Context.CalculatorRuns.Update(calculatorRun);
                await this.Context.SaveChangesAsync();
            }
            catch (Exception exception)
            {

            }
        }
    }
}
