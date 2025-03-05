namespace EPR.Calculator.Service.Function.UnitTests.Misc
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Data;
    using EPR.Calculator.Service.Function.Data.DataModels;
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Misc;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DbLoadingChunkerTests
    {
        private const string LocalConnectionString = "Data Source=.;Initial Catalog=PayCal;Integrated Security=True;TrustServerCertificate=True";

        private const int ChunkSize = 100;

        private DbLoadingChunker<ProducerDetail> TestClass { get; set; }

        private IFixture Fixture { get; init; }

        private ApplicationDBContext Context { get; set; }

        public DbLoadingChunkerTests()
        {
            this.Fixture = new Fixture();
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            this.Context = new ApplicationDBContext(dbContextOptions);
            this.TestClass = new DbLoadingChunker<ProducerDetail>(
                new TelemetryClient(TelemetryConfiguration.CreateDefault()),
                new Mock<ICommandTimeoutService>().Object,
                this.Context,
                ChunkSize);
        }

        [TestMethod]
        public async Task CanCallInsertRecords()
        {
            // Arrange
            var numberOfRecords = 100;
            var records = this.Fixture.CreateMany<ProducerDetail>(numberOfRecords);

            // Act
            await this.TestClass.InsertRecords(records);

            // Assert
            Assert.AreEqual(numberOfRecords, await this.Context.ProducerDetail.CountAsync());
        }

        //[TestMethod]
        //public async Task InsertToLocalDb()
        //{
        //    // Arrange
        //    this.Context = new ApplicationDBContext();
        //    this.Context.Database.SetConnectionString(LocalConnectionString);
        //    this.TestClass = new DbLoadingChunker<ProducerDetail>(
        //        new TelemetryClient(TelemetryConfiguration.CreateDefault()),
        //        new Mock<ICommandTimeoutService>().Object,
        //        this.Context,
        //        1000);

        //    var calculatorRunId = 25;

        //    var numberOfRecords = 10000;

        //    var records = new List<ProducerDetail>();
        //    for (int i = 0; i < numberOfRecords; i++)
        //    {
        //        records.Add(new ProducerDetail
        //        {
        //            CalculatorRunId = calculatorRunId,
        //            ProducerId = this.Fixture.Create<int>(),
        //            ProducerName = this.Fixture.Create<string>(),
        //            SubsidiaryId = this.Fixture.Create<string>(),
        //        });
        //    }

        //    // Act
        //    await this.TestClass.InsertRecords(records, null);

        //    // Assert
        //    Assert.AreEqual(
        //        numberOfRecords,
        //        await this.Context.ProducerDetail.Where(r => r.CalculatorRunId == calculatorRunId).CountAsync());
        //}
    }
}