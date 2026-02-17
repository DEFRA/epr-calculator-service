namespace EPR.Calculator.Service.Function.Services {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public interface ICalculatorRunPomData { Task LoadPomDataForCalcRun(int runId, string calendarYear, string createdBy, CancellationToken cancellationToken); } 

    public class CalculatorRunPomData : ICalculatorRunPomData
    {
        private readonly ApplicationDBContext _context; 
        
        public CalculatorRunPomData(ApplicationDBContext context) { _context = context; }

        public async Task LoadPomDataForCalcRun(int runId, string calendarYear, string createdBy, CancellationToken cancellationToken)
        {
            var now = DateTime.Now;

            var oldPomMaster = await _context.CalculatorRunPomDataMaster
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (oldPomMaster != null)
            {
                oldPomMaster.EffectiveTo = now;
            }

            var newMaster = new CalculatorRunPomDataMaster
            {
                CalendarYear = calendarYear,
                CreatedAt = now,
                CreatedBy = createdBy,
                EffectiveFrom = now
            };

            _context.CalculatorRunPomDataMaster.Add(newMaster);
            await _context.SaveChangesAsync(cancellationToken);

            var newMasterId = newMaster.Id;

            // Bulk insert via raw SQL for performance (server side - no loading of entities into memory)
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"
                INSERT INTO calculator_run_pom_data_detail
                (
                    calculator_run_pom_data_master_id, 
                    load_ts,
                    organisation_id,
                    packaging_activity,
                    packaging_type,
                    packaging_class,
                    packaging_material,
                    packaging_material_weight,
                    submission_period,
                    submission_period_desc,
                    subsidiary_id,
                    submitter_id
                )
                SELECT
                    {newMasterId},
                    load_ts,
                    organisation_id,
                    packaging_activity,
                    packaging_type,
                    packaging_class,
                    packaging_material,
                    packaging_material_weight,
                    submission_period,
                    submission_period_desc,
                    CASE
                        WHEN LTRIM(RTRIM(subsidiary_id)) = '' THEN NULL
                        ELSE subsidiary_id
                    END,
                    submitter_id
                FROM pom_data;",
                cancellationToken
            );

            var calculatorRun = await _context.CalculatorRuns
                .FirstAsync(x => x.Id == runId, cancellationToken);

            calculatorRun.CalculatorRunPomDataMasterId = newMasterId;

            await _context.SaveChangesAsync(cancellationToken);  
        }
    }
}