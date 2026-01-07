using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.ErrorReport;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.ErrorReport
{
    [TestClass]
    public class CalcResultErrorReportBuilderTests
    {
        private ApplicationDBContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return (ApplicationDBContext)Activator.CreateInstance(typeof(ApplicationDBContext), options)!;
        }

        [TestMethod]
        public async Task ConstructAsync_ReturnsMappedErrorReport()
        {
            // Arrange
            var synapseError = "Conflicting Obligations(blanks)";
            await using var context = CreateDbContext();

            context.CalculatorRunOrganisationDataMaster.AddRange(TestDataHelper.GetCalculatorRunOrganisationDataMaster());
            context.CalculatorRunOrganisationDataDetails.AddRange(TestDataHelper.GetCalculatorRunOrganisationDataDetails());
            context.CalculatorRuns.AddRange(TestDataHelper.GetCaculatorRuns());

            context.ErrorReports.AddRange(
                new EPR.Calculator.API.Data.DataModels.ErrorReport
                {
                    Id = 1,
                    CalculatorRunId = 1,
                    ProducerId = 1,
                    SubsidiaryId = null,
                    ErrorCode = ErrorCodes.MissingPOMData,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Test user"
                },
                new EPR.Calculator.API.Data.DataModels.ErrorReport
                {
                    Id = 2,
                    CalculatorRunId = 1,
                    ProducerId = 1,
                    SubsidiaryId = "Sub 1",
                    ErrorCode = ErrorCodes.MissingPOMData,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Test user"
                },
                new EPR.Calculator.API.Data.DataModels.ErrorReport
                {
                    Id = 3,
                    CalculatorRunId = 1,
                    ProducerId = 1,
                    SubsidiaryId = "Sub 2",
                    ErrorCode = ErrorCodes.MissingPOMData,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Test user"
                },
                new EPR.Calculator.API.Data.DataModels.ErrorReport
                {
                    Id = 5,
                    CalculatorRunId = 1,
                    ProducerId = 2,
                    SubsidiaryId = "Sub 2",
                    ErrorCode = synapseError,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Test user"
                },
                new EPR.Calculator.API.Data.DataModels.ErrorReport
                {
                    Id = 6,
                    CalculatorRunId = 2,
                    ProducerId = 1,
                    SubsidiaryId = null,
                    ErrorCode = ErrorCodes.MissingRegistrationData,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Test user"
                },
                new EPR.Calculator.API.Data.DataModels.ErrorReport
                {
                    Id = 7,
                    CalculatorRunId = 2,
                    ProducerId = 1,
                    SubsidiaryId = "Sub 1",
                    ErrorCode = ErrorCodes.MissingPOMData,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Test user"
                },
                new EPR.Calculator.API.Data.DataModels.ErrorReport
                {
                    Id = 8,
                    CalculatorRunId = 2,
                    ProducerId = 1,
                    SubsidiaryId = "Sub 2",
                    ErrorCode = ErrorCodes.MissingPOMData,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Test user"
                }     
            );

            await context.SaveChangesAsync();

            var builder = new CalcResultErrorReportBuilder(context);
            var request = new CalcResultsRequestDto { RunId = 1 };

            // Act
            var result = (await builder.ConstructAsync(request)).ToList();

            Assert.AreEqual(4, result.Count);
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
            Assert.AreEqual("Sub 2", report3.SubsidiaryId);
            Assert.AreEqual(CommonConstants.Hyphen, report3.ProducerName);
            Assert.AreEqual(CommonConstants.Hyphen, report3.TradingName);
            Assert.AreEqual(CommonConstants.Hyphen, report3.LeaverCode);
            Assert.AreEqual(synapseError, report3.ErrorCodeText);
        }

        [TestMethod]
        public async Task ConstructAsync_NullSubsidiaryId_MapsToEmptyString()
        {
            // Arrange
            await using var context = CreateDbContext();

            context.CalculatorRunOrganisationDataMaster.AddRange(TestDataHelper.GetCalculatorRunOrganisationDataMaster());
            context.CalculatorRunOrganisationDataDetails.AddRange(TestDataHelper.GetCalculatorRunOrganisationDataDetails());
            context.CalculatorRuns.AddRange(TestDataHelper.GetCaculatorRuns());

            context.ErrorReports.Add(new EPR.Calculator.API.Data.DataModels.ErrorReport
            {
                Id = 1,
                CalculatorRunId = 1,
                ProducerId = 1,
                SubsidiaryId = null,
                ErrorCode = ErrorCodes.MissingRegistrationData,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user"
            });

            await context.SaveChangesAsync();

            var builder = new CalcResultErrorReportBuilder(context);
            var request = new CalcResultsRequestDto { RunId = 1 };

            // Act
            var result = (await builder.ConstructAsync(request)).ToList();

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
            await using var context = CreateDbContext();

            context.CalculatorRunOrganisationDataMaster.AddRange(TestDataHelper.GetCalculatorRunOrganisationDataMaster());
            context.CalculatorRunOrganisationDataDetails.AddRange(TestDataHelper.GetCalculatorRunOrganisationDataDetails());
            context.CalculatorRuns.AddRange(TestDataHelper.GetCaculatorRuns());

            context.ErrorReports.Add(new EPR.Calculator.API.Data.DataModels.ErrorReport
            {
                Id = 1,
                CalculatorRunId = 1,
                ProducerId = 2,
                SubsidiaryId = "SUB-2",
                ErrorCode = ErrorCodes.MissingRegistrationData,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user"
            });

            await context.SaveChangesAsync();

            var builder = new CalcResultErrorReportBuilder(context);
            var request = new CalcResultsRequestDto { RunId = 1 };

            // Act
            var result = (await builder.ConstructAsync(request)).ToList();

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
