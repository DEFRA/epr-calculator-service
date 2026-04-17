using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Builder.ErrorReport;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.ErrorReport
{
    [TestClass]
    public class CalcResultErrorReportBuilderTests
    {
        ApplicationDBContext dbContext = null!;

        [TestInitialize]
        public void Init()
        {
            dbContext = TestFixtures.New().Create<ApplicationDBContext>();
        }

        [TestMethod]
        public async Task ConstructAsync_ReturnsMappedErrorReport()
        {
            // Arrange
            var synapseError = "Conflicting Obligations(blanks)";
            var testErrorCode = "Test Error code";

            dbContext.CalculatorRunOrganisationDataMaster.AddRange(TestData.GetCalculatorRunOrganisationDataMaster());
            dbContext.CalculatorRunOrganisationDataDetails.AddRange(TestData.GetCalculatorRunOrganisationDataDetails());
            dbContext.CalculatorRuns.AddRange(TestData.GetCaculatorRuns());

            dbContext.ErrorReports.AddRange(
                new API.Data.DataModels.ErrorReport
                {
                    Id = 1,
                    CalculatorRunId = 1,
                    ProducerId = 1,
                    SubsidiaryId = null,
                    ErrorCode = ErrorCodes.MissingPOMData,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Test user"
                },
                new API.Data.DataModels.ErrorReport
                {
                    Id = 2,
                    CalculatorRunId = 1,
                    ProducerId = 1,
                    SubsidiaryId = "Sub 1",
                    ErrorCode = ErrorCodes.MissingPOMData,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Test user"
                },
                new API.Data.DataModels.ErrorReport
                {
                    Id = 3,
                    CalculatorRunId = 1,
                    ProducerId = 1,
                    SubsidiaryId = "Sub 2",
                    ErrorCode = ErrorCodes.MissingPOMData,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Test user"
                },
                new API.Data.DataModels.ErrorReport
                {
                    Id = 5,
                    CalculatorRunId = 1,
                    ProducerId = 2,
                    SubsidiaryId = "Sub 2",
                    ErrorCode = synapseError,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Test user"
                },
                new API.Data.DataModels.ErrorReport
                {
                    Id = 6,
                    CalculatorRunId = 2,
                    ProducerId = 1,
                    SubsidiaryId = null,
                    ErrorCode = ErrorCodes.MissingRegistrationData,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Test user"
                },
                new API.Data.DataModels.ErrorReport
                {
                    Id = 7,
                    CalculatorRunId = 2,
                    ProducerId = 1,
                    SubsidiaryId = "Sub 1",
                    ErrorCode = ErrorCodes.MissingPOMData,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Test user"
                },
                new API.Data.DataModels.ErrorReport
                {
                    Id = 8,
                    CalculatorRunId = 2,
                    ProducerId = 1,
                    SubsidiaryId = "Sub 2",
                    ErrorCode = ErrorCodes.MissingPOMData,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Test user"
                },
                new API.Data.DataModels.ErrorReport
                {
                    Id = 9,
                    CalculatorRunId = 1,
                    ProducerId = 2,
                    SubsidiaryId = null,
                    ErrorCode = testErrorCode,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Test user"
                }
            );

            await dbContext.SaveChangesAsync();

            var builder = new CalcResultErrorReportBuilder(dbContext);
            var runContext = TestFixtures.Default.Create<CalculatorRunContext>();

            // Act
            var result = builder.ConstructAsync(runContext).ToList();

            Assert.AreEqual(5, result.Count);
            var report = result[0];
            Assert.AreEqual(1, report.ProducerId);
            Assert.AreEqual(CommonConstants.Hyphen, report.SubsidiaryId);
            Assert.AreEqual("Allied Packaging", report.ProducerName);
            Assert.AreEqual("Allied Trading", report.TradingName);
            Assert.AreEqual(CommonConstants.Hyphen, report.LeaverCode);
            Assert.AreEqual(ErrorCodes.MissingPOMData, report.ErrorCodeText);

            var report1 = result[1];
            Assert.AreEqual(1, report1.ProducerId);
            Assert.AreEqual("Sub 1", report1.SubsidiaryId);
            Assert.AreEqual("Allied Packaging sub 1", report1.ProducerName);
            Assert.AreEqual("Allied Trading sub 1", report1.TradingName);
            Assert.AreEqual(CommonConstants.Hyphen, report1.LeaverCode);
            Assert.AreEqual(ErrorCodes.MissingPOMData, report1.ErrorCodeText);

            var report2 = result[2];
            Assert.AreEqual(1, report2.ProducerId);
            Assert.AreEqual("Sub 2", report2.SubsidiaryId);
            Assert.AreEqual("Allied Packaging sub 2", report2.ProducerName);
            Assert.AreEqual("Allied Trading sub 2", report2.TradingName);
            Assert.AreEqual(CommonConstants.Hyphen, report2.LeaverCode);
            Assert.AreEqual(ErrorCodes.MissingPOMData, report2.ErrorCodeText);

            var report3 = result[3];
            Assert.AreEqual(2, report3.ProducerId);
            Assert.AreEqual(CommonConstants.Hyphen, report3.SubsidiaryId);
            Assert.AreEqual(CommonConstants.Hyphen, report3.ProducerName);
            Assert.AreEqual(CommonConstants.Hyphen, report3.TradingName);
            Assert.AreEqual(CommonConstants.Hyphen, report3.LeaverCode);
            Assert.AreEqual(testErrorCode, report3.ErrorCodeText);

            var report4 = result[4];
            Assert.AreEqual(2, report4.ProducerId);
            Assert.AreEqual("Sub 2", report4.SubsidiaryId);
            Assert.AreEqual(CommonConstants.Hyphen, report4.ProducerName);
            Assert.AreEqual(CommonConstants.Hyphen, report4.TradingName);
            Assert.AreEqual(CommonConstants.Hyphen, report4.LeaverCode);
            Assert.AreEqual(synapseError, report4.ErrorCodeText);
        }

        [TestMethod]
        public async Task ConstructAsync_NullSubsidiaryId_MapsToEmptyString()
        {
            // Arrange
            dbContext.CalculatorRunOrganisationDataMaster.AddRange(TestData.GetCalculatorRunOrganisationDataMaster());
            dbContext.CalculatorRunOrganisationDataDetails.AddRange(TestData.GetCalculatorRunOrganisationDataDetails());
            dbContext.CalculatorRuns.AddRange(TestData.GetCaculatorRuns());

            dbContext.ErrorReports.Add(new API.Data.DataModels.ErrorReport
            {
                Id = 1,
                CalculatorRunId = 1,
                ProducerId = 1,
                SubsidiaryId = null,
                ErrorCode = ErrorCodes.MissingRegistrationData,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user"
            });

            await dbContext.SaveChangesAsync();

            var builder = new CalcResultErrorReportBuilder(dbContext);
            var runContext = TestFixtures.Default.Create<CalculatorRunContext>();

            // Act
            var result = builder.ConstructAsync(runContext).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            var report = result[0];
            Assert.AreEqual(1, report.ProducerId);
            Assert.AreEqual(CommonConstants.Hyphen, report.SubsidiaryId);
            Assert.AreEqual("Allied Packaging", report.ProducerName);
            Assert.AreEqual("Allied Trading", report.TradingName);
            Assert.AreEqual(CommonConstants.Hyphen, report.LeaverCode);
            Assert.AreEqual(ErrorCodes.MissingRegistrationData, report.ErrorCodeText);
        }

        [TestMethod]
        public async Task ConstructAsync_ReturnsProducerNameMappedErrorReport()
        {
            // Arrange
            dbContext.CalculatorRunOrganisationDataMaster.AddRange(TestData.GetCalculatorRunOrganisationDataMaster());
            dbContext.CalculatorRunOrganisationDataDetails.AddRange(TestData.GetCalculatorRunOrganisationDataDetails());
            dbContext.CalculatorRuns.AddRange(TestData.GetCaculatorRuns());

            dbContext.ErrorReports.Add(new API.Data.DataModels.ErrorReport
            {
                Id = 1,
                CalculatorRunId = 1,
                ProducerId = 2,
                SubsidiaryId = "SUB-2",
                ErrorCode = ErrorCodes.MissingRegistrationData,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user"
            });

            await dbContext.SaveChangesAsync();

            var builder = new CalcResultErrorReportBuilder(dbContext);
            var runContext = TestFixtures.Default.Create<CalculatorRunContext>();

            // Act
            var result = builder.ConstructAsync(runContext).ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            var report = result[0];
            Assert.AreEqual(2, report.ProducerId);
            Assert.AreEqual("SUB-2", report.SubsidiaryId);
            Assert.AreEqual(CommonConstants.Hyphen, report.ProducerName);
            Assert.AreEqual(CommonConstants.Hyphen, report.TradingName);
            Assert.AreEqual(CommonConstants.Hyphen, report.LeaverCode);
            Assert.AreEqual(ErrorCodes.MissingRegistrationData, report.ErrorCodeText);
        }


    }
}