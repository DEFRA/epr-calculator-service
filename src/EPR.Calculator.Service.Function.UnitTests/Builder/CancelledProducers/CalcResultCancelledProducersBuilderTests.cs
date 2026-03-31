using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.CancelledProducers;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.CancelledProducers
{
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
            dbContextFactory = new Mock<IDbContextFactory<ApplicationDBContext>>();
            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            dbContextFactory.Setup(factory => factory.CreateDbContext()).Returns(dbContext);
            materialService = new Mock<IMaterialService>();
            var producerDetailService = new ProducerDetailService(dbContext);

            builder = new CalcResultCancelledProducersBuilder(
                dbContext,
                materialService.Object,
                producerDetailService);
        }

        [TestCleanup]
        public void TearDown()
        {
            dbContext?.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task CanConstruct()
        {
            TestDataHelper.SeedDatabaseForInitialRun(dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto { RunId = 2, RelativeYear = new RelativeYear(2025) };

            var expected = dbContext.ProducerDesignatedRunInvoiceInstruction.FirstOrDefault(t => t.ProducerId == 2 && t.CalculatorRunId == 1);


            materialService.Setup(t => t.GetMaterials()).ReturnsAsync(TestDataHelper.GetMaterials().ToList());

            // Act
            var result = await builder.ConstructAsync(requestDto);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreEqual(2, cancelledProducer.ProducerId);
            Assert.AreEqual("Test2", cancelledProducer.ProducerOrSubsidiaryNameValue);
            Assert.AreEqual(expected?.BillingInstructionId, cancelledProducer?.LatestInvoice?.BillingInstructionIdValue);
            Assert.AreEqual(expected?.CalculatorRunId.ToString(), cancelledProducer?.LatestInvoice?.RunNumberValue);
            Assert.AreEqual(100, cancelledProducer?.LatestInvoice?.CurrentYearInvoicedTotalToDateValue);
        }


        [TestMethod]
        public async Task CanConstructWithOutCancelledProducers()
        {
            SeedDatabaseForUnclassified(dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };

            materialService.Setup(t => t.GetMaterials()).ReturnsAsync(TestDataHelper.GetMaterials().ToList());

            // Act
            var result = await builder.ConstructAsync(requestDto);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.Count();
            Assert.AreEqual(0, cancelledProducer);
        }



        [TestMethod]
        public async Task CancelledProducersShouldNotGetAcceptedProducers()
        {
            SeedDatabaseForInitialRunCompleted(dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto { RunId = 2, RelativeYear = new RelativeYear(2025) };

            var expected = dbContext.ProducerResultFileSuggestedBillingInstruction.FirstOrDefault(t => t.BillingInstructionAcceptReject == CommonConstants.Accepted && t.SuggestedBillingInstruction.ToLowerInvariant() == CommonConstants.Cancel.ToLowerInvariant() && t.CalculatorRunId == 2);


            materialService.Setup(t => t.GetMaterials()).ReturnsAsync(TestDataHelper.GetMaterials().ToList());

            // Act
            var result = await builder.ConstructAsync(requestDto);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreNotEqual(expected?.ProducerId, cancelledProducer.ProducerId);
        }

        [TestMethod]
        public async Task CancelledProducersShouldGetAcceptedProducersForBillingFile()
        {
            SeedDatabaseForInitialRunCompleted(dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto { RunId = 3, RelativeYear = new RelativeYear(2025), IsBillingFile = true };

            var expected = dbContext.ProducerResultFileSuggestedBillingInstruction.FirstOrDefault(t => t.BillingInstructionAcceptReject == CommonConstants.Accepted && t.SuggestedBillingInstruction.ToLowerInvariant() == CommonConstants.Cancel.ToLowerInvariant() && t.CalculatorRunId == 3);


            materialService.Setup(t => t.GetMaterials()).ReturnsAsync(TestDataHelper.GetMaterials().ToList());

            //this.producerDetailsService.Setup(t => t.GetMaterials()).ReturnsAsync(TestDataHelper.GetMaterials().ToList());

            // Act
            var result = await builder.ConstructAsync(requestDto);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreEqual(expected?.ProducerId, cancelledProducer.ProducerId);
        }

        [TestMethod]
        public async Task CancelledProducersShouldNotGetRejectedProducersForBillingFile()
        {
            SeedDatabaseForInitialRunCompleted(dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto { RunId = 3, RelativeYear = new RelativeYear(2025), IsBillingFile = true };

            var expected = dbContext.ProducerResultFileSuggestedBillingInstruction.FirstOrDefault(t => t.BillingInstructionAcceptReject == CommonConstants.Rejected && t.SuggestedBillingInstruction.ToLowerInvariant() == CommonConstants.Cancel.ToLowerInvariant() && t.CalculatorRunId == 3);


            materialService.Setup(t => t.GetMaterials()).ReturnsAsync(TestDataHelper.GetMaterials().ToList());

            // Act
            var result = await builder.ConstructAsync(requestDto);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreNotEqual(expected?.ProducerId, cancelledProducer.ProducerId);
        }


        [TestMethod]
        public async Task CancelledProducersShouldGetRejectedProducers()
        {
            SeedDatabaseForInitialRunCompleted(dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto { RunId = 3, RelativeYear = new RelativeYear(2025) };

            var expected = dbContext.ProducerResultFileSuggestedBillingInstruction.FirstOrDefault(t => t.BillingInstructionAcceptReject ==
            CommonConstants.Rejected && t.SuggestedBillingInstruction.ToLowerInvariant() == CommonConstants.Cancel.ToLowerInvariant()
            && t.CalculatorRunId == 2);

            materialService.Setup(t => t.GetMaterials()).ReturnsAsync(TestDataHelper.GetMaterials().ToList());

            // Act
            var result = await builder.ConstructAsync(requestDto);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreEqual(expected?.ProducerId, cancelledProducer.ProducerId);
        }

        [TestMethod]
        public async Task CancelledProducersShouldNotGetRejectedInitialProducer()
        {
            SeedDatabaseForInitialRunCompleted(dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto { RunId = 3, RelativeYear = new RelativeYear(2025) };

            materialService.Setup(t => t.GetMaterials()).ReturnsAsync(TestDataHelper.GetMaterials().ToList());

            // Act
            var result = await builder.ConstructAsync(requestDto);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.Where(t=>t.ProducerId == 3).FirstOrDefault();
            Assert.IsNull(cancelledProducer);
        }

        [TestMethod]
        public async Task CancelledProducersWithNoMaterials()
        {
            SeedDatabaseForInitialRunCompleted(dbContext);

            // Arrange
            var requestDto = new CalcResultsRequestDto { RunId = 3, RelativeYear = new RelativeYear(2025) };

            var expected = dbContext.ProducerResultFileSuggestedBillingInstruction.FirstOrDefault(t => t.BillingInstructionAcceptReject ==
            CommonConstants.Rejected && t.SuggestedBillingInstruction.ToLowerInvariant() == CommonConstants.Cancel.ToLowerInvariant()
            && t.CalculatorRunId == 2);


            // Act
            var result = await builder.ConstructAsync(requestDto);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreEqual(expected?.ProducerId, cancelledProducer.ProducerId);
            Assert.IsNull(cancelledProducer.LastTonnage?.AluminiumValue);
        }

        private void SeedDatabaseForUnclassified(ApplicationDBContext context)
        {
            //calculator runs
            var runs = new List<CalculatorRun> { new CalculatorRun { Id = 1, RelativeYear = new RelativeYear(2025), CalculatorRunClassificationId=2, Name = "CalculatorRunTest1" },
             new CalculatorRun { Id = 2, RelativeYear = new RelativeYear(2025), CalculatorRunClassificationId=2, Name = "CalculatorRunTest2" }};
            context.CalculatorRuns.AddRange(runs);



            var producerDetails = new List<ProducerDetail>
            {  new ProducerDetail { Id =1 , CalculatorRunId = 1, ProducerName="Test1", ProducerId = 1, TradingName = "TN1"},
             new ProducerDetail { Id =2 , CalculatorRunId = 1, ProducerName="Test2", ProducerId = 2, TradingName = "TN2"},
              new ProducerDetail { Id =3 , CalculatorRunId = 2, ProducerName="Test1", ProducerId = 1, TradingName = "TN3"},
               new ProducerDetail { Id =4 , CalculatorRunId = 1, ProducerName="Test3", ProducerId = 3, TradingName = "TN4"},
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



            var designatedRunInvoice = new List<ProducerDesignatedRunInvoiceInstruction>
            { new ProducerDesignatedRunInvoiceInstruction
                {
                    BillingInstructionId = "1_1",
                    CalculatorRunId = 1,
                    CurrentYearInvoicedTotalAfterThisRun = 100,
                    Id = 1,
                    ProducerId = 1,
                    InvoiceAmount = 100,
                    OutstandingBalance = 100,

                },
                new ProducerDesignatedRunInvoiceInstruction
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


            var billingInstructionList = new List<ProducerResultFileSuggestedBillingInstruction>
            {  new ProducerResultFileSuggestedBillingInstruction
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 1,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 1,
                    BillingInstructionAcceptReject = "Accepted"
                },
            new ProducerResultFileSuggestedBillingInstruction
            {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 2,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 1,
                    BillingInstructionAcceptReject = "Accepted"
                },
             new ProducerResultFileSuggestedBillingInstruction
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

            var materialInvoiceTonnage = new List<ProducerInvoicedMaterialNetTonnage>
            {
                 new ProducerInvoicedMaterialNetTonnage
                 {
                      CalculatorRunId =1,
                      MaterialId= 1,
                      InvoicedNetTonnage = 100,
                      ProducerId =1, Id=1

                 },
                new ProducerInvoicedMaterialNetTonnage
                {
                      CalculatorRunId =1,
                      MaterialId= 2,
                      InvoicedNetTonnage = 100,
                      ProducerId =1,
                    Id=2

                 },
            new ProducerInvoicedMaterialNetTonnage
            {
                      CalculatorRunId =1,
                      MaterialId= 1,
                      InvoicedNetTonnage = 100,
                      ProducerId =2, Id=3

                 },
                new ProducerInvoicedMaterialNetTonnage
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
            //calculator runs
            var runs = new List<CalculatorRun> { new CalculatorRun { Id = 1, RelativeYear = new RelativeYear(2025), CalculatorRunClassificationId=7, Name = "CalculatorRunTest1" },
             new CalculatorRun { Id = 2, RelativeYear = new RelativeYear(2025), CalculatorRunClassificationId=12, Name = "CalculatorRunTest2" },
            new CalculatorRun { Id = 3, RelativeYear = new RelativeYear(2025), CalculatorRunClassificationId=2, Name = "CalculatorRunTest3" }};

            context.CalculatorRuns.AddRange(runs);



            var producerDetails = new List<ProducerDetail>
            {  new ProducerDetail { Id =1 , CalculatorRunId = 1, ProducerName="Test1", ProducerId = 1, TradingName = "TN1"},
             new ProducerDetail { Id =2 , CalculatorRunId = 1, ProducerName="Test2", ProducerId = 2, TradingName = "TN2"},
               new ProducerDetail { Id =4 , CalculatorRunId = 1, ProducerName="Test3", ProducerId = 3, TradingName = "TN4"},
                new ProducerDetail { Id =5 , CalculatorRunId = 1, ProducerName="Test4", ProducerId = 4, TradingName = "TN5"},
                 new ProducerDetail { Id =6 , CalculatorRunId = 2, ProducerName="Test2", ProducerId = 2, TradingName = "TN2"},
              new ProducerDetail { Id =3 , CalculatorRunId = 2, ProducerName="Test1", ProducerId = 1, TradingName = "TN3"},
               new ProducerDetail { Id =7 , CalculatorRunId = 3, ProducerName="Test1", ProducerId = 1, TradingName = "TN3"}
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



            var designatedRunInvoice = new List<ProducerDesignatedRunInvoiceInstruction>
            { new ProducerDesignatedRunInvoiceInstruction
                {
                    BillingInstructionId = "1_1",
                    CalculatorRunId = 1,
                    CurrentYearInvoicedTotalAfterThisRun = 100,
                    Id = 1,
                    ProducerId = 1,
                    InvoiceAmount = 100,
                    OutstandingBalance = 100,

                },
                 new ProducerDesignatedRunInvoiceInstruction
                 {
                    BillingInstructionId = "1_2",
                    CalculatorRunId = 1,
                    CurrentYearInvoicedTotalAfterThisRun = 100,
                    Id = 2,
                    ProducerId = 2,
                    InvoiceAmount = 100,
                    OutstandingBalance = 100,

                },
                 new ProducerDesignatedRunInvoiceInstruction
                 {
                    BillingInstructionId = "1_3",
                    CalculatorRunId = 1,
                    CurrentYearInvoicedTotalAfterThisRun = 100,
                    Id = 3,
                    ProducerId = 3,
                    InvoiceAmount = 100,
                    OutstandingBalance = 100,

                },
                 new ProducerDesignatedRunInvoiceInstruction
                 {
                    BillingInstructionId = "1_4",
                    CalculatorRunId = 1,
                    CurrentYearInvoicedTotalAfterThisRun = 100,
                    Id = 4,
                    ProducerId = 4,
                    InvoiceAmount = 100,
                    OutstandingBalance = 100,

                },
                  new ProducerDesignatedRunInvoiceInstruction
                  {
                    BillingInstructionId = "2_5",
                    CalculatorRunId = 2,
                    CurrentYearInvoicedTotalAfterThisRun = 100,
                    Id = 5,
                    ProducerId = 1,
                    InvoiceAmount = 100,
                    OutstandingBalance = 100,
                },
                  new ProducerDesignatedRunInvoiceInstruction
                  {
                    BillingInstructionId = "2_6",
                    CalculatorRunId = 2,
                    CurrentYearInvoicedTotalAfterThisRun = null,
                    Id = 6,
                    ProducerId = 2,
                    InvoiceAmount = null,
                    OutstandingBalance = 100,
                },
            };


            context.ProducerDesignatedRunInvoiceInstruction.AddRange(designatedRunInvoice);


            var billingInstructionList = new List<ProducerResultFileSuggestedBillingInstruction>
            {  new ProducerResultFileSuggestedBillingInstruction
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 1,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 1,
                    BillingInstructionAcceptReject = "Accepted"
                },
            new ProducerResultFileSuggestedBillingInstruction
            {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 2,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 1,
                    BillingInstructionAcceptReject = "Accepted"
                },
             new ProducerResultFileSuggestedBillingInstruction
             {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 3,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 1,
                    BillingInstructionAcceptReject = "Rejected"
                },
              new ProducerResultFileSuggestedBillingInstruction
              {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 4,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 1,
                    BillingInstructionAcceptReject = "Accepted"
                },
               new ProducerResultFileSuggestedBillingInstruction
               {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 1,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 2,
                    BillingInstructionAcceptReject = "Accepted"
                },
                new ProducerResultFileSuggestedBillingInstruction
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 2,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 2,
                    BillingInstructionAcceptReject = "Accepted"
                },
                 new ProducerResultFileSuggestedBillingInstruction
                 {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 4,
                    SuggestedBillingInstruction = "CANCEL",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 2,
                    BillingInstructionAcceptReject = "Rejected"
                },
                  new ProducerResultFileSuggestedBillingInstruction
                  {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 4,
                    SuggestedBillingInstruction = "CANCEL",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 3,
                    BillingInstructionAcceptReject = "Accepted"
                },
                   new ProducerResultFileSuggestedBillingInstruction
                   {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 2,
                    SuggestedBillingInstruction = "CANCEL",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 3,
                    BillingInstructionAcceptReject = "Rejected"
                }
            };



            context.ProducerResultFileSuggestedBillingInstruction.AddRange(billingInstructionList);

            var materialInvoiceTonnage = new List<ProducerInvoicedMaterialNetTonnage>
            {
                 new ProducerInvoicedMaterialNetTonnage
                 {
                     CalculatorRunId =1,
                      MaterialId= 1,
                      InvoicedNetTonnage = 100,
                      ProducerId =1,
                     Id=1

                 },
                new ProducerInvoicedMaterialNetTonnage
                {
                      CalculatorRunId =1,
                      MaterialId= 2,
                      InvoicedNetTonnage = 100,
                      ProducerId =1,
                     Id=2
                 },
            new ProducerInvoicedMaterialNetTonnage
            {
                      CalculatorRunId =1,
                      MaterialId= 1,
                      InvoicedNetTonnage = 100,
                      ProducerId =2,
                      Id=3

                 },
                new ProducerInvoicedMaterialNetTonnage
                {
                      CalculatorRunId =1,
                      MaterialId= 2,
                      InvoicedNetTonnage = 100,
                      ProducerId =2,
                     Id=4

                 },
                 new ProducerInvoicedMaterialNetTonnage
                 {
                      CalculatorRunId =1,
                      MaterialId= 2,
                      InvoicedNetTonnage = 100,
                      ProducerId =3,
                     Id=5

                 },
                  new ProducerInvoicedMaterialNetTonnage
                  {
                      CalculatorRunId =1,
                      MaterialId= 2,
                      InvoicedNetTonnage = 100,
                      ProducerId =4,
                     Id=6

                 },
            new ProducerInvoicedMaterialNetTonnage
            {
                 CalculatorRunId = 2,
                 MaterialId = 2,
                 InvoicedNetTonnage = 100,
                 ProducerId = 1,
                 Id = 7

             }
             ,new ProducerInvoicedMaterialNetTonnage
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
    }
}