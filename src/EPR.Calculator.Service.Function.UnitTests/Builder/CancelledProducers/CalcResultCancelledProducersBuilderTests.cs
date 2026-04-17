using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.CancelledProducers;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.CancelledProducers
{
    [TestClass]
    public class CalcResultCancelledProducersBuilderTests
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly Mock<IMaterialService> _materialService;
        private readonly CalcResultCancelledProducersBuilder _sut;

        public CalcResultCancelledProducersBuilderTests()
        {
            _dbContext = TestFixtures.New().Create<ApplicationDBContext>();

            _materialService = new Mock<IMaterialService>();
            _materialService.Setup(t => t.GetMaterialIdsByType(It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Materials.ToImmutableDictionary(t => t.Name, t => t.Id));

            var invoicedProducerService = new InvoicedProducerService(_dbContext, new Mock<ILogger<InvoicedProducerService>>().Object);

            _sut = new CalcResultCancelledProducersBuilder(invoicedProducerService, _materialService.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [TestMethod]
        public async Task CanConstruct()
        {
            TestData.SeedDatabaseForInitialRun(_dbContext);

            // Arrange
            var runContext = TestFixtures.Default.Create<CalculatorRunContext>() with { RunId = 2 };

            var expected = _dbContext.ProducerDesignatedRunInvoiceInstruction.FirstOrDefault(t => t.ProducerId == 2 && t.CalculatorRunId == 1);

            // Act
            var result = await _sut.ConstructAsync(runContext);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreEqual(2, cancelledProducer.ProducerId);
            Assert.AreEqual("TestOrgName|Master_1|Producer_2", cancelledProducer.ProducerOrSubsidiaryNameValue);
            Assert.AreEqual(expected?.BillingInstructionId, cancelledProducer.LatestInvoice?.BillingInstructionIdValue);
            Assert.AreEqual(expected?.CalculatorRunId.ToString(), cancelledProducer.LatestInvoice?.RunNumberValue);
            Assert.AreEqual(100, cancelledProducer.LatestInvoice?.CurrentYearInvoicedTotalToDateValue);
        }


        [TestMethod]
        public async Task CanConstructWithOutCancelledProducers()
        {
            TestData.SeedDatabaseForUnclassified(_dbContext);

            // Arrange
            var runContext = TestFixtures.Default.Create<CalculatorRunContext>();

            _materialService.Setup(t => t.GetMaterials(It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Materials);

            // Act
            var result = await _sut.ConstructAsync(runContext);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.Count();
            Assert.AreEqual(0, cancelledProducer);
        }


        [TestMethod]
        public async Task CancelledProducersShouldNotGetAcceptedProducers()
        {
            TestData.SeedDatabaseForInitialRunCompleted(_dbContext);

            // Arrange
            var runContext = TestFixtures.Default.Create<CalculatorRunContext>() with { RunId = 2 };

            var expected = _dbContext.ProducerResultFileSuggestedBillingInstruction
                .FirstOrDefault(t =>
                    t.BillingInstructionAcceptReject == CommonConstants.Accepted
                    && CommonConstants.Cancel.Equals(t.SuggestedBillingInstruction, StringComparison.OrdinalIgnoreCase)
                    && t.CalculatorRunId == 2);

            // Act
            var result = await _sut.ConstructAsync(runContext);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreNotEqual(expected?.ProducerId, cancelledProducer.ProducerId);
        }

        [TestMethod]
        public async Task CancelledProducersShouldGetAcceptedProducersForBillingFile()
        {
            TestData.SeedDatabaseForInitialRunCompleted(_dbContext);

            // Arrange
            var runContext = TestFixtures.Default.Create<BillingRunContext>();

            var expected = _dbContext.ProducerResultFileSuggestedBillingInstruction
                .FirstOrDefault(t =>
                    t.BillingInstructionAcceptReject == CommonConstants.Accepted
                    && CommonConstants.Cancel.Equals(t.SuggestedBillingInstruction, StringComparison.OrdinalIgnoreCase)
                    && t.CalculatorRunId == runContext.RunId);

            // Act
            var result = await _sut.ConstructAsync(runContext);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreEqual(expected?.ProducerId, cancelledProducer.ProducerId);
        }

        [TestMethod]
        public async Task CancelledProducersShouldNotGetRejectedProducersForBillingFile()
        {
            TestData.SeedDatabaseForInitialRunCompleted(_dbContext);

            // Arrange
            var runContext = TestFixtures.Default.Create<BillingRunContext>();

            var expected = _dbContext.ProducerResultFileSuggestedBillingInstruction
                .FirstOrDefault(t =>
                    t.BillingInstructionAcceptReject == CommonConstants.Rejected
                    && CommonConstants.Cancel.Equals(t.SuggestedBillingInstruction, StringComparison.OrdinalIgnoreCase)
                    && t.CalculatorRunId == runContext.RunId);

            // Act
            var result = await _sut.ConstructAsync(runContext);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreNotEqual(expected?.ProducerId, cancelledProducer.ProducerId);
        }


        [TestMethod]
        public async Task CancelledProducersShouldGetRejectedProducers()
        {
            TestData.SeedDatabaseForInitialRunCompleted(_dbContext);

            // Arrange
            var runContext = TestFixtures.Default.Create<BillingRunContext>();

            var expected = _dbContext.ProducerResultFileSuggestedBillingInstruction
                .FirstOrDefault(t =>
                    t.BillingInstructionAcceptReject == CommonConstants.Rejected
                    && CommonConstants.Cancel.Equals(t.SuggestedBillingInstruction, StringComparison.OrdinalIgnoreCase)
                    && t.CalculatorRunId == 2);

            // Act
            var result = await _sut.ConstructAsync(runContext);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.LastOrDefault();
            Assert.IsNotNull(cancelledProducer);
            Assert.AreEqual(expected?.ProducerId, cancelledProducer.ProducerId);
        }

        [TestMethod]
        public async Task CancelledProducersShouldNotGetRejectedInitialProducer()
        {
            TestData.SeedDatabaseForInitialRunCompleted(_dbContext);

            // Arrange
            var runContext = TestFixtures.Default.Create<CalculatorRunContext>();

            // Act
            var result = await _sut.ConstructAsync(runContext);

            // Assert
            Assert.IsNotNull(result);
            var cancelledProducer = result.CancelledProducers.FirstOrDefault(t => t.ProducerId == 3);
            Assert.IsNull(cancelledProducer);
        }

        [TestMethod]
        public async Task CancelledProducersWithNoMaterials()
        {
            TestData.SeedDatabaseForInitialRunCompleted(_dbContext);

            // Arrange
            var runContext = TestFixtures.Default.Create<CalculatorRunContext>() with { RunId = 3 };

            var expected = _dbContext.ProducerResultFileSuggestedBillingInstruction
                .FirstOrDefault(t =>
                    t.BillingInstructionAcceptReject == CommonConstants.Rejected
                    && CommonConstants.Cancel.Equals(t.SuggestedBillingInstruction, StringComparison.OrdinalIgnoreCase)
                    && t.CalculatorRunId == 2);

            // Act
            var result = await _sut.ConstructAsync(runContext);

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
            new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 1, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
            new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 1, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
            new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 2, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
            new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 2, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
            new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
            new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
            new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 1, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
            new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 1, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
            new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 2, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
            new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 2, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
            new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 3, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
            new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 3, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },

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



            var producerDetails = new List<ProducerDetail>()
            {  
                new ProducerDetail() { Id =1 , CalculatorRunId = 1, ProducerName="Test1", ProducerId = 1, TradingName = "TN1"},
                new ProducerDetail() { Id =2 , CalculatorRunId = 1, ProducerName="Test2", ProducerId = 2, TradingName = "TN2"},
                new ProducerDetail() { Id =4 , CalculatorRunId = 1, ProducerName="Test3", ProducerId = 3, TradingName = "TN4"},
                new ProducerDetail() { Id =5 , CalculatorRunId = 1, ProducerName="Test4", ProducerId = 4, TradingName = "TN5"},
                new ProducerDetail() { Id =6 , CalculatorRunId = 2, ProducerName="Test2", ProducerId = 2, TradingName = "TN2"},
                new ProducerDetail() { Id =3 , CalculatorRunId = 2, ProducerName="Test1", ProducerId = 1, TradingName = "TN3"},
                new ProducerDetail() { Id =7 , CalculatorRunId = 3, ProducerName="Test1", ProducerId = 1, TradingName = "TN3"}
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
            new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 1, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
            new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 1, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
            new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 2, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
            new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 2, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
            new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
            new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
            new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 1,SubmissionPeriod = "2025-H1",  PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
            new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 1,SubmissionPeriod = "2025-H2",  PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
            new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 2, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
            new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 2, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
            new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 3, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
            new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 3, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
            new ProducerReportedMaterial { ProducerDetailId = 4, MaterialId = 1, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
            new ProducerReportedMaterial { ProducerDetailId = 4, MaterialId = 1, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
            new ProducerReportedMaterial { ProducerDetailId = 4, MaterialId = 2, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
            new ProducerReportedMaterial { ProducerDetailId = 4, MaterialId = 2, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
            new ProducerReportedMaterial { ProducerDetailId = 4, MaterialId = 3, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
            new ProducerReportedMaterial { ProducerDetailId = 4, MaterialId = 3, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
            new ProducerReportedMaterial { ProducerDetailId = 3, MaterialId = 1,SubmissionPeriod = "2025-H1",  PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
            new ProducerReportedMaterial { ProducerDetailId = 3, MaterialId = 1,SubmissionPeriod = "2025-H2",  PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
            new ProducerReportedMaterial { ProducerDetailId = 3, MaterialId = 2,SubmissionPeriod = "2025-H1",  PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
            new ProducerReportedMaterial { ProducerDetailId = 3, MaterialId = 2,SubmissionPeriod = "2025-H2",  PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
            new ProducerReportedMaterial { ProducerDetailId = 3, MaterialId = 3,SubmissionPeriod = "2025-H1",  PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
            new ProducerReportedMaterial { ProducerDetailId = 3, MaterialId = 3,SubmissionPeriod = "2025-H2",  PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },

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
                _dbContext.ProducerDetail.Add(new ProducerDetail
                {
                    ProducerId = producerId++,
                    SubsidiaryId = $"{producerId}-Sub",
                    ProducerName = producerName,
                    CalculatorRunId = 1,
                });
            }

            _dbContext.SaveChanges();

            for (int producerDetailId = 1; producerDetailId <= 10; producerDetailId++)
            {
                for (int materialId = 1; materialId < 9; materialId++)
                {
                    _dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        MaterialId = materialId,
                        ProducerDetailId = producerDetailId,
                        SubmissionPeriod = "2025-H1",
                        PackagingType = "HH",
                        PackagingTonnage = materialId * 50,
                    });
                    _dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        MaterialId = materialId,
                        ProducerDetailId = producerDetailId,
                        SubmissionPeriod = "2025-H2",
                        PackagingType = "HH",
                        PackagingTonnage = materialId * 50,
                    });
                }
            }

            _dbContext.ProducerReportedMaterial.AddRange(
                new()  {
                    MaterialId = 3,
                    ProducerDetailId = 1,
                    PackagingType = "HDC",
                    SubmissionPeriod = "2025-H1",
                    PackagingTonnage = 50,
                },
                new()  {
                    MaterialId = 3,
                    ProducerDetailId = 1,
                    PackagingType = "HDC",
                    SubmissionPeriod = "2025-H2",
                    PackagingTonnage = 50,
                },
                new() {
                    MaterialId = 3,
                    ProducerDetailId = 2,
                    PackagingType = "HDC",
                    SubmissionPeriod = "2025-H1",
                    PackagingTonnage = 100,
                },
                new() {
                    MaterialId = 3,
                    ProducerDetailId = 2,
                    PackagingType = "HDC",
                    SubmissionPeriod = "2025-H2",
                    PackagingTonnage = 50,
                },
                new() {
                    MaterialId = 2,
                    ProducerDetailId = 1,
                    PackagingType = "PB",
                    SubmissionPeriod = "2025-H1",
                    PackagingTonnage = 50,
                },
                new() {
                    MaterialId = 2,
                    ProducerDetailId = 1,
                    PackagingType = "PB",
                    SubmissionPeriod = "2025-H2",
                    PackagingTonnage = 100,
                }
            );

            _dbContext.SaveChanges();
        }

        private void CreateNewRun()
        {
            var run = new CalculatorRun
            {
                CalculatorRunClassificationId = 7,
                Name = "Test Run",
                RelativeYear = new RelativeYear(2025),
                CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User",
                DefaultParameterSettingMasterId = 1,
            };

            var run1 = new CalculatorRun
            {
                CalculatorRunClassificationId = 2,
                Name = "Test Run1",
                RelativeYear = new RelativeYear(2025),
                CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User",
                DefaultParameterSettingMasterId = 2,
            };
            _dbContext.CalculatorRuns.Add(run);
            _dbContext.CalculatorRuns.Add(run1);
            _dbContext.SaveChanges();
        }
    }
}