using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Data;
using EPR.Calculator.Service.Function.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Constants;
using Microsoft.Azure.Amqp.Framing;

namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    [TestClass]
    public class CalcResultScaledupProducersBuilderTest
    {
        private CalcResultScaledupProducersBuilder builder;
        private ApplicationDBContext dbContext;
        int runId = 1;

        private void PrepareScaledUpProducer()
        {
            var producerDetail = new ProducerDetail
            {
                CalculatorRunId = runId,
                ProducerId = 10,
                SubsidiaryId = "Subsidary 1",
            };
            dbContext.ProducerDetail.Add(producerDetail);
            dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                PackagingType = "HH",
                ProducerDetail = producerDetail
            });
            var calcRunPomDataMaster = new CalculatorRunPomDataMaster
            {
                CalendarYear = "2024",
                EffectiveFrom = DateTime.Now,
                CreatedAt = DateTime.Now,
                CreatedBy = "Test User"
            };
            dbContext.CalculatorRunPomDataMaster.Add(calcRunPomDataMaster);
            dbContext.CalculatorRuns.Add(new CalculatorRun
            {
                Financial_Year = "2024-25",
                Name = "Name",
                CalculatorRunPomDataMaster = calcRunPomDataMaster
            });
            dbContext.CalculatorRunPomDataDetails.Add(
                new CalculatorRunPomDataDetail
                {
                    LoadTimeStamp = DateTime.Now,
                    SubmissionPeriod = "2024-P1",
                    SubmissionPeriodDesc = "desc",
                    CalculatorRunPomDataMaster = calcRunPomDataMaster,
                    OrganisationId = 10
                });
            dbContext.CalculatorRunPomDataDetails.Add(
                new CalculatorRunPomDataDetail
                {
                    LoadTimeStamp = DateTime.Now,
                    SubmissionPeriod = "2024-P2",
                    SubmissionPeriodDesc = "desc",
                    CalculatorRunPomDataMaster = calcRunPomDataMaster,
                    OrganisationId = 11
                });
            dbContext.SubmissionPeriodLookup.Add(
                new SubmissionPeriodLookup
                {
                    DaysInSubmissionPeriod = 0,
                    DaysInWholePeriod = 0,
                    EndDate = DateTime.Now,
                    StartDate = DateTime.Now,
                    ScaleupFactor = 1,
                    SubmissionPeriod = "2024-P1",
                    SubmissionPeriodDesc = ""
                });
            dbContext.SubmissionPeriodLookup.Add(
                new SubmissionPeriodLookup
                {
                    DaysInSubmissionPeriod = 0,
                    DaysInWholePeriod = 0,
                    EndDate = DateTime.Now,
                    StartDate = DateTime.Now,
                    ScaleupFactor = 2.999M,
                    SubmissionPeriod = "2024-P2",
                    SubmissionPeriodDesc = ""
                });
            dbContext.SaveChanges();
        }

        [TestInitialize]
        public void Init()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: "PayCal")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;


            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            this.builder = new CalcResultScaledupProducersBuilder(dbContext);
        }

        [TestMethod]
        public void GetScaledUpProducerIds_Test()
        {
            PrepareScaledUpProducer();
            var task = this.builder.GetScaledUpOrganisationIdsAsync(runId);
            task.Wait();

            var result = task.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
        }

        [TestMethod]
        public void AddExtraRowsTest()
        {
            builder = new CalcResultScaledupProducersBuilder(dbContext);
            var runProducerMaterialDetails = new List<CalcResultScaledupProducer>();
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 1,
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 1,
                SubsidiaryId = "Sub1"
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 1,
                SubsidiaryId = "Sub2"
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 2,
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 2,
                SubsidiaryId = "Sub3"
            });
            runProducerMaterialDetails.Add(new CalcResultScaledupProducer
            {
                ProducerId = 2,
                SubsidiaryId = "Sub4"
            });
            builder.AddExtraRows(runProducerMaterialDetails);

            Assert.AreEqual(8, runProducerMaterialDetails.Count);
            var allProducersWithLevel2 = runProducerMaterialDetails.Where(x => x.SubsidiaryId == null);
            Assert.IsTrue(allProducersWithLevel2.All(x => x.Level == CommonConstants.LevelTwo.ToString()));

            var extraRows = runProducerMaterialDetails.Skip(Math.Max(0, runProducerMaterialDetails.Count() - 2));
            Assert.AreEqual(2, extraRows.Count());
            Assert.IsTrue(extraRows.All(x => x.IsSubtotalRow == true));
            Assert.AreEqual(2, runProducerMaterialDetails.Count(x => x.IsSubtotalRow == true));
        }
    }
}
