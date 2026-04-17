using AutoFixture;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    /// <summary>Unit tests for <see cref="DbLoadingChunkerService{TRecord}"/>.</summary>
    [TestClass]
    public class DbLoadingChunkerServiceTests
    {
        private const int ChunkSize = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbLoadingChunkerServiceTests"/> class.
        /// </summary>
        public DbLoadingChunkerServiceTests()
        {
            Fixture = new Fixture();
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            Context = new ApplicationDBContext(dbContextOptions);

            ProducerDetailTestClass = new DbLoadingChunkerService<ProducerDetail>(
                new TelemetryClient(TelemetryConfiguration.CreateDefault()),
                new Mock<ICommandTimeoutService>().Object,
                Context,
                ChunkSize);

            ProducerReportedMaterialTestClass = new DbLoadingChunkerService<ProducerReportedMaterial>(
                new TelemetryClient(TelemetryConfiguration.CreateDefault()),
                new Mock<ICommandTimeoutService>().Object,
                Context,
                ChunkSize);
        }

        private DbLoadingChunkerService<ProducerDetail> ProducerDetailTestClass { get; set; }

        private DbLoadingChunkerService<ProducerReportedMaterial> ProducerReportedMaterialTestClass { get; set; }

        private IFixture Fixture { get; init; }

        private ApplicationDBContext Context { get; set; }

        /// <summary>
        /// Checks that the <see cref="DbLoadingChunkerService{TRecord}.InsertRecords"/> method inserts 
        /// the expected number of records.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task CanCallInsertRecords()
        {
            // Arrange
            var numberOfRecords = 100;
            var records = Fixture.CreateMany<ProducerDetail>(numberOfRecords);

            // Act
            await ProducerDetailTestClass.InsertRecords(records);

            // Assert
            Assert.AreEqual(numberOfRecords, await Context.ProducerDetail.CountAsync());
        }

        /// <summary>
        /// Checks the insert of a producer detail record.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task Transpose_Should_Return_Correct_Producer_Detail()
        {
            var expectedResult = new ProducerDetail
            {
                Id = 1,
                ProducerId = 1,
                ProducerName = "UPU LIMITED",
                CalculatorRunId = 1,
                CalculatorRun = Fixture.Create<CalculatorRun>(),
            };

            await ProducerDetailTestClass.InsertRecords([expectedResult]);

            var producerDetail = await Context.ProducerDetail.FirstOrDefaultAsync();
            Assert.IsNotNull(producerDetail);
            Assert.AreEqual(expectedResult.ProducerId, producerDetail.ProducerId);
            Assert.AreEqual(expectedResult.ProducerName, producerDetail.ProducerName);
        }

        /// <summary>
        /// Checks the insert of a producer reported material record.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task Transpose_Should_Return_Correct_Producer_Reported_Material()
        {
            var producer = new ProducerDetail
            {
                Id = 1,
                ProducerId = 1,
                SubsidiaryId = "1",
                ProducerName = "UPU LIMITED",
                CalculatorRunId = 1,
                CalculatorRun = this.Fixture.Create<CalculatorRun>(),
            };
            var material = new Material
            {
                Id = 4,
                Code = "PC",
                Name = "Paper or card",
                Description = "Paper or card",
            };
            var expectedResult = new List<ProducerReportedMaterial>(){
                new(){
                    MaterialId = 4,
                    ProducerDetailId = 1,
                    PackagingType = "CW",
                    PackagingTonnage = 1,
                    SubmissionPeriod = "2025-H1",
                    Material = material,
                    ProducerDetail = producer
                },
                new(){
                    MaterialId = 4,
                    ProducerDetailId = 1,
                    PackagingType = "CW",
                    PackagingTonnage = 1,
                    SubmissionPeriod = "2025-H2",
                    Material = material,
                    ProducerDetail = producer
                }
            };
            await this.ProducerReportedMaterialTestClass.InsertRecords(expectedResult);

            var producerReportedMaterial = this.Context.ProducerReportedMaterial.ToList();
            Assert.IsNotNull(producerReportedMaterial);
            Assert.AreEqual(2, producerReportedMaterial.Count());
            Assert.AreEqual(expectedResult[0].Material!.Code, producerReportedMaterial[0].Material!.Code);
            Assert.AreEqual(expectedResult[0].Material!.Name, producerReportedMaterial[0].Material!.Name);
            Assert.AreEqual(expectedResult[0].ProducerDetail!.ProducerId, producerReportedMaterial[0].ProducerDetail!.ProducerId);
            Assert.AreEqual(expectedResult[0].ProducerDetail!.ProducerName, producerReportedMaterial[0].ProducerDetail!.ProducerName);
            Assert.AreEqual(expectedResult[1].Material!.Code, producerReportedMaterial[1].Material!.Code);
            Assert.AreEqual(expectedResult[1].Material!.Name, producerReportedMaterial[1].Material!.Name);
            Assert.AreEqual(expectedResult[1].ProducerDetail!.ProducerId, producerReportedMaterial[1].ProducerDetail!.ProducerId);
            Assert.AreEqual(expectedResult[1].ProducerDetail!.ProducerName, producerReportedMaterial[1].ProducerDetail!.ProducerName);
        }

        /// <summary>
        /// Checks the insert of a producer subsidary record.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task Transpose_Should_Return_Correct_Producer_Subsidary_Detail()
        {
            var expectedResult = new ProducerDetail
            {
                Id = 1,
                ProducerId = 2,
                SubsidiaryId = "1",
                ProducerName = "Subsid2",
                CalculatorRunId = 1,
                CalculatorRun = Fixture.Create<CalculatorRun>(),
            };

            await ProducerDetailTestClass.InsertRecords([expectedResult]);

            var producerDetail = await Context.ProducerDetail.FirstOrDefaultAsync(t => t.SubsidiaryId != null);
            Assert.IsNotNull(producerDetail);
            Assert.AreEqual(expectedResult.ProducerId, producerDetail.ProducerId);
            Assert.AreEqual(expectedResult.ProducerName, producerDetail.ProducerName);
        }

        /// <summary>
        /// Checks the insert of a producer detail record without a submission period.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task Transpose_Should_Return_Correct_Producer_Detail_When_Submission_Period_Not_Exists()
        {
            var expectedResult = new ProducerDetail
            {
                Id = 1,
                ProducerId = 2,
                ProducerName = "Subsid2",
                CalculatorRunId = 1,
                CalculatorRun = Fixture.Create<CalculatorRun>(),
            };

            await ProducerDetailTestClass.InsertRecords([expectedResult]);

            var producerDetail = await Context.ProducerDetail.FirstOrDefaultAsync();
            Assert.IsNotNull(producerDetail);
            Assert.AreEqual(expectedResult.ProducerId, producerDetail.ProducerId);
            Assert.AreEqual(expectedResult.ProducerName, producerDetail.ProducerName);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new DbLoadingChunkerService<string>(
                new TelemetryClient(TelemetryConfiguration.CreateDefault()),
                new Mock<ICommandTimeoutService>().Object,
                Context,
                Fixture.Create<int>());

            // Assert
            Assert.IsNotNull(instance);

            // Act
            instance = new DbLoadingChunkerService<string>(
                new Mock<IConfigurationService>().Object,
                new TelemetryClient(TelemetryConfiguration.CreateDefault()),
                new Mock<ICommandTimeoutService>().Object,
                Context);

            // Assert
            Assert.IsNotNull(instance);
        }
    }
}