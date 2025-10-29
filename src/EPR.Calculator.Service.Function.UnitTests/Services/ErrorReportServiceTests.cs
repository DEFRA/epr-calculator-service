using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Services;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class ErrorReportServiceTests
    {
        private Mock<IDbLoadingChunkerService<ErrorReport>> mockErrorReport = null!;
        private ErrorReportService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            // Use Strict so unexpected calls cause immediate failures
            mockErrorReport = new Mock<IDbLoadingChunkerService<ErrorReport>>(MockBehavior.Strict);
            _service = new ErrorReportService(mockErrorReport.Object);
        }

        [TestMethod]
        public async Task HandleUnmatchedPomAsync_InsertsCorrectErrorReports_WhenUnmatchedExists()
        {
            // Arrange
            var runId = 123;
            var createdBy = "test user";

            var pomDetails = new[]
            {
                new CalculatorRunPomDataDetail 
                { 
                    OrganisationId = 1,
                    SubsidaryId = "11",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail 
                { 
                    OrganisationId = 2,
                    SubsidaryId = "22",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail 
                { 
                    OrganisationId = 3,
                    SubsidaryId = "33",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                }
            };

            var orgDetails = new[]
            {
                new CalculatorRunOrganisationDataDetail 
                { 
                    OrganisationId = 1,
                    OrganisationName = "Test",
                    SubmissionPeriodDesc = "July to December 2023" 
                }
            };

            IEnumerable<ErrorReport>? capturedReports = null;

            mockErrorReport
                .Setup(c => c.InsertRecords(It.IsAny<IEnumerable<ErrorReport>>()))
                .Returns(Task.CompletedTask)
                .Callback<IEnumerable<ErrorReport>>(reports => capturedReports = reports);

            var beforeInvoke = DateTime.UtcNow.AddSeconds(-1);

            // Act
            await _service.HandleUnmatchedPomAsync(pomDetails, orgDetails, runId, createdBy, CancellationToken.None);

            var afterInvoke = DateTime.UtcNow.AddSeconds(1);

            // Assert
            mockErrorReport.Verify(x => x.InsertRecords(It.IsAny<IEnumerable<ErrorReport>>()), Times.Once);
            Assert.IsNotNull(capturedReports);

            var reportsList = capturedReports!.ToList();
            Assert.AreEqual(2, reportsList.Count, "Expected 2 unmatched records to be inserted (OrganisationId 2 and 3).");

            foreach (var r in reportsList)
            {
                Assert.AreEqual(runId, r.CalculatorRunId);
                Assert.AreEqual((int)ErrorTypes.MISSINGREGISTRATIONDATA, r.ErrorTypeId);
                Assert.AreEqual(createdBy, r.CreatedBy);
                Assert.IsTrue(r.CreatedAt >= beforeInvoke && r.CreatedAt <= afterInvoke);
            }

            Assert.IsTrue(reportsList.Any(r => r.ProducerId == 2 && r.SubsidiaryId == "22"));
            Assert.IsTrue(reportsList.Any(r => r.ProducerId == 3 && r.SubsidiaryId == "33"));
        }

        [TestMethod]
        public async Task HandleUnmatchedPomAsync_DeduplicatesMultiplePomsForSameOrgSub()
        {
            // Arrange
            var runId = 200;
            var createdBy = "dedup test";

            var pomDetails = new[]
            {
                new CalculatorRunPomDataDetail 
                { 
                    OrganisationId = 10,
                    SubsidaryId = "101",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail 
                { 
                    OrganisationId = 10,
                    SubsidaryId = "101",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail 
                { 
                    OrganisationId = 10,
                    SubsidaryId = "102",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                }
            };

            var orgDetails = Array.Empty<CalculatorRunOrganisationDataDetail>();

            IEnumerable<ErrorReport>? capturedReports = null;

            mockErrorReport
                .Setup(c => c.InsertRecords(It.IsAny<IEnumerable<ErrorReport>>()))
                .Returns(Task.CompletedTask)
                .Callback<IEnumerable<ErrorReport>>(reports => capturedReports = reports);

            // Act
            await _service.HandleUnmatchedPomAsync(pomDetails, orgDetails, runId, createdBy, CancellationToken.None);

            // Assert
            var reportsList = capturedReports!.ToList();
            Assert.AreEqual(2, reportsList.Count, "Expected 1 error per unique Org+Sub combination.");

            Assert.IsTrue(reportsList.Any(r => r.ProducerId == 10 && r.SubsidiaryId == "101"));
            Assert.IsTrue(reportsList.Any(r => r.ProducerId == 10 && r.SubsidiaryId == "102"));
        }

        [TestMethod]
        public async Task HandleUnmatchedPomAsync_DoesNotInsert_WhenAllMatched()
        {
            // Arrange
            var runId = 300;
            var createdBy = "no error";

            var pomDetails = new[]
            {
                new CalculatorRunPomDataDetail 
                { 
                    OrganisationId = 1, 
                    SubsidaryId = "101",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail 
                { 
                    OrganisationId = 2,
                    SubsidaryId = "102",
                     SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                }
            };

            var orgDetails = new[]
            {
                new CalculatorRunOrganisationDataDetail 
                { 
                    OrganisationId = 1,
                    SubsidaryId = "101",
                    OrganisationName = "Test",
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunOrganisationDataDetail 
                { 
                    OrganisationId = 2,
                    SubsidaryId = "102",
                    OrganisationName = "Test1",
                    SubmissionPeriodDesc = "July to December 2023"
                }
            };

            // Act
            await _service.HandleUnmatchedPomAsync(pomDetails, orgDetails, runId, createdBy, CancellationToken.None);

            // Assert
            mockErrorReport.Verify(x => x.InsertRecords(It.IsAny<IEnumerable<ErrorReport>>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleUnmatchedPomAsync_DeduplicatesMultiplePomsForSameOrgSub_Issue3()
        {
            // Arrange
            var runId = 400;
            var createdBy = "dedup issue3";

            // Simulate 41 POMs for the same Org/Sub which does NOT exist in org table
            var pomDetails = Enumerable.Range(1, 41)
                .Select(i => new CalculatorRunPomDataDetail
                {
                    OrganisationId = 100,
                    SubsidaryId = "200",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                })
                .ToArray();

            // Org table is empty → all POMs are unmatched
            var orgDetails = Array.Empty<CalculatorRunOrganisationDataDetail>();

            IEnumerable<ErrorReport>? capturedReports = null;

            mockErrorReport
                .Setup(c => c.InsertRecords(It.IsAny<IEnumerable<ErrorReport>>()))
                .Returns(Task.CompletedTask)
                .Callback<IEnumerable<ErrorReport>>(reports => capturedReports = reports);

            var beforeInvoke = DateTime.UtcNow.AddSeconds(-1);

            // Act
            await _service.HandleUnmatchedPomAsync(pomDetails, orgDetails, runId, createdBy, CancellationToken.None);

            var afterInvoke = DateTime.UtcNow.AddSeconds(1);

            // Assert
            mockErrorReport.Verify(x => x.InsertRecords(It.IsAny<IEnumerable<ErrorReport>>()), Times.Once);
            Assert.IsNotNull(capturedReports);

            var reportsList = capturedReports!.ToList();

            // Should insert ONLY 1 error for the unique Org/Sub combination
            Assert.AreEqual(1, reportsList.Count, "Expected only 1 error for the unique Org/Sub combination.");

            var report = reportsList.First();
            Assert.AreEqual(100, report.ProducerId);
            Assert.AreEqual("200", report.SubsidiaryId);
            Assert.AreEqual(runId, report.CalculatorRunId);
            Assert.AreEqual((int)ErrorTypes.MISSINGREGISTRATIONDATA, report.ErrorTypeId);
            Assert.AreEqual(createdBy, report.CreatedBy);
            Assert.IsTrue(report.CreatedAt >= beforeInvoke && report.CreatedAt <= afterInvoke, "CreatedAt should be within test time window.");
        }

        [TestMethod]
        public async Task HandleUnmatchedPomAsync_DoesNotInsert_WhenNoUnmatched()
        {
            // Arrange
            var runId = 500;
            var createdBy = "no unmatched test";

            var pomDetails = new[]
            {
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 1,
                    SubsidaryId = "101",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 2,
                    SubsidaryId = "202",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                }
            };

            var orgDetails = new[]
            {
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 1,
                    SubsidaryId = "101",
                    OrganisationName = "Test",
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 2,
                    SubsidaryId = "202",
                    OrganisationName = "Test1",
                    SubmissionPeriodDesc = "July to December 2023"
                }
            };

            // Act
            await _service.HandleUnmatchedPomAsync(pomDetails, orgDetails, runId, createdBy, CancellationToken.None);

            // Assert
            // Verify that InsertRecords was never called
            mockErrorReport.Verify(x => x.InsertRecords(It.IsAny<IEnumerable<ErrorReport>>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task HandleUnmatchedPomAsync_Throws_WhenPomDetailsNull()
        {
            // Arrange
            IEnumerable<CalculatorRunPomDataDetail>? pomDetails = null;
            var orgDetails = new[] { new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 1,
                    OrganisationName="Test1",
                    SubmissionPeriodDesc="July to December 2023"
                },
            };

            // Act
            await _service.HandleUnmatchedPomAsync(pomDetails!, orgDetails, 1, "u", CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task HandleUnmatchedPomAsync_Throws_WhenOrgDetailsNull()
        {
            // Arrange
            var pomDetails = new[] { new CalculatorRunPomDataDetail
                {
                    OrganisationId = 1,
                    SubsidaryId = "11",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
            };
            IEnumerable<CalculatorRunOrganisationDataDetail>? orgDetails = null;

            // Act
            await _service.HandleUnmatchedPomAsync(pomDetails, orgDetails!, 1, "u", CancellationToken.None);
        }
    }
}
