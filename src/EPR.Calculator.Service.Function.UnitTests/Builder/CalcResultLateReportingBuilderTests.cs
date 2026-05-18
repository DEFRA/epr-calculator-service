using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    [TestClass]
    public class CalcResultLateReportingBuilderTest
    {
        public required CalcResultLateReportingBuilder builder;
        protected ApplicationDBContext? dbContext;

        private static readonly IImmutableList<MaterialDetail> Materials = ImmutableList.Create(
            new MaterialDetail { Id = 1, Code = "AL", Name = "Aluminium",       Description = "Aluminium"       },
            new MaterialDetail { Id = 2, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" }
        );

        [TestInitialize]
        public void DataSetup()
        {
            var dbContextOptions =
                new DbContextOptionsBuilder<ApplicationDBContext>()
                    .UseInMemoryDatabase(databaseName: "PayCal")
                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();

            dbContext.DefaultParameterTemplateMasterList.RemoveRange(dbContext.DefaultParameterTemplateMasterList);
            dbContext.DefaultParameterSettingDetail.RemoveRange(dbContext.DefaultParameterSettingDetail);
            dbContext.CalculatorRuns.RemoveRange(dbContext.CalculatorRuns);
            dbContext.SaveChanges();

            var calculatorRuns = new List<CalculatorRun>
            {
                new() { Id = 1,
                        DefaultParameterSettingMasterId = 1,
                        CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                        Name = "Test Run",
                        RelativeYear = new RelativeYear(2024),
                        CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                        CreatedBy = "Test User",
                        LapcapDataMasterId = 2,
                      },
            };

            var defaultParameterSettings = new List<DefaultParameterSettingMaster>
            {
                new() { Id = 1, RelativeYear = new RelativeYear(2024) },
            };

            var defaultParameterTemplateMasterList = new List<DefaultParameterTemplateMaster>
            {
                new() { ParameterUniqueReferenceId = "1", ParameterType = "Late Reporting Tonnage", ParameterCategory = "Aluminium-R" },
                new() { ParameterUniqueReferenceId = "2", ParameterType = "Late Reporting Tonnage", ParameterCategory = "Aluminium-A" },
                new() { ParameterUniqueReferenceId = "3", ParameterType = "Late Reporting Tonnage", ParameterCategory = "Aluminium-G" },
                new() { ParameterUniqueReferenceId = "4", ParameterType = "Late Reporting Tonnage", ParameterCategory = "Fibre composite-R" },
                new() { ParameterUniqueReferenceId = "5", ParameterType = "Late Reporting Tonnage", ParameterCategory = "Fibre composite-A" },
                new() { ParameterUniqueReferenceId = "6", ParameterType = "Late Reporting Tonnage", ParameterCategory = "Fibre composite-G" },
            };

            var defaultParameterSettingDetails = new List<DefaultParameterSettingDetail>
            {
                new() { DefaultParameterSettingMasterId = 1, ParameterUniqueReferenceId = "1", ParameterValue = 100, DefaultParameterSettingMaster = defaultParameterSettings[0] },
                new() { DefaultParameterSettingMasterId = 1, ParameterUniqueReferenceId = "2", ParameterValue = 200, DefaultParameterSettingMaster = defaultParameterSettings[0] },
                new() { DefaultParameterSettingMasterId = 1, ParameterUniqueReferenceId = "3", ParameterValue = 300, DefaultParameterSettingMaster = defaultParameterSettings[0] },
                new() { DefaultParameterSettingMasterId = 1, ParameterUniqueReferenceId = "4", ParameterValue = 400, DefaultParameterSettingMaster = defaultParameterSettings[0] },
                new() { DefaultParameterSettingMasterId = 1, ParameterUniqueReferenceId = "5", ParameterValue = 500, DefaultParameterSettingMaster = defaultParameterSettings[0] },
                new() { DefaultParameterSettingMasterId = 1, ParameterUniqueReferenceId = "6", ParameterValue = 600, DefaultParameterSettingMaster = defaultParameterSettings[0] },
            };

            dbContext.CalculatorRuns.AddRange(calculatorRuns);
            dbContext.DefaultParameterSettings.AddRange(defaultParameterSettings);
            dbContext.DefaultParameterSettingDetail.AddRange(defaultParameterSettingDetails);
            dbContext.DefaultParameterTemplateMasterList.AddRange(defaultParameterTemplateMasterList);
            dbContext.SaveChanges();

            builder = new CalcResultLateReportingBuilder(dbContext);
        }

        public ApplicationDBContext? GetDbContext() => dbContext;

        [TestMethod]
        public async Task Construct_ShouldReturnCorrectResults()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2024) };

            var result = await builder.ConstructAsync(Materials, requestDto);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.LateReportingTonnageByMaterial.Count);

            var aluminium = result.LateReportingTonnageByMaterial["AL"];
            Assert.AreEqual(100m, aluminium.RedLateReportingTonnage);
            Assert.AreEqual(200m, aluminium.AmberLateReportingTonnage);
            Assert.AreEqual(300m, aluminium.GreenLateReportingTonnage);
            Assert.AreEqual(600m, aluminium.TotalLateReportingTonnage);

            var fibre = result.LateReportingTonnageByMaterial["FC"];
            Assert.AreEqual(400m, fibre.RedLateReportingTonnage);
            Assert.AreEqual(500m, fibre.AmberLateReportingTonnage);
            Assert.AreEqual(600m, fibre.GreenLateReportingTonnage);
            Assert.AreEqual(1500m, fibre.TotalLateReportingTonnage);

            var total = result.LateReportingTonnageTotal;
            Assert.AreEqual(500m,  total.RedLateReportingTonnage);
            Assert.AreEqual(700m,  total.AmberLateReportingTonnage);
            Assert.AreEqual(900m,  total.GreenLateReportingTonnage);
            Assert.AreEqual(2100m, total.TotalLateReportingTonnage);
        }
    }
}
