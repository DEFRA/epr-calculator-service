using System;
using System.Linq;
using System.Threading.Tasks;
using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    public class ClassificationService : IClassificationService
    {
        private readonly ApplicationDBContext Context;
        private readonly ICalculatorTelemetryLogger telemetryLogger;

        public ClassificationService(
            IDbContextFactory<ApplicationDBContext> context,
            ICalculatorTelemetryLogger telemetryLogger)
        {
            this.Context = context.CreateDbContext();
            this.telemetryLogger = telemetryLogger;
        }

        public async Task UpdateRunClassification(int runId, RunClassification runClassification)
        {
            var calculatorRun = this.Context.CalculatorRuns.FirstOrDefault(run => run.Id == runId);
            if (calculatorRun == null)
            {
                throw new Exception($"Calculator run id {runId} not found");
            }

            calculatorRun.CalculatorRunClassificationId = (int)runClassification;
            this.Context.CalculatorRuns.Update(calculatorRun);
            await this.Context.SaveChangesAsync();
        }
    }
}
