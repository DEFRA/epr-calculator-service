using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.ErrorReport;
using EPR.Calculator.Service.Function.Dtos;
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
            await using var context = CreateDbContext();

            context.ErrorTypes.Add(new ErrorType { Id = 1, Name = "ErrorName" });

            context.CalculatorRunOrganisationDataMaster.AddRange(TestDataHelper.GetCalculatorRunOrganisationDataMaster());
            context.CalculatorRunOrganisationDataDetails.AddRange(TestDataHelper.GetCalculatorRunOrganisationDataDetails());
            context.CalculatorRuns.AddRange(TestDataHelper.GetCaculatorRuns());

            context.ErrorReports.Add(new EPR.Calculator.API.Data.DataModels.ErrorReport
            {
                Id = 1,
                CalculatorRunId = 1,
                ProducerId = 123,
                SubsidiaryId = "SUB-1",
                ErrorTypeId = 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user"
            });

            await context.SaveChangesAsync();

            var builder = new CalcResultErrorReportBuilder(context);
            var request = new CalcResultsRequestDto { RunId = 1 };

            // Act
            var result = (await builder.ConstructAsync(request)).ToList();

            // Assert
            Assert.AreEqual(2, result.Count);
            var report = result[0];
            Assert.AreEqual(123, report.ProducerId);
            Assert.AreEqual("SUB-1", report.SubsidiaryId);
            Assert.AreEqual("Allied Packaging", report.ProducerName);
            Assert.AreEqual(string.Empty, report.TradingName);
            Assert.AreEqual(string.Empty, report.LeaverCode);
            Assert.AreEqual("ErrorName", report.ErrorCodeText);
        }

        [TestMethod]
        public async Task ConstructAsync_NullSubsidiaryId_MapsToEmptyString()
        {
            // Arrange
            await using var context = CreateDbContext();

            context.ErrorTypes.Add(new ErrorType { Id = 1, Name = "ErrorName" });

            context.CalculatorRunOrganisationDataMaster.AddRange(TestDataHelper.GetCalculatorRunOrganisationDataMaster());
            context.CalculatorRunOrganisationDataDetails.AddRange(TestDataHelper.GetCalculatorRunOrganisationDataDetails());
            context.CalculatorRuns.AddRange(TestDataHelper.GetCaculatorRuns());

            context.ErrorReports.Add(new EPR.Calculator.API.Data.DataModels.ErrorReport
            {
                Id = 1,
                CalculatorRunId = 1,
                ProducerId = 123,
                SubsidiaryId = null,
                ErrorTypeId = 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user"
            });

            await context.SaveChangesAsync();

            var builder = new CalcResultErrorReportBuilder(context);
            var request = new CalcResultsRequestDto { RunId = 1 };

            // Act
            var result = (await builder.ConstructAsync(request)).ToList();

            // Assert
            Assert.AreEqual(2, result.Count);
            var report = result[0];
            Assert.AreEqual(123, report.ProducerId);
            Assert.AreEqual(string.Empty, report.SubsidiaryId);
            Assert.AreEqual("Allied Packaging", report.ProducerName);
            Assert.AreEqual(string.Empty, report.TradingName);
            Assert.AreEqual(string.Empty, report.LeaverCode);
            Assert.AreEqual("ErrorName", report.ErrorCodeText);
        }
    }
}
