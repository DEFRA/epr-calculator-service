namespace EPR.Calculator.Service.Function.UnitTests.Builder.CancelledProducers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.Service.Function.Builder.CancelledProducers;
    using EPR.Calculator.Service.Function.Builder.CommsCost;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Enums;

    [TestClass]
    public class CalcResultCancelledProducersBuilderTests
    {
        private CalcResultCancelledProducersBuilder builder;
        private readonly ApplicationDBContext dbContext;
        private Mock<IDbContextFactory<ApplicationDBContext>> dbContextFactory;

        private Mock<IMaterialService> materialService;

        public CalcResultCancelledProducersBuilderTests()
        {

            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            this.dbContextFactory = new Mock<IDbContextFactory<ApplicationDBContext>>();
            this.dbContext = new ApplicationDBContext(dbContextOptions);
            this.dbContext.Database.EnsureCreated();
            this.dbContextFactory.Setup(factory => factory.CreateDbContext()).Returns(this.dbContext);
            this.materialService = new Mock<IMaterialService>();
            this.builder = new CalcResultCancelledProducersBuilder(
                this.dbContext, this.materialService.Object);
        }

        private Fixture Fixture { get; init; } = new Fixture();

        [TestCleanup]
        public void TearDown()
        {
            this.dbContext?.Database.EnsureDeleted();
        }        

        [TestMethod]
        public async Task CanConstruct()
        {
            this.SeedDatabaseForInitialRun(dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto() { RunId = 2 };

            var expected = dbContext.ProducerDesignatedRunInvoiceInstruction.FirstOrDefault(t => t.ProducerId == 2 && t.CalculatorRunId == 1);


            this.materialService.Setup(t => t.GetMaterials()).ReturnsAsync(TestDataHelper.GetMaterials().ToList());

            // Act
            var result = await builder.Construct(requestDto, "2025-26");

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreEqual("2", cancelledProducer.ProducerIdValue);
            Assert.AreEqual("Test2", cancelledProducer.ProducerOrSubsidiaryNameValue);
            Assert.AreEqual(expected?.BillingInstructionId, cancelledProducer?.LatestInvoice?.BillingInstructionIdValue);
            Assert.AreEqual(expected?.CalculatorRunId.ToString(), cancelledProducer?.LatestInvoice?.RunNumberValue);
            Assert.AreEqual(expected?.CurrentYearInvoicedTotalAfterThisRun, 100);
        }

        [TestMethod]
        public async Task CancelledProducersShouldNotGetAcceptedProducers()
        {
            this.SeedDatabaseForInitialRunCompleted(dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto() { RunId = 2 };

            var expected = dbContext.ProducerResultFileSuggestedBillingInstruction.FirstOrDefault(t => t.BillingInstructionAcceptReject == CommonConstants.Accepted && t.SuggestedBillingInstruction.ToLowerInvariant() == CommonConstants.Cancel.ToLowerInvariant() && t.CalculatorRunId == 2);


            this.materialService.Setup(t => t.GetMaterials()).ReturnsAsync(TestDataHelper.GetMaterials().ToList());

            // Act
            var result = await builder.Construct(requestDto, "2025-26");

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreNotEqual(expected?.ProducerId.ToString(), cancelledProducer.ProducerIdValue);
            Assert.AreEqual("4", cancelledProducer.ProducerIdValue);
        }

        [TestMethod]
        public async Task CancelledProducersShouldGetRejectedProducers()
        {
            this.SeedDatabaseForInitialRunCompleted(dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto() { RunId = 3 };

            var expected = dbContext.ProducerResultFileSuggestedBillingInstruction.FirstOrDefault(t => t.BillingInstructionAcceptReject ==
            CommonConstants.Rejected && t.SuggestedBillingInstruction.ToLowerInvariant() == CommonConstants.Cancel.ToLowerInvariant()
            && t.CalculatorRunId == 2);

            this.materialService.Setup(t => t.GetMaterials()).ReturnsAsync(TestDataHelper.GetMaterials().ToList());

            // Act
            var result = await builder.Construct(requestDto, "2025-26");

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreEqual(expected?.ProducerId.ToString(), cancelledProducer.ProducerIdValue);
        }

        [TestMethod]
        public async Task CancelledProducersWithNoMaterials()
        {
            this.SeedDatabaseForInitialRunCompleted(dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto() { RunId = 3 };

            var expected = dbContext.ProducerResultFileSuggestedBillingInstruction.FirstOrDefault(t => t.BillingInstructionAcceptReject ==
            CommonConstants.Rejected && t.SuggestedBillingInstruction.ToLowerInvariant() == CommonConstants.Cancel.ToLowerInvariant()
            && t.CalculatorRunId == 2);
          

            // Act
            var result = await builder.Construct(requestDto, "2025-26");

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreEqual(expected?.ProducerId.ToString(), cancelledProducer.ProducerIdValue);
            Assert.IsNull(cancelledProducer.LastTonnage?.AluminiumValue);
        }

        private void SeedDatabaseForInitialRun(ApplicationDBContext context)
        {
            var financialYear = new CalculatorRunFinancialYear { Name = "2025-26" };

            //calculator runs
            var runs = new List<CalculatorRun>() { new CalculatorRun { Id = 1, Financial_Year = financialYear, FinancialYearId = "2025-26", CalculatorRunClassificationId=7, Name = "CalculatorRunTest1" },
             new CalculatorRun { Id = 2, Financial_Year = financialYear, FinancialYearId = "2025-26", CalculatorRunClassificationId=2, Name = "CalculatorRunTest2" }};
            context.CalculatorRuns.AddRange(runs);



            var producerDetails = new List<ProducerDetail>()
            {  new ProducerDetail() { Id =1 , CalculatorRunId = 1, ProducerName="Test1", ProducerId = 1, TradingName = "TN1"},
             new ProducerDetail() { Id =2 , CalculatorRunId = 1, ProducerName="Test2", ProducerId = 2, TradingName = "TN2"},
              new ProducerDetail() { Id =3 , CalculatorRunId = 2, ProducerName="Test1", ProducerId = 1, TradingName = "TN3"},
               new ProducerDetail() { Id =4 , CalculatorRunId = 1, ProducerName="Test3", ProducerId = 3, TradingName = "TN4"},
            };

            context.ProducerDetail.AddRange(producerDetails);

            var materials = new List<Material>
        {
            new Material { Id = 1, Name = "Plastic", Code = MaterialCodes.Plastic },
            new Material { Id = 2, Name = "Steel", Code = MaterialCodes.Steel },
            new Material { Id = 3, Name = "Glass", Code = MaterialCodes.Glass },
        };
            context.Material.AddRange(materials);

            var producerReportedMaterials = new List<ProducerReportedMaterial>
        {
            new ProducerReportedMaterial { Id = 1, ProducerDetailId = 1, MaterialId = 1, PackagingType = PackagingTypes.Household, PackagingTonnage = 100 },
            new ProducerReportedMaterial { Id = 2, ProducerDetailId = 1, MaterialId = 2, PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 200 },
            new ProducerReportedMaterial { Id = 3, ProducerDetailId = 1, MaterialId = 3, PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 300 },
             new ProducerReportedMaterial { Id = 4, ProducerDetailId = 2, MaterialId = 1, PackagingType = PackagingTypes.Household, PackagingTonnage = 100 },
            new ProducerReportedMaterial { Id = 5, ProducerDetailId = 2, MaterialId = 2, PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 200 },
            new ProducerReportedMaterial { Id = 6, ProducerDetailId = 2, MaterialId = 3, PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 300 },

        };
            context.ProducerReportedMaterial.AddRange(producerReportedMaterials);



            var designatedRunInvoice = new List<ProducerDesignatedRunInvoiceInstruction>()
            { new ProducerDesignatedRunInvoiceInstruction()
                {
                    BillingInstructionId = "1_1",
                    CalculatorRunId = 1,
                    CurrentYearInvoicedTotalAfterThisRun = 100,
                    Id = 1,
                    ProducerId = 1,
                    InvoiceAmount = 100,
                    OutstandingBalance = 100,

                },
                new ProducerDesignatedRunInvoiceInstruction()
                {
                    BillingInstructionId = "1_2",
                    CalculatorRunId = 1,
                    CurrentYearInvoicedTotalAfterThisRun = 100,
                    Id = 2,
                    ProducerId = 2,
                    InvoiceAmount = 100,
                    OutstandingBalance = 100,

                },
            };


            context.ProducerDesignatedRunInvoiceInstruction.AddRange(designatedRunInvoice);


            var billingInstructionList = new List<ProducerResultFileSuggestedBillingInstruction>()
            {  new ProducerResultFileSuggestedBillingInstruction()
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 1,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 1,
                    BillingInstructionAcceptReject = "Accepted"
                },
            new ProducerResultFileSuggestedBillingInstruction()
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 2,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 1,
                    BillingInstructionAcceptReject = "Accepted"
                },
             new ProducerResultFileSuggestedBillingInstruction()
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 3,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 1,
                    BillingInstructionAcceptReject = "Accepted"
                }
            };



            context.ProducerResultFileSuggestedBillingInstruction.AddRange(billingInstructionList);

            var materialInvoiceTonnage = new List<ProducerInvoicedMaterialNetTonnage>()
            {
                 new ProducerInvoicedMaterialNetTonnage()
                 {
                      CalculatorRunId =1,
                      MaterialId= 1,
                      InvoicedNetTonnage = 100,
                      ProducerId =1, Id=1

                 },
                new ProducerInvoicedMaterialNetTonnage()
                 {
                      CalculatorRunId =1,
                      MaterialId= 2,
                      InvoicedNetTonnage = 100,
                      ProducerId =1,
                    Id=2

                 },
            new ProducerInvoicedMaterialNetTonnage()
                 {
                      CalculatorRunId =1,
                      MaterialId= 1,
                      InvoicedNetTonnage = 100,
                      ProducerId =2, Id=3

                 },
                new ProducerInvoicedMaterialNetTonnage()
                 {
                      CalculatorRunId =1,
                      MaterialId= 2,
                      InvoicedNetTonnage = 100,
                      ProducerId =2,
                    Id=4

                 }};

            context.ProducerInvoicedMaterialNetTonnage.AddRange(materialInvoiceTonnage);



            context.SaveChanges();
        }

        private void SeedDatabaseForInitialRunCompleted(ApplicationDBContext context)
        {
            var financialYear = new CalculatorRunFinancialYear { Name = "2025-26" };

            //calculator runs
            var runs = new List<CalculatorRun>() { new CalculatorRun { Id = 1, Financial_Year = financialYear, FinancialYearId = "2025-26", CalculatorRunClassificationId=7, Name = "CalculatorRunTest1" },
             new CalculatorRun { Id = 2, Financial_Year = financialYear, FinancialYearId = "2025-26", CalculatorRunClassificationId=12, Name = "CalculatorRunTest2" },
            new CalculatorRun { Id = 3, Financial_Year = financialYear, FinancialYearId = "2025-26", CalculatorRunClassificationId=2, Name = "CalculatorRunTest3" }};

            context.CalculatorRuns.AddRange(runs);



            var producerDetails = new List<ProducerDetail>()
            {  new ProducerDetail() { Id =1 , CalculatorRunId = 1, ProducerName="Test1", ProducerId = 1, TradingName = "TN1"},
             new ProducerDetail() { Id =2 , CalculatorRunId = 1, ProducerName="Test2", ProducerId = 2, TradingName = "TN2"},
               new ProducerDetail() { Id =4 , CalculatorRunId = 1, ProducerName="Test3", ProducerId = 3, TradingName = "TN4"},
                new ProducerDetail() { Id =5 , CalculatorRunId = 1, ProducerName="Test4", ProducerId = 4, TradingName = "TN5"},
                 new ProducerDetail() { Id =6 , CalculatorRunId = 2, ProducerName="Test2", ProducerId = 2, TradingName = "TN2"},
              new ProducerDetail() { Id =3 , CalculatorRunId = 2, ProducerName="Test1", ProducerId = 1, TradingName = "TN3"},
               new ProducerDetail() { Id =7 , CalculatorRunId = 3, ProducerName="Test1", ProducerId = 1, TradingName = "TN3"},
                new ProducerDetail() { Id =8 , CalculatorRunId = 3, ProducerName="Test2", ProducerId = 2, TradingName = "TN2"},
            };

            context.ProducerDetail.AddRange(producerDetails);

            var materials = new List<Material>
        {
            new Material { Id = 1, Name = "Plastic", Code = MaterialCodes.Plastic },
            new Material { Id = 2, Name = "Steel", Code = MaterialCodes.Steel },
            new Material { Id = 3, Name = "Glass", Code = MaterialCodes.Glass },
        };
            context.Material.AddRange(materials);

            var producerReportedMaterials = new List<ProducerReportedMaterial>
        {
            new ProducerReportedMaterial { Id = 1, ProducerDetailId = 1, MaterialId = 1, PackagingType = PackagingTypes.Household, PackagingTonnage = 100 },
            new ProducerReportedMaterial { Id = 2, ProducerDetailId = 1, MaterialId = 2, PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 200 },
            new ProducerReportedMaterial { Id = 3, ProducerDetailId = 1, MaterialId = 3, PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 300 },
             new ProducerReportedMaterial { Id = 4, ProducerDetailId = 2, MaterialId = 1, PackagingType = PackagingTypes.Household, PackagingTonnage = 100 },
            new ProducerReportedMaterial { Id = 5, ProducerDetailId = 2, MaterialId = 2, PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 200 },
            new ProducerReportedMaterial { Id = 6, ProducerDetailId = 2, MaterialId = 3, PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 300 },
                         new ProducerReportedMaterial { Id = 7, ProducerDetailId = 4, MaterialId = 1, PackagingType = PackagingTypes.Household, PackagingTonnage = 100 },
            new ProducerReportedMaterial { Id = 8, ProducerDetailId = 4, MaterialId = 2, PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 200 },
            new ProducerReportedMaterial { Id = 9, ProducerDetailId = 4, MaterialId = 3, PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 300 },
                                 new ProducerReportedMaterial { Id = 10, ProducerDetailId = 3, MaterialId = 1, PackagingType = PackagingTypes.Household, PackagingTonnage = 100 },
            new ProducerReportedMaterial { Id = 11, ProducerDetailId = 3, MaterialId = 2, PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 200 },
            new ProducerReportedMaterial { Id = 12, ProducerDetailId = 3, MaterialId = 3, PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 300 },

        };
            context.ProducerReportedMaterial.AddRange(producerReportedMaterials);



            var designatedRunInvoice = new List<ProducerDesignatedRunInvoiceInstruction>()
            { new ProducerDesignatedRunInvoiceInstruction()
                {
                    BillingInstructionId = "1_1",
                    CalculatorRunId = 1,
                    CurrentYearInvoicedTotalAfterThisRun = 100,
                    Id = 1,
                    ProducerId = 1,
                    InvoiceAmount = 100,
                    OutstandingBalance = 100,

                },
                 new ProducerDesignatedRunInvoiceInstruction()
                {
                    BillingInstructionId = "1_2",
                    CalculatorRunId = 1,
                    CurrentYearInvoicedTotalAfterThisRun = 100,
                    Id = 2,
                    ProducerId = 2,
                    InvoiceAmount = 100,
                    OutstandingBalance = 100,

                },
                 new ProducerDesignatedRunInvoiceInstruction()
                {
                    BillingInstructionId = "1_3",
                    CalculatorRunId = 1,
                    CurrentYearInvoicedTotalAfterThisRun = 100,
                    Id = 3,
                    ProducerId = 3,
                    InvoiceAmount = 100,
                    OutstandingBalance = 100,

                },
                 new ProducerDesignatedRunInvoiceInstruction()
                {
                    BillingInstructionId = "1_4",
                    CalculatorRunId = 1,
                    CurrentYearInvoicedTotalAfterThisRun = 100,
                    Id = 4,
                    ProducerId = 4,
                    InvoiceAmount = 100,
                    OutstandingBalance = 100,

                },
                  new ProducerDesignatedRunInvoiceInstruction()
                {
                    BillingInstructionId = "2_5",
                    CalculatorRunId = 2,
                    CurrentYearInvoicedTotalAfterThisRun = 100,
                    Id = 5,
                    ProducerId = 1,
                    InvoiceAmount = 100,
                    OutstandingBalance = 100,
                },
                  new ProducerDesignatedRunInvoiceInstruction()
                {
                    BillingInstructionId = "2_6",
                    CalculatorRunId = 2,
                    CurrentYearInvoicedTotalAfterThisRun = 100,
                    Id = 6,
                    ProducerId = 2,
                    InvoiceAmount = 100,
                    OutstandingBalance = 100,
                },
            };


            context.ProducerDesignatedRunInvoiceInstruction.AddRange(designatedRunInvoice);


            var billingInstructionList = new List<ProducerResultFileSuggestedBillingInstruction>()
            {  new ProducerResultFileSuggestedBillingInstruction()
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 1,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 1,
                    BillingInstructionAcceptReject = "Accepted"
                },
            new ProducerResultFileSuggestedBillingInstruction()
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 2,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 1,
                    BillingInstructionAcceptReject = "Accepted"
                },
             new ProducerResultFileSuggestedBillingInstruction()
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 3,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 1,
                    BillingInstructionAcceptReject = "Accepted"
                },
              new ProducerResultFileSuggestedBillingInstruction()
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 4,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 1,
                    BillingInstructionAcceptReject = "Accepted"
                },
               new ProducerResultFileSuggestedBillingInstruction()
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 1,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 2,
                    BillingInstructionAcceptReject = "Accepted"
                },
                new ProducerResultFileSuggestedBillingInstruction()
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 2,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 2,
                    BillingInstructionAcceptReject = "Accepted"
                }, new ProducerResultFileSuggestedBillingInstruction()
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 3,
                    SuggestedBillingInstruction = "CANCEL",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 2,
                    BillingInstructionAcceptReject = "Accepted"
                },
                 new ProducerResultFileSuggestedBillingInstruction()
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 4,
                    SuggestedBillingInstruction = "CANCEL",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 2,
                    BillingInstructionAcceptReject = "Rejected"
                }
            };



            context.ProducerResultFileSuggestedBillingInstruction.AddRange(billingInstructionList);

            var materialInvoiceTonnage = new List<ProducerInvoicedMaterialNetTonnage>()
            {
                 new ProducerInvoicedMaterialNetTonnage()
                 {
                     CalculatorRunId =1,
                      MaterialId= 1,
                      InvoicedNetTonnage = 100,
                      ProducerId =1,
                     Id=1

                 },
                new ProducerInvoicedMaterialNetTonnage()
                 {
                      CalculatorRunId =1,
                      MaterialId= 2,
                      InvoicedNetTonnage = 100,
                      ProducerId =1,
                     Id=2
                 },
            new ProducerInvoicedMaterialNetTonnage()
                 {
                      CalculatorRunId =1,
                      MaterialId= 1,
                      InvoicedNetTonnage = 100,
                      ProducerId =2,
                      Id=3

                 },
                new ProducerInvoicedMaterialNetTonnage()
                 {
                      CalculatorRunId =1,
                      MaterialId= 2,
                      InvoicedNetTonnage = 100,
                      ProducerId =2,
                     Id=4

                 },
                 new ProducerInvoicedMaterialNetTonnage()
                 {
                      CalculatorRunId =1,
                      MaterialId= 2,
                      InvoicedNetTonnage = 100,
                      ProducerId =3,
                     Id=5

                 },
                  new ProducerInvoicedMaterialNetTonnage()
                 {
                      CalculatorRunId =1,
                      MaterialId= 2,
                      InvoicedNetTonnage = 100,
                      ProducerId =4,
                     Id=6

                 },
            new ProducerInvoicedMaterialNetTonnage()
             {
                 CalculatorRunId = 2,
                 MaterialId = 2,
                 InvoicedNetTonnage = 100,
                 ProducerId = 1,
                 Id = 7

             }
             ,new ProducerInvoicedMaterialNetTonnage()
                 {
                      CalculatorRunId =2,
                      MaterialId= 2,
                      InvoicedNetTonnage = 100,
                      ProducerId =2,
                     Id=8
                 }
            };
            context.ProducerInvoicedMaterialNetTonnage.AddRange(materialInvoiceTonnage);
            context.SaveChanges();
        }

        private void CreateProducerDetail()
        {
            var producerNames = new string[]
            {
                "Allied Packaging",
                "Beeline Materials",
                "Cloud Boxes",
                "Decking and Shed",
                "Electric Things",
                "French Flooring",
                "Good Fruit Co",
                "Happy Shopper",
                "Icicle Foods",
                "Jumbo Box Store",
            };

            var producerId = 1;
            foreach (var producerName in producerNames)
            {
                this.dbContext.ProducerDetail.Add(new ProducerDetail
                {
                    ProducerId = producerId++,
                    SubsidiaryId = $"{producerId}-Sub",
                    ProducerName = producerName,
                    CalculatorRunId = 1,
                });
            }

            this.dbContext.SaveChanges();

            for (int producerDetailId = 1; producerDetailId <= 10; producerDetailId++)
            {
                for (int materialId = 1; materialId < 9; materialId++)
                {
                    this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        MaterialId = materialId,
                        ProducerDetailId = producerDetailId,
                        PackagingType = "HH",
                        PackagingTonnage = materialId * 100,
                    });
                }
            }

            this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial()
            {
                MaterialId = 3,
                ProducerDetailId = 1,
                PackagingType = "HDC",
                PackagingTonnage = 100,
            });

            this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial()
            {
                MaterialId = 3,
                ProducerDetailId = 2,
                PackagingType = "HDC",
                PackagingTonnage = 100,
            });

            this.dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial()
            {
                MaterialId = 2,
                ProducerDetailId = 1,
                PackagingType = "PB",
                PackagingTonnage = 200,
            });

            this.dbContext.SaveChanges();
        }

        private void CreateNewRun()
        {
            var calculatorRunFinancialYear = new CalculatorRunFinancialYear { Name = "2025-26" };
            var run = new CalculatorRun
            {
                CalculatorRunClassificationId = 7,
                Name = "Test Run",
                Financial_Year = calculatorRunFinancialYear,
                CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User",
                DefaultParameterSettingMasterId = 1,
            };

            var run1 = new CalculatorRun
            {
                CalculatorRunClassificationId = 2,
                Name = "Test Run1",
                Financial_Year = calculatorRunFinancialYear,
                CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User",
                DefaultParameterSettingMasterId = 2,
            };
            this.dbContext.CalculatorRuns.Add(run);
            this.dbContext.CalculatorRuns.Add(run1);
            this.dbContext.SaveChanges();
        }
    }
}
