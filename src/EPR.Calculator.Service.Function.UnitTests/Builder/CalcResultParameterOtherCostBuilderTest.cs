namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Builder.ParametersOther;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Moq;

    [TestClass]
    public class CalcResultParameterOtherCostBuilderTest
    {
        public CalcResultParameterOtherCostBuilder builder;
        protected ApplicationDBContext dbContext;

        public CalcResultParameterOtherCostBuilderTest()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            dbContext.DefaultParameterTemplateMasterList.RemoveRange(dbContext.DefaultParameterTemplateMasterList);
            dbContext.SaveChanges();
            dbContext.DefaultParameterTemplateMasterList.AddRange(TestDataHelper.GetDefaultParameterTemplateMasterData().ToList());
            // dbContext.LapcapDataTemplateMaster.AddRange(BaseControllerTest.GetLapcapTemplateMasterData().ToList());
            dbContext.SaveChanges();

            var mockService = new Mock<ICalcCountryApportionmentService>();
            builder = new CalcResultParameterOtherCostBuilder(dbContext, mockService.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            dbContext?.Database.EnsureDeleted();
        }

        [TestMethod]
        public void ConstructTest()
        {
            var calculatorRunFinancialYear = new CalculatorRunFinancialYear { Name = "2023-24" };
            var run = new CalculatorRun
            {
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                Name = "Test Run",
                Financial_Year = calculatorRunFinancialYear,
                CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User",
                DefaultParameterSettingMasterId = 1,
            };

            var templateMasterList = dbContext.DefaultParameterTemplateMasterList.ToList();

            var defaultMaster = new DefaultParameterSettingMaster { ParameterYear = calculatorRunFinancialYear };
            defaultMaster.ParameterYear = new CalculatorRunFinancialYear { Name = "2025-26" };

            dbContext.DefaultParameterSettings.Add(defaultMaster);
            dbContext.CalculatorRuns.Add(run);
            dbContext.SaveChanges();

            foreach (var templateMaster in templateMasterList)
            {
                var defaultDetail = new DefaultParameterSettingDetail
                {
                    ParameterUniqueReferenceId = templateMaster.ParameterUniqueReferenceId,
                    ParameterValue = GetValue(templateMaster),
                    DefaultParameterSettingMasterId = 1,
                    DefaultParameterSettingMaster = null,
                };
                dbContext.DefaultParameterSettingDetail.Add(defaultDetail);
            }

            var param = dbContext.DefaultParameterTemplateMasterList.Single(x =>
                x.ParameterUniqueReferenceId == "BADEBT-P");
            param.ParameterCategory = "Percentage";
            param.ParameterType = "Bad debt provision";

            dbContext.DefaultParameterTemplateMasterList.Update(param);

            dbContext.CostType.Add(new CostType
                { Code = "1", Name = "LA Data Prep Charge", Description = "LA Data Prep Charge" });

            dbContext.Country.Add(new Country { Code = "En", Name = "England", Description = "England" });
            dbContext.Country.Add(new Country { Code = "Wa", Name = "Wales", Description = "Wales" });
            dbContext.Country.Add(new Country { Code = "Sc", Name = "Scotland", Description = "Scotland" });
            dbContext.Country.Add(new Country { Code = "NI", Name = "Northern Ireland", Description = "Northern Ireland" });

            dbContext.SaveChanges();

            var results = builder.ConstructAsync(new CalcResultsRequestDto { RunId = 1 });
            results.Wait();
            var otherCost = results.Result;

            Assert.IsNotNull(otherCost.SaOperatingCost);
            Assert.AreEqual(2, otherCost.SaOperatingCost.Count());
            var saOperatingheader = otherCost.SaOperatingCost.First();
            Assert.AreEqual("England", saOperatingheader.England);
            Assert.AreEqual("Northern Ireland", saOperatingheader.NorthernIreland);
            Assert.AreEqual("Scotland", saOperatingheader.Scotland);
            Assert.AreEqual("Wales", saOperatingheader.Wales);

            var saOperatingData = otherCost.SaOperatingCost.Last();
            Assert.AreEqual("3 SA Operating Costs", saOperatingData.Name);

            Assert.AreEqual(40M, saOperatingData.EnglandValue);
            Assert.AreEqual(10, saOperatingData.NorthernIrelandValue);
            Assert.AreEqual(20, saOperatingData.ScotlandValue);
            Assert.AreEqual(30, saOperatingData.WalesValue);


            var dataLa = otherCost.Details.First();
            Assert.AreEqual(40M, dataLa.EnglandValue);
            Assert.AreEqual(10M, dataLa.NorthernIrelandValue);
            Assert.AreEqual(20M, dataLa.ScotlandValue);
            Assert.AreEqual(30M, dataLa.WalesValue);

            var counteyAppLa = otherCost.Details.Last();
            Assert.AreEqual(40, counteyAppLa.EnglandValue);
            Assert.AreEqual(10, counteyAppLa.NorthernIrelandValue);
            Assert.AreEqual(20, counteyAppLa.ScotlandValue);
            Assert.AreEqual(30, counteyAppLa.WalesValue);

            Assert.AreEqual("6 Bad Debt Provision", otherCost.BadDebtProvision.Key);
            Assert.AreEqual("10.00%", otherCost.BadDebtProvision.Value);

            var schemeSetup = otherCost.SchemeSetupCost;
            Assert.AreEqual(40, schemeSetup.EnglandValue);
            Assert.AreEqual(10, schemeSetup.NorthernIrelandValue);
            Assert.AreEqual(20, schemeSetup.ScotlandValue);
            Assert.AreEqual(30, schemeSetup.WalesValue);

            Assert.AreEqual(6, otherCost.Materiality.Count());

            var header = otherCost.Materiality.First();
            Assert.AreEqual("7 Materiality", header.SevenMateriality);
            Assert.AreEqual("Amount £s", header.Amount);
            Assert.AreEqual("%", header.Percentage);

            Assert.IsTrue(
                otherCost.Materiality.Where(x => x.SevenMateriality == "Increase" || x.SevenMateriality == "Decrease")
                    .All(x => x.Amount == "£10.00" && x.Percentage == "10.00%"));
        }

        private static decimal GetValue(DefaultParameterTemplateMaster templateMaster)
        {
            if (templateMaster.ParameterType == "Scheme setup costs" ||
                templateMaster.ParameterType == "Scheme administrator operating costs" ||
                templateMaster.ParameterType == "Local authority data preparation costs")
            {
                switch (templateMaster.ParameterCategory)
                {
                    case "England":
                        return 40M;
                    case "Northern Ireland":
                        return 10M;
                    case "Scotland":
                        return 20M;
                    case "Wales":
                        return 30M;
                }
            }

            return 10;
        }
    }
}
