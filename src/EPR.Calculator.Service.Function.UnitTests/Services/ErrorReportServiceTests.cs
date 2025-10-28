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
                },
             };

            var orgDetails = new[]
            {
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 1,
                    OrganisationName = "Test",
                    SubmissionPeriodDesc = "July to December 2023"
                },
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

            Assert.IsNotNull(capturedReports, "Expected InsertRecords to be called with a non-null list of ErrorReports.");

            var reportsList = capturedReports!.ToList();
            Assert.AreEqual(2, reportsList.Count, "Expected 2 unmatched records to be inserted (OrganisationId 2 and 3).");

            var report2 = reportsList.FirstOrDefault(r => r.ProducerId == 2);
            var report3 = reportsList.FirstOrDefault(r => r.ProducerId == 3);

            Assert.IsNotNull(report2, "Missing report for OrganisationId 2");
            Assert.IsNotNull(report3, "Missing report for OrganisationId 3");

            Assert.AreEqual(runId, report2!.CalculatorRunId);
            Assert.AreEqual("22", report2.SubsidiaryId);
            Assert.AreEqual((int)ErrorTypes.UNKNOWN, report2.ErrorTypeId);
            Assert.AreEqual(createdBy, report2.CreatedBy);
            Assert.IsTrue(report2.CreatedAt >= beforeInvoke && report2.CreatedAt <= afterInvoke, "CreatedAt should be within test time window.");

            Assert.AreEqual(runId, report3!.CalculatorRunId);
            Assert.AreEqual("33", report3.SubsidiaryId);
            Assert.AreEqual((int)ErrorTypes.UNKNOWN, report3.ErrorTypeId);
            Assert.AreEqual(createdBy, report3.CreatedBy);
            Assert.IsTrue(report3.CreatedAt >= beforeInvoke && report3.CreatedAt <= afterInvoke, "CreatedAt should be within test time window.");
        }

        [TestMethod]
        public async Task HandleUnmatchedPomAsync_DoesNotInsert_WhenNoUnmatched()
        {
            // Arrange
            var runId = 456;
            var createdBy = "Systemtest";

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
            };

            // Org details contains both IDs, so none are unmatched
            var orgDetails = new[]
            {
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 1,
                    OrganisationName="Test1",
                    SubmissionPeriodDesc="July to December 2023"
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 2,
                    OrganisationName="Test2",
                    SubmissionPeriodDesc="July to December 2023"
                },
            };

            var mockErrorReportService = new Mock<IDbLoadingChunkerService<ErrorReport>>();
            // Act
            await _service.HandleUnmatchedPomAsync(pomDetails, orgDetails, runId, createdBy, CancellationToken.None);

            // Assert
            mockErrorReportService.Verify(c => c.InsertRecords(It.IsAny<IReadOnlyCollection<ErrorReport>>()), Times.Never);
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
