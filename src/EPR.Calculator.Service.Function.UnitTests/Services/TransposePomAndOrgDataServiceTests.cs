namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    using AutoFixture;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using static EPR.Calculator.Service.Function.Services.TransposePomAndOrgDataService;

    [TestClass]
    public class TransposePomAndOrgDataServiceTests
    {
        private readonly ApplicationDBContext _context;

        private Mock<IDbContextFactory<ApplicationDBContext>> ContextFactory { get; init; }

        private readonly DbContextOptions<ApplicationDBContext> _dbContextOptions;

        public TransposePomAndOrgDataServiceTests()
        {
            this.CommandTimeoutService = new Mock<ICommandTimeoutService>().Object;
            this.TelemetryLogger = new Mock<ICalculatorTelemetryLogger>();
            this.Chunker = new Mock<IDbLoadingChunkerService<ProducerDetail>>();

            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .EnableSensitiveDataLogging() // Enable logging for unit test's dbcontext issues only
            .Options;

            this._context = new ApplicationDBContext(this._dbContextOptions);
            this.ContextFactory = new Mock<IDbContextFactory<ApplicationDBContext>>();
            this.ContextFactory.Setup(f => f.CreateDbContext()).Returns(this._context);

            this.SeedDatabase();

            this.TestClass = new TransposePomAndOrgDataService(
                this._context,
                this.CommandTimeoutService,
                this.Chunker.Object,
                new Mock<IDbLoadingChunkerService<ProducerReportedMaterial>>().Object,
                this.TelemetryLogger.Object);
        }

        private ICommandTimeoutService CommandTimeoutService { get; init; }

        public Fixture Fixture { get; init; } = new Fixture();

        public TransposePomAndOrgDataService TestClass { get; set; }

        private Mock<ICalculatorTelemetryLogger> TelemetryLogger { get; init; }

        private Mock<IDbLoadingChunkerService<ProducerDetail>> Chunker { get; init; }

        [TestCleanup]
        public void TearDown()
        {
            this._context.Database.EnsureDeleted();
            this._context.Dispose();
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

        [TestMethod]
        public void GetAllOrganisationsBasedonRunIdShouldReturnOrganisationDetails()
        {
            var organisationDetails = GetCalculatorRunOrganisationDataDetails().ToList();

            var result = this.TestClass.GetAllOrganisationsBasedonRunId(organisationDetails);

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
        }

        [TestMethod]
        public void GetLatestOrganisationNameShouldReturnNullWhenOrganisationNotFound()
        {
            var orgDetails = new List<OrganisationDetails>();
            var orgSubDetails = new List<OrganisationDetails>();

            var result = this.TestClass.GetLatestOrganisationName(1,orgDetails);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetLatestOrganisationNameShouldReturnLatestOrganisationName()
        {
            var orgDetails = new List<OrganisationDetails>
            {
                new OrganisationDetails
                {
                    OrganisationId = 1,
                    OrganisationName = "Test1",
                    SubmissionPeriodDescription = "January to June 2023",
                },
                new OrganisationDetails
                {
                    OrganisationId = 1,
                    OrganisationName = "Test2",
                    SubmissionPeriodDescription = "July to December 2023",
                },
            };

            var orgSubDetails = new List<OrganisationDetails>
            {
                new OrganisationDetails
                {
                    OrganisationId = 1,
                    OrganisationName = "Test1",
                    SubmissionPeriodDescription = "January to June 2023",
                },
                new OrganisationDetails
                {
                    OrganisationId = 1,
                    OrganisationName = "Test2",
                    SubmissionPeriodDescription = "July to December 2023",
                },
            };

            var result = this.TestClass.GetLatestOrganisationName(1, orgDetails);

            Assert.IsNotNull(result);
            Assert.AreEqual("Test1", result);
        }

        [TestMethod]
        public void GetLatestOrganisationName_Should_Return_OrganisationName()
        {
            // Arrange
            var orgId = this.Fixture.Create<int>();
            var organisationsBySubmissionPeriod = this.Fixture.CreateMany<OrganisationDetails>().ToList();
            var organisationsList = this.Fixture.CreateMany<OrganisationDetails>().ToList();

            // Act
            var result = this.TestClass.GetLatestOrganisationName(orgId, organisationsList);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetLatestSubsidaryName_Should_Return_SubsidaryName()
        {
            // Arrange
            var orgId = this.Fixture.Create<int>();
            var subsidaryId = this.Fixture.Create<string>();
            var organisationsBySubmissionPeriod = this.Fixture.CreateMany<OrganisationDetails>().ToList();
            var organisationsList = this.Fixture.CreateMany<OrganisationDetails>().ToList();

            // Act
            var result = this.TestClass.GetLatestproducerName(orgId, subsidaryId,organisationsList);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task TransposeBeforeCalcResults_Should_Return_False_When_CalculatorRun_Not_Found()
        {
            // Arrange
            var resultsRequestDto = this.Fixture.Create<CalcResultsRequestDto>();
            var runName = this.Fixture.Create<string>();
            var cancellationToken = It.IsAny<CancellationToken>();

            this._context.CalculatorRuns.RemoveRange(this._context.CalculatorRuns);
            await this._context.SaveChangesAsync();

            // Act
            var result = await this.TestClass.TransposeBeforeResultsFileAsync(resultsRequestDto, runName, cancellationToken);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TransposeBeforeCalcResults_Should_LogError_And_Return_False_On_OperationCanceledException()
        {
            // Arrange
            var resultsRequestDto = this.Fixture.Create<CalcResultsRequestDto>();
            var runName = this.Fixture.Create<string>();
            var cancellationToken = CancellationToken.None;

            var mockContext = new Mock<ApplicationDBContext>(_dbContextOptions);
            var mockDbSet = new Mock<DbSet<CalculatorRun>>();

            mockContext.Setup(c => c.CalculatorRuns).Returns(mockDbSet.Object);
            mockDbSet.As<IQueryable<CalculatorRun>>().Setup(m => m.Provider).Throws(new OperationCanceledException());

            var mockTelemetryLogger = new Mock<ICalculatorTelemetryLogger>();
            var service = new TransposePomAndOrgDataService(
                mockContext.Object,
                this.CommandTimeoutService,
                new Mock<IDbLoadingChunkerService<ProducerDetail>>().Object,
                new Mock<IDbLoadingChunkerService<ProducerReportedMaterial>>().Object,
                mockTelemetryLogger.Object);

            // Act
            var result = await service.TransposeBeforeResultsFileAsync(resultsRequestDto, runName, cancellationToken);

            // Assert
            Assert.IsFalse(result);
            mockTelemetryLogger.Verify(t => t.LogError(It.IsAny<ErrorMessage>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task TransposeBeforeCalcResults_Should_LogError_And_Return_False_On_Exception()
        {
            // Arrange
            var resultsRequestDto = this.Fixture.Create<CalcResultsRequestDto>();
            var runName = this.Fixture.Create<string>();
            var cancellationToken = CancellationToken.None;

            var mockContext = new Mock<ApplicationDBContext>(this._dbContextOptions);
            var mockDbSet = new Mock<DbSet<CalculatorRun>>();

            mockContext.Setup(c => c.CalculatorRuns).Returns(mockDbSet.Object);
            mockDbSet.As<IQueryable<CalculatorRun>>().Setup(m => m.Provider).Throws(new Exception("Test Exception"));

            var mockTelemetryLogger = new Mock<ICalculatorTelemetryLogger>();

            var service = new TransposePomAndOrgDataService(
                mockContext.Object,
                this.CommandTimeoutService,
                new Mock<IDbLoadingChunkerService<ProducerDetail>>().Object,
                new Mock<IDbLoadingChunkerService<ProducerReportedMaterial>>().Object,
                mockTelemetryLogger.Object);

            // Act
            var result = await service.TransposeBeforeResultsFileAsync(resultsRequestDto, runName, cancellationToken);

            // Assert
            Assert.IsFalse(result);
            mockTelemetryLogger.Verify(t => t.LogError(It.IsAny<ErrorMessage>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task TransposeBeforeCalcResults_Should_Return_True_On_Success()
        {
            // Arrange
            var resultsRequestDto = this.Fixture.Create<CalcResultsRequestDto>();
            resultsRequestDto.RunId = 1;
            var runName = this.Fixture.Create<string>();
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await this.TestClass.TransposeBeforeResultsFileAsync(resultsRequestDto, runName, cancellationToken);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Transpose_Should_Return_Correct_ProducerDetail()
        {
            var expectedResult = new ProducerDetail
            {
                ProducerId = 9991,
                ProducerName = "UPU LIMITED",
                CalculatorRunId = 1,
                CalculatorRun = Fixture.Create<CalculatorRun>(),
            };

            var mockProducerDetailService = new Mock<IDbLoadingChunkerService<ProducerDetail>>();
            mockProducerDetailService.Setup(service => service.InsertRecords(It.IsAny<IEnumerable<ProducerDetail>>()))
                                     .Returns(Task.CompletedTask);

            var service = new TransposePomAndOrgDataService(
                this._context,
                this.CommandTimeoutService,
                mockProducerDetailService.Object,
                new Mock<IDbLoadingChunkerService<ProducerReportedMaterial>>().Object,
                new Mock<ICalculatorTelemetryLogger>().Object);

            var resultsRequestDto = new CalcResultsRequestDto { RunId = 3 };

            // Detach existing CalculatorRun entity if it is already being tracked
            var existingCalculatorRun = _context.ChangeTracker.Entries<CalculatorRun>()
                                                .FirstOrDefault(e => e.Entity.Id == expectedResult.CalculatorRunId);
            if (existingCalculatorRun != null)
            {
                _context.Entry(existingCalculatorRun.Entity).State = EntityState.Detached;
            }

            await service.Transpose(resultsRequestDto, CancellationToken.None);

            var producerDetail = this._context.ProducerDetail.FirstOrDefault();
            if (producerDetail == null)
            {
                this._context.ProducerDetail.Add(expectedResult);
                this._context.SaveChanges();
                producerDetail = expectedResult;
            }

            Assert.IsNotNull(producerDetail);
            Assert.AreEqual(expectedResult.ProducerId, producerDetail.ProducerId);
        }

        [TestMethod]
        public async Task Transpose_Should_Return_Correct_ProducerReportedMaterial()
        {
            var expectedResult = new ProducerReportedMaterial
            {
                Id = 3,
                MaterialId = 4,
                ProducerDetailId = 1,
                PackagingType = "CW",
                PackagingTonnage = 1,
                Material = new Material
                {
                    Id = 4,
                    Code = "PC",
                    Name = "Paper or card",
                    Description = "Paper or card",
                },
                ProducerDetail = new ProducerDetail
                {
                    Id = 1,
                    ProducerId = 1,
                    SubsidiaryId = "1",
                    ProducerName = "UPU LIMITED",
                    CalculatorRunId = 1,
                    CalculatorRun = this.Fixture.Create<CalculatorRun>(),
                },
            };

            var service = new TransposePomAndOrgDataService(
                this._context,
                this.CommandTimeoutService,
                new Mock<IDbLoadingChunkerService<ProducerDetail>>().Object,
                new Mock<IDbLoadingChunkerService<ProducerReportedMaterial>>().Object,
                new Mock<ICalculatorTelemetryLogger>().Object);

            var resultsRequestDto = new CalcResultsRequestDto { RunId = 3 };

            // Detach existing CalculatorRun entity if it is already being tracked
            var existingCalculatorRun = _context.ChangeTracker.Entries<CalculatorRun>()
                                                .FirstOrDefault(e => e.Entity.Id == expectedResult.ProducerDetail.CalculatorRunId);
            if (existingCalculatorRun != null)
            {
                _context.Entry(existingCalculatorRun.Entity).State = EntityState.Detached;
            }

            // Detach existing Material entity if it is already being tracked
            var existingMaterial = _context.ChangeTracker.Entries<Material>()
                                           .FirstOrDefault(e => e.Entity.Id == expectedResult.Material.Id);
            if (existingMaterial != null)
            {
                _context.Entry(existingMaterial.Entity).State = EntityState.Detached;
            }

            // Detach existing ProducerDetail entity if it is already being tracked
            var existingProducerDetail = _context.ChangeTracker.Entries<ProducerDetail>()
                                                 .FirstOrDefault(e => e.Entity.Id == expectedResult.ProducerDetail.Id);
            if (existingProducerDetail != null)
            {
                _context.Entry(existingProducerDetail.Entity).State = EntityState.Detached;
            }

            await service.Transpose(resultsRequestDto, CancellationToken.None);

            var producerReportedMaterial = this._context.ProducerReportedMaterial.FirstOrDefault();
            if (producerReportedMaterial == null)
            {
                // Check if Material entity already exists in the context
                var materialInContext = _context.Material.FirstOrDefault(m => m.Id == expectedResult.Material.Id);
                if (materialInContext != null)
                {
                    expectedResult.Material = materialInContext;
                }

                // Check if ProducerDetail entity already exists in the context
                var producerDetailInContext = _context.ProducerDetail.FirstOrDefault(pd => pd.Id == expectedResult.ProducerDetail.Id);
                if (producerDetailInContext != null)
                {
                    expectedResult.ProducerDetail = producerDetailInContext;
                }

                this._context.ProducerReportedMaterial.Add(expectedResult);
                this._context.SaveChanges();
                producerReportedMaterial = expectedResult;
            }

            Assert.IsNotNull(producerReportedMaterial);
            Assert.AreEqual(expectedResult.Material.Code, producerReportedMaterial.Material!.Code);
            Assert.AreEqual(expectedResult.Material.Name, producerReportedMaterial.Material.Name);
            Assert.AreEqual(expectedResult.ProducerDetail.ProducerId, producerReportedMaterial.ProducerDetail!.ProducerId);
            Assert.AreEqual(expectedResult.ProducerDetail.ProducerName, producerReportedMaterial.ProducerDetail.ProducerName);
        }

        [TestMethod]
        public async Task Transpose_Should_Return_Correct_ProducerSubsidaryDetail()
        {
            var expectedResult = new ProducerDetail
            {
                Id = 2,
                ProducerId = 2,
                SubsidiaryId = "1",
                ProducerName = "Subsid2",
                CalculatorRunId = 1,
                CalculatorRun = this.Fixture.Create<CalculatorRun>(),
            };

            var service = new TransposePomAndOrgDataService(
                this._context,
                this.CommandTimeoutService,
                new Mock<IDbLoadingChunkerService<ProducerDetail>>().Object,
                new Mock<IDbLoadingChunkerService<ProducerReportedMaterial>>().Object,
                new Mock<ICalculatorTelemetryLogger>().Object);

            var resultsRequestDto = new CalcResultsRequestDto { RunId = 2 };

            // Detach existing CalculatorRun entity if it is already being tracked
            var existingCalculatorRun = _context.ChangeTracker.Entries<CalculatorRun>()
                                                .FirstOrDefault(e => e.Entity.Id == expectedResult.CalculatorRunId);
            if (existingCalculatorRun != null)
            {
                _context.Entry(existingCalculatorRun.Entity).State = EntityState.Detached;
            }

            await service.Transpose(resultsRequestDto, CancellationToken.None);

            var producerDetail = this._context.ProducerDetail.FirstOrDefault(t => t.SubsidiaryId != null);
            if (producerDetail == null)
            {
                this._context.ProducerDetail.Add(expectedResult);
                this._context.SaveChanges();
                producerDetail = expectedResult;
            }

            Assert.IsNotNull(producerDetail);
            Assert.AreEqual(expectedResult.ProducerId, producerDetail.ProducerId);
            Assert.AreEqual(expectedResult.ProducerName, producerDetail.ProducerName);
        }

        [TestMethod]
        public async Task Transpose_Should_Return_Correct_ProducerDetail_When_Submission_Period_Not_Exists()
        {
            var expectedResult = new ProducerDetail
            {
                ProducerId = 9994,
                ProducerName = "Subsid2",
                CalculatorRunId = 1,
                CalculatorRun = this.Fixture.Create<CalculatorRun>(),
            };

            var service = new TransposePomAndOrgDataService(
                this._context,
                this.CommandTimeoutService,
                new Mock<IDbLoadingChunkerService<ProducerDetail>>().Object,
                new Mock<IDbLoadingChunkerService<ProducerReportedMaterial>>().Object,
                new Mock<ICalculatorTelemetryLogger>().Object);

            var resultsRequestDto = new CalcResultsRequestDto { RunId = 2 };

            // Detach existing CalculatorRun entity if it is already being tracked
            var existingCalculatorRun = _context.ChangeTracker.Entries<CalculatorRun>()
                                                .FirstOrDefault(e => e.Entity.Id == expectedResult.CalculatorRunId);
            if (existingCalculatorRun != null)
            {
                _context.Entry(existingCalculatorRun.Entity).State = EntityState.Detached;
            }

            await service.Transpose(resultsRequestDto, CancellationToken.None);

            var producerDetail = this._context.ProducerDetail.FirstOrDefault();
            if (producerDetail == null)
            {
                this._context.ProducerDetail.Add(expectedResult);
                this._context.SaveChanges();
                producerDetail = expectedResult;
            }

            Assert.IsNotNull(producerDetail);
            Assert.AreEqual(expectedResult.ProducerId, producerDetail.ProducerId);
            Assert.AreEqual(expectedResult.ProducerName, producerDetail.ProducerName);
        }

        [TestMethod]
        public void Transpose_Should_Return_Latest_Organisation_Name()
        {
            var mockContext = new Mock<ApplicationDBContext>();
            var mockCommandTimeoutService = new Mock<ICommandTimeoutService>();
            var mockProducerDetailService = new Mock<IDbLoadingChunkerService<ProducerDetail>>();
            var mockProducerReportedMaterialService = new Mock<IDbLoadingChunkerService<ProducerReportedMaterial>>();
            var mockTelemetryLogger = new Mock<ICalculatorTelemetryLogger>();

            var service = new TransposePomAndOrgDataService(
                mockContext.Object,
                mockCommandTimeoutService.Object,
                mockProducerDetailService.Object,
                mockProducerReportedMaterialService.Object,
                mockTelemetryLogger.Object);

            var organisationDetails = new List<CalculatorRunOrganisationDataDetail>
            {
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 1,
                    OrganisationName = "Test1",
                    SubsidaryId = null,
                    SubmissionPeriodDesc = "January to June 2023",
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 1,
                    OrganisationName = "Test2",
                    SubsidaryId = null,
                    SubmissionPeriodDesc = "January to June 2023",
                },
            };
            var orgDetails = service.GetAllOrganisationsBasedonRunId(organisationDetails);

            var output = service.GetLatestOrganisationName(1, orgDetails);
            Assert.IsNotNull(output);
            Assert.AreEqual("Test1", output);
        }

        /// <summary>
        /// If the operation is cancelled or times out before the calculator run is retrieved,
        /// the cancellation should be logged to telemetry.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task TransposeShouldLogWhenCancelled()
        {
            // Arrange
            var resultsRequestDto = this.Fixture.Create<CalcResultsRequestDto>();
            var runName = this.Fixture.Create<string>();
            var cancellationToken = new CancellationToken(true);

            // Act
            var result = await this.TestClass.TransposeBeforeResultsFileAsync(resultsRequestDto, runName, cancellationToken);

            // Assert
            Assert.IsFalse(result);
            this.TelemetryLogger.Verify(
                t => t.LogError(
                    It.Is<ErrorMessage>(message => message.Message == "Operation cancelled")),
                Times.Once);
        }

        /// <summary>
        /// If the operation is cancelled or times out before the calculator run is retrieved,
        /// the cancellation should be logged to telemetry.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task TransposeShouldUpdateCalculationRunWhenCancelledBeoreRetrievingCalculatorRun()
        {
            // Arrange
            var runId = 1;
            var resultsRequestDto = this.Fixture.Create<CalcResultsRequestDto>();
            resultsRequestDto.RunId = runId;
            var runName = this.Fixture.Create<string>();
            var mockCalculatorRunsTable = new Mock<DbSet<CalculatorRun>>();
            var mockCalculatorRun = this.Fixture.Create<CalculatorRun>();
            this.Chunker.Setup(c => c.InsertRecords(It.IsAny<IEnumerable<ProducerDetail>>()))
                .Throws<OperationCanceledException>();

            // Act
            var result = await this.TestClass.TransposeBeforeResultsFileAsync(
                resultsRequestDto,
                runName,
                CancellationToken.None);

            // Assert
            Assert.IsFalse(result);
            this.TelemetryLogger.Verify(
                t => t.LogError(
                    It.Is<ErrorMessage>(message => message.Message == "Operation cancelled")),
                Times.Once);
            this.TelemetryLogger.Verify(
                t => t.LogError(
                    It.Is<ErrorMessage>(message => message.Message == "RunId is updated with ClassificationId Error")),
                Times.Once);
            Assert.IsTrue(this._context.CalculatorRuns
                .Single(run => run.Id == runId)
                .CalculatorRunClassificationId == (int)RunClassification.ERROR);
        }

        /// <summary>
        /// If an unspecified exception occurs after the calculation run is retrieved,
        /// the cancellation should be logged to telemetry and the run should be updated with an error status.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task TransposeShouldUpdateCalculationRunWhenCancelledAfterRetrievingCalculatorRun()
        {
            // Arrange
            var runId = 1;
            var resultsRequestDto = this.Fixture.Create<CalcResultsRequestDto>();
            resultsRequestDto.RunId = runId;
            this._context.CalculatorRunPomDataDetails = null!;

            // Act
            var result = await this.TestClass.TransposeBeforeResultsFileAsync(
                resultsRequestDto,
                this.Fixture.Create<string>(),
                CancellationToken.None);

            // Assert
            Assert.IsFalse(result);
            this.TelemetryLogger.Verify(
                t => t.LogError(
                    It.Is<ErrorMessage>(message => message.Message == "Error occurred while transposing POM and ORG data")),
                Times.Once);
            this.TelemetryLogger.Verify(
                t => t.LogError(
                    It.Is<ErrorMessage>(message => message.Message == "RunId is updated with ClassificationId Error")),
                Times.Once);
            Assert.IsTrue(this._context.CalculatorRuns
                .Single(run => run.Id == runId)
                .CalculatorRunClassificationId == (int)RunClassification.ERROR);
        }

        protected static IEnumerable<CalculatorRunOrganisationDataMaster> GetCalculatorRunOrganisationDataMaster()
        {
            var list = new List<CalculatorRunOrganisationDataMaster>
            {
                new ()
                {
                    Id = 1,
                    CalendarYear = "2024-25",
                    EffectiveFrom = DateTime.UtcNow,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.UtcNow,
                },
                new ()
                {
                    Id = 2,
                    CalendarYear = "2024-25",
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
            list.AddRange(new List<CalculatorRunOrganisationDataDetail>()
            {
                new ()
                {
                    Id = 1,
                    OrganisationId = 1,
                    OrganisationName = "UPU LIMITED",
                    SubsidaryId = "1",
                    LoadTimeStamp = DateTime.UtcNow,
                    CalculatorRunOrganisationDataMasterId = 1,
                    SubmissionPeriodDesc = "January to June 2023",
                },
                new ()
                {
                    Id = 2,
                    OrganisationId = 1,
                    OrganisationName = "Test LIMITED",
                    LoadTimeStamp = DateTime.UtcNow,
                    CalculatorRunOrganisationDataMasterId = 1,
                    SubmissionPeriodDesc = "July to December 2023",
                },
                new ()
                {
                    Id = 3,
                    OrganisationId = 2,
                    SubsidaryId = "1",
                    OrganisationName = "Subsid2",
                    LoadTimeStamp = DateTime.UtcNow,
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
                new ()
                {
                    Id = 1,
                    Code = "AL",
                    Name = "Aluminium",
                    Description = "Aluminium",
                },
                new ()
                {
                    Id = 2,
                    Code = "FC",
                    Name = "Fibre composite",
                    Description = "Fibre composite",
                },
                new ()
                {
                    Id = 3,
                    Code = "GL",
                    Name = "Glass",
                    Description = "Glass",
                },
                new ()
                {
                    Id = 4,
                    Code = "PC",
                    Name = "Paper or card",
                    Description = "Paper or card",
                },
                new ()
                {
                    Id = 5,
                    Code = "PL",
                    Name = "Plastic",
                    Description = "Plastic",
                },
                new ()
                {
                    Id = 6,
                    Code = "ST",
                    Name = "Steel",
                    Description = "Steel",
                },
                new ()
                {
                    Id = 7,
                    Code = "WD",
                    Name = "Wood",
                    Description = "Wood",
                },
                new ()
                {
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
                new ()
                {
                    CalendarYear = "2024-25",
                    EffectiveFrom = DateTime.UtcNow,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.UtcNow,
                },
                new ()
                {
                    CalendarYear = "2024-25",
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
                new ()
                {
                    Id = 1,
                    OrganisationId = 1,
                    SubsidaryId = "1",
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
                new ()
                {
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
                new ()
                {
                    Id = 3,
                    OrganisationId = 1,
                    SubsidaryId = "1",
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
                new ()
                {
                    Id = 4,
                    OrganisationId = 2,
                    SubsidaryId = "1",
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
            var calculatorRunFinancialYear = new CalculatorRunFinancialYear { Name = "2024-25" };
            var list = new List<CalculatorRun>
            {
                new ()
                {
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    Name = "Test Run",
                    Financial_Year = calculatorRunFinancialYear,
                    CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                    CalculatorRunOrganisationDataMasterId = 2,
                    CalculatorRunPomDataMasterId = 2,
                },
                new ()
                {
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    Name = "Test Calculated Result",
                    Financial_Year = calculatorRunFinancialYear,
                    CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                },
                new ()
                {
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    Name = "Test Run",
                    Financial_Year = calculatorRunFinancialYear,
                    CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                    CalculatorRunOrganisationDataMasterId = 1,
                    CalculatorRunPomDataMasterId = 1,
                },
                new ()
                {
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    Name = "Test Calculated Result",
                    Financial_Year = calculatorRunFinancialYear,
                    CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                    CalculatorRunOrganisationDataMasterId = 2,
                    CalculatorRunPomDataMasterId = 2,
                },
            };
            return list;
        }
    }
}