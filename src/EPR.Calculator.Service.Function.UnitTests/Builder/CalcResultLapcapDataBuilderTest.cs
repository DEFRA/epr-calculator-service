﻿using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Data;
using EPR.Calculator.Service.Function.Data.DataModels;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    [TestClass]
    public class CalcResultLapcapDataBuilderTest
    {
        public CalcResultLapcapDataBuilder builder;
        protected ApplicationDBContext dbContext;

        public CalcResultLapcapDataBuilderTest()
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
            dbContext.SaveChanges();

            builder = new CalcResultLapcapDataBuilder(dbContext);
        }

        [TestCleanup]
        public void TearDown()
        {
            dbContext?.Database.EnsureDeleted();
        }

        [TestMethod]
        public void ConstructTest_For_Aluminuim_Plastic()
        {
            const string aluminium = "Aluminium";
            const string plastic = "Plastic";
            var run = new CalculatorRun
            {
                Id = 1,
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                Name = "Test Run",
                Financial_Year = "2024-25",
                CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User",
                LapcapDataMasterId = 2
            };

            
            var lapcapDataMaster = new LapcapDataMaster
            {
                Id = 2,
                ProjectionYear = "2024-25",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
            };
            var details = GetLapcapDetails(lapcapDataMaster);
            details.ForEach(detail => detail.LapcapDataMaster = lapcapDataMaster);

            dbContext.Country.Add(new Country { Code = "En", Name = "England", Description = "England" });
            dbContext.Country.Add(new Country { Code = "Wa", Name = "Wales", Description = "Wales" });
            dbContext.Country.Add(new Country { Code = "Sc", Name = "Scotland", Description = "Scotland" });
            dbContext.Country.Add(new Country { Code = "NI", Name = "Northern Ireland", Description = "Northern Ireland" });

            dbContext.CostType.Add(new CostType { Code = "1", Name = "Fee for LA Disposal Costs", Description = "Fee for LA Disposal Costs" });

            dbContext.LapcapDataMaster.Add(lapcapDataMaster);
            dbContext.LapcapDataDetail.AddRange(details);
            dbContext.SaveChanges();

            dbContext.Material.Add(new Material { Name = aluminium, Code = "AL", Description = "Some" });
            dbContext.Material.Add(new Material { Name = plastic, Code = "PL", Description = "Some" });
            dbContext.CalculatorRuns.Add(run);
            dbContext.SaveChanges();

            var resultsDto = new CalcResultsRequestDto { RunId = 1 };
            var results = builder.Construct(resultsDto);

            results.Wait();
            var lapcapDisposalCostResults = results.Result;

            var lapcapResults = results.Result;

            Assert.IsNotNull(lapcapResults);
            Assert.AreEqual(CalcResultLapcapDataBuilder.LapcapHeader, lapcapResults.Name);
            Assert.AreEqual(5, lapcapResults.CalcResultLapcapDataDetails?.Count());

            var headerRow = lapcapResults.CalcResultLapcapDataDetails?.Single(x => x.OrderId == 1);
            Assert.IsNotNull(headerRow);
            Assert.AreEqual(LapcapHeaderConstants.Name, headerRow.Name);
            Assert.AreEqual(LapcapHeaderConstants.EnglandDisposalCost, headerRow.EnglandDisposalCost);
            Assert.AreEqual(LapcapHeaderConstants.WalesDisposalCost, headerRow.WalesDisposalCost);
            Assert.AreEqual(LapcapHeaderConstants.ScotlandDisposalCost, headerRow.ScotlandDisposalCost);
            Assert.AreEqual(LapcapHeaderConstants.NorthernIrelandDisposalCost, headerRow.NorthernIrelandDisposalCost);
            Assert.AreEqual(LapcapHeaderConstants.TotalDisposalCost, headerRow.TotalDisposalCost);

            var aluminiumRow = lapcapResults.CalcResultLapcapDataDetails?.Single(x => x.Name == aluminium);
            Assert.IsNotNull(aluminiumRow);
            Assert.AreEqual(aluminium, aluminiumRow.Name);
            Assert.AreEqual("£100.00", aluminiumRow.EnglandDisposalCost);
            Assert.AreEqual("£50.00", aluminiumRow.WalesDisposalCost);
            Assert.AreEqual("£75.00", aluminiumRow.ScotlandDisposalCost);
            Assert.AreEqual("£25.00", aluminiumRow.NorthernIrelandDisposalCost);
            Assert.AreEqual("£250.00", aluminiumRow.TotalDisposalCost);

            var plasticRow = lapcapResults.CalcResultLapcapDataDetails?.Single(x => x.Name == plastic);
            Assert.IsNotNull(plasticRow);
            Assert.AreEqual(plastic, plasticRow.Name);
            Assert.AreEqual("£100.00", plasticRow.EnglandDisposalCost);
            Assert.AreEqual("£50.00", plasticRow.WalesDisposalCost);
            Assert.AreEqual("£75.00", plasticRow.ScotlandDisposalCost);
            Assert.AreEqual("£25.00", plasticRow.NorthernIrelandDisposalCost);
            Assert.AreEqual("£250.00", plasticRow.TotalDisposalCost);

            var totalRow = lapcapResults.CalcResultLapcapDataDetails?.Single(x => x.OrderId == 4);
            Assert.IsNotNull(totalRow);
            Assert.AreEqual("Total", totalRow.Name);
            Assert.AreEqual("£200.00", totalRow.EnglandDisposalCost);
            Assert.AreEqual("£100.00", totalRow.WalesDisposalCost);
            Assert.AreEqual("£150.00", totalRow.ScotlandDisposalCost);
            Assert.AreEqual("£50.00", totalRow.NorthernIrelandDisposalCost);
            Assert.AreEqual("£500.00", totalRow.TotalDisposalCost);

            var countryApp = lapcapResults.CalcResultLapcapDataDetails?.Single(x => x.OrderId == 5);
            Assert.IsNotNull(countryApp);
            Assert.AreEqual(CalcResultLapcapDataBuilder.CountryApportionment, countryApp.Name);
            Assert.AreEqual("40.00000000%", countryApp.EnglandDisposalCost);
            Assert.AreEqual("20.00000000%", countryApp.WalesDisposalCost);
            Assert.AreEqual("30.00000000%", countryApp.ScotlandDisposalCost);
            Assert.AreEqual("10.00000000%", countryApp.NorthernIrelandDisposalCost);
            Assert.AreEqual("100.00000000%", countryApp.TotalDisposalCost);

            var countryAppList = dbContext.CountryApportionment.Where(x => x.CalculatorRunId == run.Id);
            Assert.IsNotNull(countryAppList);
            Assert.AreEqual(4, countryAppList.Count());

            var englandApp = countryAppList.Single(x => x.CountryId == 1);
            Assert.IsNotNull(englandApp);
            Assert.AreEqual(englandApp.CalculatorRunId, run.Id);
            Assert.AreEqual(1, englandApp.CostTypeId);
            Assert.AreEqual(englandApp.Apportionment, totalRow.EnglandCost);

            var walesApp = countryAppList.Single(x => x.CountryId == 2);
            Assert.IsNotNull(walesApp);
            Assert.AreEqual(walesApp.CalculatorRunId, run.Id);
            Assert.AreEqual(1, walesApp.CostTypeId);
            Assert.AreEqual(walesApp.Apportionment, totalRow.WalesCost);

            var scotlandApp = countryAppList.Single(x => x.CountryId == 3);
            Assert.IsNotNull(scotlandApp);
            Assert.AreEqual(scotlandApp.CalculatorRunId, run.Id);
            Assert.AreEqual(1, scotlandApp.CostTypeId);
            Assert.AreEqual(scotlandApp.Apportionment, totalRow.ScotlandCost);

            var niApp = countryAppList.Single(x => x.CountryId == 4);
            Assert.IsNotNull(niApp);
            Assert.AreEqual(niApp.CalculatorRunId, run.Id);
            Assert.AreEqual(1, niApp.CostTypeId);
            Assert.AreEqual(niApp.Apportionment, totalRow.NorthernIrelandCost);

        }

        public static List<LapcapDataDetail> GetLapcapDetails(LapcapDataMaster master)
        {
            var details = new List<LapcapDataDetail>();

            foreach (var uniqueRef in LapcapDataUniqueReferences.UniqueReferences)
            {
                details.Add(
                    new LapcapDataDetail
                    {
                        LapcapDataMasterId = 2,
                        UniqueReference = uniqueRef,
                        TotalCost = GetTotalCostByCountry(uniqueRef),
                        LapcapDataMaster = master,
                    }
                );
            }

            return details;
        }

        public static decimal GetTotalCostByCountry(string uniqueRef)
        {
            if(uniqueRef.StartsWith("ENG-"))
            {
                return 100M;
            }
            else if(uniqueRef.StartsWith("SCT-"))
            {
                return 75M;
            }
            else if(uniqueRef.StartsWith("WLS-"))
            {
                return 50M;
            }
            return 25M;
        }
    }
}
