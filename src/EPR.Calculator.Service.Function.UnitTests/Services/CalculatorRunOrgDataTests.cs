namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.Data.Sqlite;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Sqlite;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalculatorRunOrgDataTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ApplicationDBContext context;

        public CalculatorRunOrgDataTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseSqlite(_connection)
                .Options;

            this.context = new ApplicationDBContext(options);
            this.context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            this.context.Dispose();
            _connection.Close();
        }

        private async Task<(CalculatorRunFinancialYear, CalculatorRunClassification, OrganisationData)> SeedData()
        {
            var financialYear = new CalculatorRunFinancialYear { Name  = "2024-25" };
            this.context.FinancialYears.Add(financialYear);

            var classification = new CalculatorRunClassification { Status = "Test Classification" };
            this.context.CalculatorRunClassifications.Add(classification);

            var orgData = new OrganisationData
            {
                OrganisationId = 1,
                OrganisationName = "Test Org",
                LoadTimestamp = DateTime.Now,
                ObligationStatus = "O",
                SubmitterId = new Guid("11111111-1111-1111-1111-111111111111"),
            };

            await this.context.Database.ExecuteSqlInterpolatedAsync(
                $@"
                INSERT INTO organisation_data(organisation_id, organisation_name, load_ts, obligation_status, submitter_id)
                VALUES ({orgData.OrganisationId}, {orgData.OrganisationName}, {orgData.LoadTimestamp}, {orgData.ObligationStatus}, {orgData.SubmitterId})");

            await this.context.SaveChangesAsync();
            return (financialYear, classification, orgData);
        }

        [TestMethod]
        public async Task LoadOrgDataForCalcRun()
        {
            int runId = 1;
            int runId2 = 2;
            string calendarYear = "2024";
            string createdBy = "TestUser";
            var cancellationToken = CancellationToken.None;
            var service = new CalculatorRunOrgData(this.context);
            var (financialYear, classification, orgData) = await SeedData();

            //Run 1
            var run = new CalculatorRun { Id = runId, Financial_Year = financialYear, FinancialYearId = "2024-25", Name = "CalculatorRunTest1", CalculatorRunClassificationId = classification.Id };
            var calcRun1 = this.context.CalculatorRuns.Add(run);
            await this.context.SaveChangesAsync();

            await service.LoadOrgDataForCalcRun(runId, calendarYear, createdBy, cancellationToken);

            var masterRecords = await this.context.CalculatorRunOrganisationDataMaster.ToListAsync();
            Assert.AreEqual(1, masterRecords.Count);
            var orgMasterRun1 = masterRecords[0];
            Assert.AreEqual(calendarYear, orgMasterRun1.CalendarYear);
            Assert.AreEqual(createdBy, orgMasterRun1.CreatedBy);
            Assert.IsNull(orgMasterRun1.EffectiveTo);

            var detailRecords = await this.context.CalculatorRunOrganisationDataDetails.ToListAsync();
            Assert.AreEqual(1, detailRecords.Count);
            var orgDataRun1 = detailRecords[0];
            Assert.AreEqual(orgData.OrganisationId, orgDataRun1.OrganisationId);
            Assert.AreEqual(orgData.OrganisationName, orgDataRun1.OrganisationName);
            Assert.AreEqual(orgMasterRun1.Id, orgDataRun1.CalculatorRunOrganisationDataMasterId);

            var calculatorRun1 = await this.context.CalculatorRuns.FirstOrDefaultAsync(c => c.Id == runId);
            Assert.AreEqual(orgMasterRun1.Id, calculatorRun1!.CalculatorRunOrganisationDataMasterId);

            //Run 2
            var run2 = new CalculatorRun { Id = runId2, Financial_Year = financialYear, FinancialYearId = "2024-25", Name = "CalculatorRunTest2", CalculatorRunClassificationId = classification.Id };
            this.context.CalculatorRuns.Add(run2);
            await this.context.SaveChangesAsync();

            await service.LoadOrgDataForCalcRun(runId2, calendarYear, createdBy, cancellationToken); 
            
            var updatedMasterRecords = await this.context.CalculatorRunOrganisationDataMaster.ToListAsync();
            Assert.AreEqual(2, updatedMasterRecords.Count);
            var orgMasterRun2 = updatedMasterRecords[1];
            Assert.IsNotNull(updatedMasterRecords[0].EffectiveTo, "EffectiveTo should be set for the master record for run 1");
            Assert.IsNull(orgMasterRun2.EffectiveTo, "EffectiveTo should be null for the master record for run 2");

            var updatedDetailRecords = await this.context.CalculatorRunOrganisationDataDetails.ToListAsync();
            Assert.AreEqual(2, updatedDetailRecords.Count);
            var orgDataRun2 = updatedDetailRecords[1];
            Assert.AreEqual(orgData.OrganisationId, orgDataRun2.OrganisationId);
            Assert.AreEqual(orgData.OrganisationName, orgDataRun1.OrganisationName);
            Assert.AreEqual(orgMasterRun2.Id, orgDataRun2.CalculatorRunOrganisationDataMasterId);

            var calculatorRun2 = await this.context.CalculatorRuns.FirstOrDefaultAsync(c => c.Id == runId2);
            Assert.AreEqual(orgMasterRun2.Id, calculatorRun2!.CalculatorRunOrganisationDataMasterId);
        } 
    }
}
