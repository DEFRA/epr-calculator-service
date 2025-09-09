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

    [TestClass]
    public class CalcResultCancelledProducersBuilderTests
    {
        private CalcResultCancelledProducersBuilder builder;
        private readonly ApplicationDBContext dbContext;
        private Mock<IDbContextFactory<ApplicationDBContext>> dbContextFactory;

        private IMaterialService materialService;

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
            this.builder = new CalcResultCancelledProducersBuilder(
                this.dbContext, new MaterialService(this.dbContextFactory.Object));
        }

        [TestMethod]
        public async Task Construct_ShouldReturnResponseWithCorrectHeaders()
        {
            // Arrange
            var requestDto = new CalcResultsRequestDto();

            // Act
            var result = await builder.Construct(requestDto, "2025-26");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(CommonConstants.CancelledProducers, result.TitleHeader);

            var headerRow = result.CancelledProducers.FirstOrDefault();
            Assert.IsNotNull(headerRow);

            // Check main headers
            Assert.AreEqual(CommonConstants.ProducerId, headerRow.ProducerId_Header);
            Assert.AreEqual(CommonConstants.ProducerOrSubsidiaryName, headerRow.ProducerName_Header);
            Assert.AreEqual(CommonConstants.TradingName, headerRow.TradingName_Header);

            // Check LastTonnage headers
            Assert.IsNotNull(headerRow.LastTonnage);
            Assert.AreEqual(CommonConstants.LastTonnage, headerRow.LastTonnage.LastTonnage_Header);
            Assert.AreEqual(CommonConstants.Aluminium, headerRow.LastTonnage.Aluminium_Header);
            Assert.AreEqual(CommonConstants.FibreComposite, headerRow.LastTonnage.FibreComposite_Header);
            Assert.AreEqual(CommonConstants.Glass, headerRow.LastTonnage.Glass_Header);
            Assert.AreEqual(CommonConstants.PaperOrCard, headerRow.LastTonnage.PaperOrCard_Header);
            Assert.AreEqual(CommonConstants.Plastic, headerRow.LastTonnage.Plastic_Header);
            Assert.AreEqual(CommonConstants.Steel, headerRow.LastTonnage.Steel_Header);
            Assert.AreEqual(CommonConstants.Wood, headerRow.LastTonnage.Wood_Header);
            Assert.AreEqual(CommonConstants.OtherMaterials, headerRow.LastTonnage.OtherMaterials_Header);

            // Check LatestInvoice headers
            Assert.IsNotNull(headerRow.LatestInvoice);
            Assert.AreEqual(CommonConstants.LatestInvoice, headerRow.LatestInvoice.LatestInvoice_Header);
            Assert.AreEqual(CommonConstants.CurrentYearInvoicedTotalToDate, headerRow.LatestInvoice.CurrentYearInvoicedTotalToDate_Header);
            Assert.AreEqual(CommonConstants.RunNumber, headerRow.LatestInvoice.RunNumber_Header);
            Assert.AreEqual(CommonConstants.RunName, headerRow.LatestInvoice.RunName_Header);
            Assert.AreEqual(CommonConstants.BillingInstructionId, headerRow.LatestInvoice.BillingInstructionId_Header);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CalcResultCancelledProducersBuilder(this.dbContext, materialService);

            // Assert
            Assert.IsNotNull(instance);
        }
       

        [TestMethod]
        public async Task CanCallConstruct()
        {
            // Arrange
            var fixture = new Fixture();
            var resultsRequestDto = fixture.Create<CalcResultsRequestDto>();
            var financialYear = fixture.Create<string>();

            // Act
            var result = await builder.Construct(resultsRequestDto, financialYear);



            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public async Task CannotCallConstructWithNullResultsRequestDto()
        {
            // Arrange
            var fixture = new Fixture();
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => builder.Construct(default(CalcResultsRequestDto), fixture.Create<string>()));
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public async Task CannotCallConstructWithInvalidFinancialYear(string value)
        {
            // Arrange
            var fixture = new Fixture();
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => builder.Construct(fixture.Create<CalcResultsRequestDto>(), value));
        }

        [TestMethod]
        public void CanCallGetLatestProducerDetailsForThisFinancialYear()
        {
            // Arrange
            var fixture = new Fixture();
            var financialYear = fixture.Create<string>();

            // Act
            var result = builder.GetLatestProducerDetailsForThisFinancialYear(financialYear);

            // Assert
            Assert.Fail("Create or modify test");
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void CannotCallGetLatestProducerDetailsForThisFinancialYearWithInvalidFinancialYear(string value)
        {
            Assert.ThrowsException<ArgumentNullException>(() => builder.GetLatestProducerDetailsForThisFinancialYear(value));
        }

        [TestMethod]
        public void CanCallGetProducers()
        {
            // Arrange
            var fixture = new Fixture();
            var runId = fixture.Create<int>();

            // Act
            var result = builder.GetProducers(runId);

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CanCallGetCancelledProducers()
        {
            // Arrange
            var fixture = new Fixture();
            var financialYear = fixture.Create<string>();
            var runId = fixture.Create<int>();

            // Act
            var result = builder.GetCancelledProducers(financialYear, runId);

            // Assert
            Assert.Fail("Create or modify test");
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void CannotCallGetCancelledProducersWithInvalidFinancialYear(string value)
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => builder.GetCancelledProducers(value, fixture.Create<int>()));
        }
    }
}
