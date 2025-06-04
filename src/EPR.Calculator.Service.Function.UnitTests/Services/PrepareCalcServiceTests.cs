using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Exporter;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.Service.Function.Builder;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Misc;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.ApplicationInsights;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PrepareCalcServiceTests
    {
        private readonly DbContextOptions<ApplicationDBContext> _dbContextOptions;
        private PrepareCalcService _testClass;
        private ApplicationDBContext _context;
        private Mock<IDbContextFactory<ApplicationDBContext>> _dbContextFactory;
        private Mock<IRpdStatusDataValidator> _rpdStatusDataValidator;
        private Mock<IOrgAndPomWrapper> _wrapper;
        private Mock<ICalcResultBuilder> _builder;
        private Mock<ICalcResultsExporter<CalcResult>> _exporter;
        private Mock<ITransposePomAndOrgDataService> _transposePomAndOrgDataService;
        private Mock<IStorageService> _storageService;
        private CalculatorRunValidator _validationRules;
        private Mock<ICommandTimeoutService> _commandTimeoutService;
        private Mock<ICalculatorTelemetryLogger> _telemetryLogger;
        private Mock<IBillingInstructionService> _billingInstructionService;

        public PrepareCalcServiceTests()
        {
            this._dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            this._context = new ApplicationDBContext(this._dbContextOptions);
            this._dbContextFactory = new Mock<IDbContextFactory<ApplicationDBContext>>();
            this._dbContextFactory.Setup(f => f.CreateDbContext()).Returns(this._context);
            this._telemetryLogger = new Mock<ICalculatorTelemetryLogger>();

            this.SeedDatabase();

            var calcResult = new CalcResult
            {
                CalcResultScaledupProducers = new CalcResultScaledupProducers(),
                CalcResultDetail = new CalcResultDetail
                {
                    RunId = 4,
                    RunDate = DateTime.Now,
                    RunName = "RunName",
                },
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    Name = string.Empty,
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>(),
                },
                CalcResultParameterOtherCost = new()
                {
                    BadDebtProvision = new KeyValuePair<string, string>(),
                    Name = string.Empty,
                    Details = new List<CalcResultParameterOtherCostDetail>(),
                    Materiality = new List<CalcResultMateriality>(),
                    SaOperatingCost = new List<CalcResultParameterOtherCostDetail>(),
                    SchemeSetupCost = new CalcResultParameterOtherCostDetail(),
                },
                CalcResultLateReportingTonnageData = new()
                {
                    Name = string.Empty,
                    CalcResultLateReportingTonnageDetails = new List<CalcResultLateReportingTonnageDetail>(),
                    MaterialHeading = string.Empty,
                    TonnageHeading = string.Empty,
                },
            };

            var fixture = new Fixture();
            this._rpdStatusDataValidator = new Mock<IRpdStatusDataValidator>();
            this._wrapper = new Mock<IOrgAndPomWrapper>();
            this._builder = new Mock<ICalcResultBuilder>();
            this._builder.Setup(b => b.Build(It.IsAny<CalcResultsRequestDto>())).ReturnsAsync(calcResult);
            this._exporter = new Mock<ICalcResultsExporter<CalcResult>>();
            this._exporter.Setup(x => x.Export(It.IsAny<CalcResult>())).Returns("Some value");
            this._transposePomAndOrgDataService = new Mock<ITransposePomAndOrgDataService>();
            this._storageService = new Mock<IStorageService>();
            this._validationRules = fixture.Create<CalculatorRunValidator>();
            this._commandTimeoutService = new Mock<ICommandTimeoutService>();
            this._billingInstructionService = new Mock<IBillingInstructionService>();
            this._testClass = new PrepareCalcService(this._dbContextFactory.Object,
                this._rpdStatusDataValidator.Object,
                this._wrapper.Object,
                this._builder.Object,
                this._exporter.Object,
                this._transposePomAndOrgDataService.Object,
                this._storageService.Object,
                this._validationRules,
                this._commandTimeoutService.Object,
                this._telemetryLogger.Object,
                this._billingInstructionService.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            this._context.Database.EnsureDeleted();
            this._context.Dispose();

            // Dispose of the DbContextFactory if it implements IDisposable
            if (this._dbContextFactory is IDisposable disposableFactory)
            {
                disposableFactory.Dispose();
            }
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new PrepareCalcService(this._dbContextFactory.Object,
                this._rpdStatusDataValidator.Object,
                this._wrapper.Object,
                this._builder.Object,
                this._exporter.Object,
                this._transposePomAndOrgDataService.Object,
                this._storageService.Object,
                this._validationRules,
                this._commandTimeoutService.Object,
                this._telemetryLogger.Object,
                this._billingInstructionService.Object);

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public async Task PrepareCalcResults_ShouldReturnTrueStatus()
        {
            // Arrange
            var fixture = new Fixture();
            var resultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            var runName = fixture.Create<string>();

            this._storageService.Setup(x => x.UploadResultFileContentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("expected result");
            // Act
            var result = await this._testClass.PrepareCalcResults(resultsRequestDto, runName, CancellationToken.None);

            // Assert
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public async Task PrepareCalcResults_Blob_Null_ShouldReturnFalseStatus()
        {
            // Arrange
            var fixture = new Fixture();
            var resultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            this._storageService = new Mock<IStorageService>();
            var runName = fixture.Create<string>();

            // Act
            var result = await this._testClass.PrepareCalcResults(resultsRequestDto, runName, CancellationToken.None);

            // Assert
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public async Task PrepareCalcResults_Exported_Exception_ShouldReturnFalseStatus()
        {
            // Arrange
            var fixture = new Fixture();
            var resultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            this._exporter.Setup(x => x.Export(It.IsAny<CalcResult>())).Throws(new Exception("Custom exception message"));
            var runName = fixture.Create<string>();

            // Act
            var result = await this._testClass.PrepareCalcResults(resultsRequestDto, runName, CancellationToken.None);

            // Assert
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public async Task PrepareCalcResults_Exported_Operation_Exception_ShouldReturnFalseStatus()
        {
            // Arrange
            var fixture = new Fixture();
            var resultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            this._exporter.Setup(x => x.Export(It.IsAny<CalcResult>())).Throws(new OperationCanceledException("Operation canceled exception message"));
            var runName = fixture.Create<string>();

            // Act
            var result = await this._testClass.PrepareCalcResults(resultsRequestDto, runName, CancellationToken.None);

            // Assert
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public async Task PrepareCalcResults_ShouldReturnFalseStatus()
        {
            // Arrange
            var resultsRequestDto = new CalcResultsRequestDto { RunId = 2 };
            var runName = "test";

            // Act
            var result = await this._testClass.PrepareCalcResults(resultsRequestDto, runName, CancellationToken.None);

            // Assert
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public async Task PrepareCalcResults_CalcRun_ShouldReturnFalseStatus()
        {
            // Arrange
            var resultsRequestDto = new CalcResultsRequestDto { RunId = 10 };
            var runName = "test";

            // Act
            var result = await this._testClass.PrepareCalcResults(resultsRequestDto, runName, CancellationToken.None);

            // Assert
            Assert.AreEqual(false, result);
        }

        private void SeedDatabase()
        {
            this._context.CalculatorRunOrganisationDataMaster.AddRange(GetCalculatorRunOrganisationDataMaster());
            this._context.CalculatorRunOrganisationDataDetails.AddRange(GetCalculatorRunOrganisationDataDetails());

            this._context.CalculatorRunPomDataMaster.AddRange(GetCalculatorRunPomDataMaster());
            this._context.CalculatorRunPomDataDetails.AddRange(GetCalculatorRunPomDataDetails());

            this._context.CalculatorRuns.AddRange(GetCalculatorRuns());
            this._context.Material.AddRange(GetMaterials());

            this._context.SaveChanges();
        }

        protected static IEnumerable<CalculatorRunOrganisationDataMaster> GetCalculatorRunOrganisationDataMaster()
        {
            var list = new List<CalculatorRunOrganisationDataMaster>
            {
                new() {
                    Id = 1,
                    CalendarYear = "2024-25",
                    EffectiveFrom = DateTime.Now,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.Now,
                },
                new() {
                    Id = 2,
                    CalendarYear = "2024-25",
                    EffectiveFrom = DateTime.Now,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.Now,
                },
            };
            return list;
        }

        protected static IEnumerable<CalculatorRunOrganisationDataDetail> GetCalculatorRunOrganisationDataDetails()
        {
            var list = new List<CalculatorRunOrganisationDataDetail>();
            list.AddRange(new List<CalculatorRunOrganisationDataDetail>()
            {
                new() {
                    Id = 1,
                    OrganisationId = 1,
                    OrganisationName = "UPU LIMITED",
                    SubsidaryId ="1",
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunOrganisationDataMasterId = 1,
                    SubmissionPeriodDesc = "January to June 2023",
                },
                new() {
                    Id = 2,
                    OrganisationId = 1,
                    OrganisationName = "Test LIMITED",
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunOrganisationDataMasterId = 1,
                    SubmissionPeriodDesc = "July to December 2023",
                },
                new() {
                    Id = 3,
                    OrganisationId = 2,
                    SubsidaryId = "1",
                    OrganisationName = "Subsid2",
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunOrganisationDataMasterId = 2,
                    SubmissionPeriodDesc = "July to December 2023",
                },
            });
            return list;
        }

        protected static IEnumerable<Material> GetMaterials()
        {
            var list = new List<Material>
            {
                new() {
                    Id = 1,
                    Code = "AL",
                    Name = "Aluminium",
                    Description = "Aluminium",
                },
                new() {
                    Id = 2,
                    Code = "FC",
                    Name = "Fibre composite",
                    Description = "Fibre composite",
                },
                new() {
                    Id = 3,
                    Code = "GL",
                    Name = "Glass",
                    Description = "Glass",
                },
                new() {
                    Id = 4,
                    Code = "PC",
                    Name = "Paper or card",
                    Description = "Paper or card",
                },
                new() {
                    Id = 5,
                    Code = "PL",
                    Name = "Plastic",
                    Description = "Plastic",
                },
                new() {
                    Id = 6,
                    Code = "ST",
                    Name = "Steel",
                    Description = "Steel",
                },
                new() {
                    Id = 7,
                    Code = "WD",
                    Name = "Wood",
                    Description = "Wood",
                },
                new() {
                    Id = 8,
                    Code = "OT",
                    Name = "Other materials",
                    Description = "Other materials",
                },
            };
            return list;
        }

        protected static IEnumerable<CalculatorRunPomDataMaster> GetCalculatorRunPomDataMaster()
        {
            var list = new List<CalculatorRunPomDataMaster>
            {
                new() {
                    Id = 1,
                    CalendarYear = "2024-25",
                    EffectiveFrom = DateTime.Now,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.Now,
                },
                new() {
                    Id = 2,
                    CalendarYear = "2024-25",
                    EffectiveFrom = DateTime.Now,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.Now,
                },
            };
            return list;
        }

        protected static IEnumerable<CalculatorRunPomDataDetail> GetCalculatorRunPomDataDetails()
        {
            var list = new List<CalculatorRunPomDataDetail>
            {
                new() {
                    Id = 1,
                    OrganisationId = 1,
                    SubsidaryId = "1",
                    SubmissionPeriod = "2023-P2",
                    PackagingActivity = null,
                    PackagingType = "CW",
                    PackagingClass = "O1",
                    PackagingMaterial = "PC",
                    PackagingMaterialWeight = 1000,
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunPomDataMasterId = 1,
                    SubmissionPeriodDesc = "July to December 2023",
                },
                new() {
                    Id = 2,
                    OrganisationId = 1,
                    SubmissionPeriod = "2023-P2",
                    PackagingActivity = null,
                    PackagingType = "CW",
                    PackagingClass = "O1",
                    PackagingMaterial = "PC",
                    PackagingMaterialWeight = 1000,
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunPomDataMasterId = 1,
                    SubmissionPeriodDesc = "July to December 2023",
                },
                new() {
                    Id = 3,
                    OrganisationId = 1,
                    SubsidaryId = "1",
                    SubmissionPeriod = "2023-P1",
                    PackagingActivity = null,
                    PackagingType = "CW",
                    PackagingClass = "O1",
                    PackagingMaterial = "PC",
                    PackagingMaterialWeight = 1000,
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunPomDataMasterId = 1,
                    SubmissionPeriodDesc = "January to June 2023",
                },
                new() {
                    Id = 4,
                    OrganisationId = 2,
                    SubsidaryId = "1",
                    SubmissionPeriod = "2024-P1",
                    PackagingActivity = null,
                    PackagingType = "CW",
                    PackagingClass = "O1",
                    PackagingMaterial = "PC",
                    PackagingMaterialWeight = 1000,
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunPomDataMasterId = 2,
                    SubmissionPeriodDesc = "January to June 2024",
                },
            };
            return list;
        }

        protected static IEnumerable<CalculatorRun> GetCalculatorRuns()
        {
            var calculatorRunFinancialYear = new CalculatorRunFinancialYear { Name = "2024-25" };
            var list = new List<CalculatorRun>
            {
                new ()
                {
                    Id = 1,
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    Name = "Test Run",
                    Financial_Year = calculatorRunFinancialYear,
                    CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                    CalculatorRunOrganisationDataMasterId = 2,
                    CalculatorRunPomDataMasterId = 2,
                    DefaultParameterSettingMasterId = 5,
                    LapcapDataMasterId = 6,
                },
                new ()
                {
                    Id = 2,
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    Name = "Test Calculated Result",
                    Financial_Year = calculatorRunFinancialYear,
                    CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                    DefaultParameterSettingMasterId = 5,
                    LapcapDataMasterId = 6,
                },
                new ()
                {
                    Id = 3,
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    Name = "Test Run",
                    Financial_Year = calculatorRunFinancialYear,
                    CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                    CalculatorRunOrganisationDataMasterId = 1,
                    CalculatorRunPomDataMasterId = 1,
                    DefaultParameterSettingMasterId = 5,
                    LapcapDataMasterId = 6,
                },
                new ()
                {
                    Id = 4,
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    Name = "Test Calculated Result",
                    Financial_Year = calculatorRunFinancialYear,
                    CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                    CalculatorRunOrganisationDataMasterId = 2,
                    CalculatorRunPomDataMasterId = 2,
                    DefaultParameterSettingMasterId = 5,
                    LapcapDataMasterId = 6,
                },
            };
            return list;
        }
    }
}