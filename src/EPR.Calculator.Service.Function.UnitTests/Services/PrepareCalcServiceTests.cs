using AutoFixture;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Exporter;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
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
        private Mock<ICalcBillingJsonExporter<CalcResult>> _jsonExporter;
        private Mock<IConfigurationService> _configService;
        private Mock<IBillingFileExporter<CalcResult>> _billingFileExporter;
        private Mock<IPrepareProducerDataInsertService> _prepareProducerDataInsertService;

        private PrepareCalcServiceDependencies _prepareCalcServiceDependencies;

        public PrepareCalcServiceTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDBContext(_dbContextOptions);
            _dbContextFactory = new Mock<IDbContextFactory<ApplicationDBContext>>();
            _dbContextFactory.Setup(f => f.CreateDbContext()).Returns(_context);
            _telemetryLogger = new Mock<ICalculatorTelemetryLogger>();

            SeedDatabase();

            var calcResult = new CalcResult
            {
                ShowModulations = true,
                CalcResultScaledupProducers = new CalcResultScaledupProducers(),
                CalcResultPartialObligations = new CalcResultPartialObligations(),
                CalcResultDetail = new CalcResultDetail
                {
                    RunId = 4,
                    RunDate = DateTime.UtcNow,
                    RunName = "RunName",
                    RelativeYear = new RelativeYear(2024),
                },
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    Name = string.Empty,
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetail>(),
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
                CalcResultProjectedProducers = new CalcResultProjectedProducers(),
            };

            var fixture = new Fixture();
            _rpdStatusDataValidator = new Mock<IRpdStatusDataValidator>();
            _wrapper = new Mock<IOrgAndPomWrapper>();
            _builder = new Mock<ICalcResultBuilder>();
            _builder.Setup(b => b.BuildAsync(It.IsAny<CalcResultsRequestDto>())).ReturnsAsync(calcResult);
            _exporter = new Mock<ICalcResultsExporter<CalcResult>>();
            _exporter.Setup(x => x.Export(It.IsAny<CalcResult>())).Returns("Some value");
            _transposePomAndOrgDataService = new Mock<ITransposePomAndOrgDataService>();
            _storageService = new Mock<IStorageService>();
            _validationRules = fixture.Create<CalculatorRunValidator>();
            _commandTimeoutService = new Mock<ICommandTimeoutService>();
            _jsonExporter = new Mock<ICalcBillingJsonExporter<CalcResult>>();
            _configService = new Mock<IConfigurationService>();
            _billingFileExporter = new Mock<IBillingFileExporter<CalcResult>>();
            _prepareProducerDataInsertService = new Mock<IPrepareProducerDataInsertService>();

            _prepareCalcServiceDependencies = new PrepareCalcServiceDependencies
            {
                Context = _context,
                Builder = _builder.Object,
                Exporter = _exporter.Object,
                StorageService = _storageService.Object,
                ValidationRules = new Mock<CalculatorRunValidator>().Object,
                CommandTimeoutService = _commandTimeoutService.Object,
                TelemetryLogger = new Mock<ICalculatorTelemetryLogger>().Object,
                JsonExporter = _jsonExporter.Object,
                ConfigService = _configService.Object,
                BillingFileExporter = _billingFileExporter.Object,
                producerDataInsertService = _prepareProducerDataInsertService.Object,

            };

            _testClass = new PrepareCalcService(_prepareCalcServiceDependencies);
        }

        [TestCleanup]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new PrepareCalcService(_prepareCalcServiceDependencies);

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public async Task PrepareCalcResults_ShouldReturnTrueStatus()
        {
            // Arrange
            var fixture = new Fixture();
            var resultsRequestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };
            var runName = fixture.Create<string>();

            _storageService.Setup(x => x.UploadFileContentAsync(
                It.IsAny<(string, string, string, string, bool)>()))
                .ReturnsAsync("expected result");
            // Act
            var result = await _testClass.PrepareCalcResultsAsync(resultsRequestDto, runName, CancellationToken.None);

            // Assert
            Assert.AreEqual(true, result);

            _storageService.Verify(x => x.UploadFileContentAsync(
                It.Is<(string, string, string, string, bool)>(y => y.Item5 == false)), Times.Once);
        }

        [TestMethod]
        public async Task PrepareCalcResults_Blob_Null_ShouldReturnFalseStatus()
        {
            // Arrange
            var fixture = new Fixture();
            var resultsRequestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };
            _storageService = new Mock<IStorageService>();
            var runName = fixture.Create<string>();

            // Act
            var result = await _testClass.PrepareCalcResultsAsync(resultsRequestDto, runName, CancellationToken.None);

            // Assert
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public async Task PrepareCalcResults_Exported_Exception_ShouldReturnFalseStatus()
        {
            // Arrange
            var fixture = new Fixture();
            var resultsRequestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };
            _exporter.Setup(x => x.Export(It.IsAny<CalcResult>())).Throws(new Exception("Custom exception message"));
            var runName = fixture.Create<string>();

            // Act
            var result = await _testClass.PrepareCalcResultsAsync(resultsRequestDto, runName, CancellationToken.None);

            // Assert
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public async Task PrepareCalcResults_Exported_Operation_Exception_ShouldReturnFalseStatus()
        {
            // Arrange
            var fixture = new Fixture();
            var resultsRequestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };
            _exporter.Setup(x => x.Export(It.IsAny<CalcResult>())).Throws(new OperationCanceledException("Operation canceled exception message"));
            var runName = fixture.Create<string>();

            // Act
            var result = await _testClass.PrepareCalcResultsAsync(resultsRequestDto, runName, CancellationToken.None);

            // Assert
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public async Task PrepareCalcResults_ShouldReturnFalseStatus()
        {
            // Arrange
            var resultsRequestDto = new CalcResultsRequestDto { RunId = 2, RelativeYear = new RelativeYear(2025) };
            var runName = "test";

            // Act
            var result = await _testClass.PrepareCalcResultsAsync(resultsRequestDto, runName, CancellationToken.None);

            // Assert
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public async Task PrepareCalcResults_CalcRun_ShouldReturnFalseStatus()
        {
            // Arrange
            var resultsRequestDto = new CalcResultsRequestDto { RunId = 10, RelativeYear = new RelativeYear(2025) };
            var runName = "test";

            // Act
            var result = await _testClass.PrepareCalcResultsAsync(resultsRequestDto, runName, CancellationToken.None);

            // Assert
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public async Task PrepareBillingResults_Test()
        {
            var fixture = new Fixture();
            var calcRun = _context.CalculatorRuns.Single(x => x.Id == 1);
            calcRun.IsBillingFileGenerating = true;
            await _context.SaveChangesAsync();

            _jsonExporter.Setup(t => t.Export(It.IsAny<CalcResult>(), It.IsAny<List<int>>())).Returns(fixture.Create<string>());
            _billingFileExporter.Setup(t => t.Export(It.IsAny<CalcResult>(), It.IsAny<List<int>>())).Returns(fixture.Create<string>());
            _storageService.Setup(x => x.UploadFileContentAsync(
               It.IsAny<(string, string, string, string, bool)>()))
               .ReturnsAsync("fileName");

            var calcResultsRequestDto = new CalcResultsRequestDto
            {
                RunId = 1,
                RelativeYear = new RelativeYear(2025),
                IsBillingFile = true,
                AcceptedProducerIds = new List<int> { 1, 2 },
                ApprovedBy = "Test User 234",
            };
            var billingResult = await _testClass.PrepareBillingResultsAsync(calcResultsRequestDto, "TestRun", CancellationToken.None);

            Assert.IsTrue(billingResult);
            calcRun = _context.CalculatorRuns.Single(x => x.Id == 1);
            Assert.IsFalse(calcRun.IsBillingFileGenerating);

            _builder
                .Verify(b => b.BuildAsync(It.Is<CalcResultsRequestDto>(x => x.RunId == 1 && x.IsBillingFile)), Times.Once);

            var billingFileMetaData = _context.CalculatorRunBillingFileMetadata.SingleOrDefault(x => x.CalculatorRunId == 1);

            Assert.IsNotNull(billingFileMetaData);

            billingFileMetaData.BillingFileCreatedBy = "Test User 234";
            var fileNamePart = $"1-TestRun_Billing File_{DateTime.Today:yyyyMMdd}";
            Assert.IsTrue(billingFileMetaData.BillingCsvFileName?.StartsWith(fileNamePart));
            Assert.AreEqual("1billing.json", billingFileMetaData.BillingJsonFileName);

            _storageService.Verify(x => x.UploadFileContentAsync(
                It.IsAny<(string, string, string, string, bool)>()), Times.Exactly(2));

            _storageService.Verify(x => x.UploadFileContentAsync(
                It.Is<(string, string, string, string, bool)>(y => !y.Item5)), Times.Once);

            _storageService.Verify(x => x.UploadFileContentAsync(
                It.Is<(string, string, string, string, bool)>(y => y.Item5)), Times.Once);

        }

        private void SeedDatabase()
        {
            _context.CalculatorRunOrganisationDataMaster.AddRange(GetCalculatorRunOrganisationDataMaster());
            _context.CalculatorRunOrganisationDataDetails.AddRange(GetCalculatorRunOrganisationDataDetails());

            _context.CalculatorRunPomDataMaster.AddRange(GetCalculatorRunPomDataMaster());
            _context.CalculatorRunPomDataDetails.AddRange(GetCalculatorRunPomDataDetails());

            _context.CalculatorRuns.AddRange(GetCalculatorRuns());
            _context.Material.AddRange(GetMaterials());

            _context.SaveChanges();
        }

        protected static IEnumerable<CalculatorRunOrganisationDataMaster> GetCalculatorRunOrganisationDataMaster()
        {
            var list = new List<CalculatorRunOrganisationDataMaster>
            {
                new() {
                    Id = 1,
                    RelativeYear = new RelativeYear(2024),
                    EffectiveFrom = DateTime.UtcNow,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.UtcNow,
                },
                new() {
                    Id = 2,
                    RelativeYear = new RelativeYear(2024),
                    EffectiveFrom = DateTime.UtcNow,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.UtcNow,
                },
            };
            return list;
        }

        protected static IEnumerable<CalculatorRunOrganisationDataDetail> GetCalculatorRunOrganisationDataDetails()
        {
            var list = new List<CalculatorRunOrganisationDataDetail>();
            list.AddRange(new List<CalculatorRunOrganisationDataDetail>
            {
                new() {
                    Id = 1,
                    OrganisationId = 1,
                    OrganisationName = "UPU LIMITED",
                    SubsidiaryId ="1",
                    LoadTimeStamp = DateTime.UtcNow,
                    CalculatorRunOrganisationDataMasterId = 1
                },
                new() {
                    Id = 2,
                    OrganisationId = 1,
                    OrganisationName = "Test LIMITED",
                    LoadTimeStamp = DateTime.UtcNow,
                    CalculatorRunOrganisationDataMasterId = 1
                },
                new() {
                    Id = 3,
                    OrganisationId = 2,
                    SubsidiaryId = "1",
                    OrganisationName = "Subsid2",
                    LoadTimeStamp = DateTime.UtcNow,
                    CalculatorRunOrganisationDataMasterId = 2
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
                    RelativeYear = new RelativeYear(2024),
                    EffectiveFrom = DateTime.UtcNow,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.UtcNow,
                },
                new() {
                    Id = 2,
                    RelativeYear = new RelativeYear(2024),
                    EffectiveFrom = DateTime.UtcNow,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.UtcNow,
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
                    SubsidiaryId = "1",
                    SubmissionPeriod = "2023-P2",
                    PackagingActivity = null,
                    PackagingType = "CW",
                    PackagingClass = "O1",
                    PackagingMaterial = "PC",
                    PackagingMaterialWeight = 1000,
                    LoadTimeStamp = DateTime.UtcNow,
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
                    LoadTimeStamp = DateTime.UtcNow,
                    CalculatorRunPomDataMasterId = 1,
                    SubmissionPeriodDesc = "July to December 2023",
                },
                new() {
                    Id = 3,
                    OrganisationId = 1,
                    SubsidiaryId = "1",
                    SubmissionPeriod = "2023-P1",
                    PackagingActivity = null,
                    PackagingType = "CW",
                    PackagingClass = "O1",
                    PackagingMaterial = "PC",
                    PackagingMaterialWeight = 1000,
                    LoadTimeStamp = DateTime.UtcNow,
                    CalculatorRunPomDataMasterId = 1,
                    SubmissionPeriodDesc = "January to June 2023",
                },
                new() {
                    Id = 4,
                    OrganisationId = 2,
                    SubsidiaryId = "1",
                    SubmissionPeriod = "2024-P1",
                    PackagingActivity = null,
                    PackagingType = "CW",
                    PackagingClass = "O1",
                    PackagingMaterial = "PC",
                    PackagingMaterialWeight = 1000,
                    LoadTimeStamp = DateTime.UtcNow,
                    CalculatorRunPomDataMasterId = 2,
                    SubmissionPeriodDesc = "January to June 2024",
                },
            };
            return list;
        }

        protected static IEnumerable<CalculatorRun> GetCalculatorRuns()
        {
            var list = new List<CalculatorRun>
            {
                new ()
                {
                    Id = 1,
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    Name = "Test Run",
                    RelativeYear = new RelativeYear(2024),
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
                    RelativeYear = new RelativeYear(2025),
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
                    RelativeYear = new RelativeYear(2025),
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
                    RelativeYear = new RelativeYear(2025),
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