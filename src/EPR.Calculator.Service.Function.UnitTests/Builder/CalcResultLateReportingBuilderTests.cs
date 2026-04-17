using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    [TestClass]
    public class CalcResultLateReportingBuilderTest
    {
        private CalcResultLateReportingBuilder builder = null!;
        private ApplicationDBContext dbContext = null!;

        [TestInitialize]
        public void DataSetup()
        {
            dbContext = TestFixtures.New().Create<ApplicationDBContext>();

            var calculatorRuns = new List<CalculatorRun>
            {
                new() { Id = 1,
                        DefaultParameterSettingMasterId = 1,
                        CalculatorRunClassificationId = RunClassificationStatusIds.RUNNINGID,
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
                new()
                {
                    DefaultParameterSettingMasterId = 1,
                    ParameterUniqueReferenceId = "1",
                    ParameterValue = 100,
                    DefaultParameterSettingMaster = defaultParameterSettings[0],
                },
                new()
                {
                    DefaultParameterSettingMasterId = 1,
                    ParameterUniqueReferenceId = "2",
                    ParameterValue = 200,
                    DefaultParameterSettingMaster = defaultParameterSettings[0],
                },
                new()
                {
                    DefaultParameterSettingMasterId = 1,
                    ParameterUniqueReferenceId = "3",
                    ParameterValue = 300,
                    DefaultParameterSettingMaster = defaultParameterSettings[0],
                },
                new()
                {
                    DefaultParameterSettingMasterId = 1,
                    ParameterUniqueReferenceId = "4",
                    ParameterValue = 400,
                    DefaultParameterSettingMaster = defaultParameterSettings[0],
                },
                new()
                {
                    DefaultParameterSettingMasterId = 1,
                    ParameterUniqueReferenceId = "5",
                    ParameterValue = 500,
                    DefaultParameterSettingMaster = defaultParameterSettings[0],
                },
                new()
                {
                    DefaultParameterSettingMasterId = 1,
                    ParameterUniqueReferenceId = "6",
                    ParameterValue = 600,
                    DefaultParameterSettingMaster = defaultParameterSettings[0],
                },
            };

            dbContext.CalculatorRuns.AddRange(calculatorRuns);
            dbContext.DefaultParameterSettings.AddRange(defaultParameterSettings);
            dbContext.DefaultParameterSettingDetail.AddRange(defaultParameterSettingDetails);
            dbContext.DefaultParameterTemplateMasterList.AddRange(defaultParameterTemplateMasterList);
            dbContext.SaveChanges();

            builder = new CalcResultLateReportingBuilder(dbContext);
        }

        [TestMethod]
        public async Task Construct_ShouldReturnCorrectResults()
        {
            var runContext = TestFixtures.Default.Create<CalculatorRunContext>();
            var result = await builder.ConstructAsync(runContext);

            Assert.IsNotNull(result);
            Assert.AreEqual(CalcResultLateReportingBuilder.LateReportingHeader, result.Name);
            Assert.AreEqual(CalcResultLateReportingBuilder.MaterialHeading, result.MaterialHeading);
            Assert.AreEqual(CalcResultLateReportingBuilder.TonnageHeading, result.TonnageHeading);
            Assert.AreEqual(CalcResultLateReportingBuilder.RedTonnageHeading, result.RedTonnageHeading);
            Assert.AreEqual(CalcResultLateReportingBuilder.AmberTonnageHeading, result.AmberTonnageHeading);
            Assert.AreEqual(CalcResultLateReportingBuilder.GreenTonnageHeading, result.GreenTonnageHeading);
            Assert.AreEqual(3, result.CalcResultLateReportingTonnageDetails.Count());

            var material1 = result.CalcResultLateReportingTonnageDetails.SingleOrDefault(x => x.Name == "Aluminium");
            Assert.IsNotNull(material1);
            Assert.AreEqual(100.000M, material1.RedLateReportingTonnage);
            Assert.AreEqual(200.000M, material1.AmberLateReportingTonnage);
            Assert.AreEqual(300.000M, material1.GreenLateReportingTonnage);
            Assert.AreEqual(600.000M, material1.TotalLateReportingTonnage);

            var material2 = result.CalcResultLateReportingTonnageDetails.SingleOrDefault(x => x.Name == "Fibre composite");
            Assert.IsNotNull(material2);
            Assert.AreEqual(400.000M, material2.RedLateReportingTonnage);
            Assert.AreEqual(500.000M, material2.AmberLateReportingTonnage);
            Assert.AreEqual(600.000M, material2.GreenLateReportingTonnage);
            Assert.AreEqual(1500.000M, material2.TotalLateReportingTonnage);

            var total = result.CalcResultLateReportingTonnageDetails.SingleOrDefault(x => x.Name == CalcResultLateReportingBuilder.Total);
            Assert.IsNotNull(total);
            Assert.AreEqual(500.000M, total.RedLateReportingTonnage);
            Assert.AreEqual(700.000M, total.AmberLateReportingTonnage);
            Assert.AreEqual(900.000M, total.GreenLateReportingTonnage);
            Assert.AreEqual(2100.000M, total.TotalLateReportingTonnage);
        }
    }
}