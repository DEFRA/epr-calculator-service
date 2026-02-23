namespace EPR.Calculator.Service.Function.Services {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public interface ICalculatorRunOrgData { Task LoadOrgDataForCalcRun(int runId, string calendarYear, string createdBy, CancellationToken cancellationToken); } 

    public class CalculatorRunOrgData : ICalculatorRunOrgData
    {
        private readonly ApplicationDBContext _context; 
        public CalculatorRunOrgData(ApplicationDBContext context) { _context = context; }

        public async Task LoadOrgDataForCalcRun(int runId, string calendarYear, string createdBy, CancellationToken cancellationToken)
        {
            var now = DateTime.Now;

            var oldOrgMaster = await _context.CalculatorRunOrganisationDataMaster
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (oldOrgMaster != null)
            {
                oldOrgMaster.EffectiveTo = now;
                await _context.SaveChangesAsync(cancellationToken);
            }

            var newMaster = new CalculatorRunOrganisationDataMaster
            {
                CalendarYear = calendarYear,
                CreatedAt = now,
                CreatedBy = createdBy,
                EffectiveFrom = now
            };

            _context.CalculatorRunOrganisationDataMaster.Add(newMaster);
            await _context.SaveChangesAsync(cancellationToken);

            var newMasterId = newMaster.Id;

            // Bulk insert via raw SQL for performance (service side - no loading of entities into memory)
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"
                INSERT INTO calculator_run_organization_data_detail
                (
                    calculator_run_organization_data_master_id,
                    load_ts,
                    organisation_id,
                    organisation_name,
                    trading_name,
                    subsidiary_id,
                    obligation_status,
                    submitter_id,
                    status_code,
                    num_days_obligated,
                    error_code,
                    joiner_date,
                    leaver_date
                )
                SELECT
                    {newMasterId},
                    load_ts,
                    organisation_id,
                    organisation_name,
                    trading_name,
                    CASE 
                        WHEN LTRIM(RTRIM(subsidiary_id)) = '' 
                        THEN NULL 
                        ELSE subsidiary_id 
                    END,
                    obligation_status,
                    submitter_id,
                    status_code,
                    num_days_obligated,
                    error_code,
                    joiner_date,
                    leaver_date
                FROM organisation_data;",
                cancellationToken);

            var calculatorRun = await _context.CalculatorRuns
                .FirstAsync(x => x.Id == runId, cancellationToken);

            calculatorRun.CalculatorRunOrganisationDataMasterId = newMasterId;

            await _context.SaveChangesAsync(cancellationToken);      
        }

    }
}