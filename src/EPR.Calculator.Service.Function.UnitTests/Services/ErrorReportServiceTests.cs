using System.Collections.ObjectModel;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Services;
using Microsoft.IdentityModel.Tokens;
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
        public void HandleMissingRegistrationData_InsertsCorrectErrorReports_WhenUnmatchedExists()
        {
            // Arrange
            var runId = 123;
            var createdBy = "test user";

            var pomDetails = new[]
            {
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 1,
                    SubsidiaryId = "11",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 2,
                    SubsidiaryId = "22",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 3,
                    SubsidiaryId = "33",
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
                    OrganisationName = "Test"
                }
            };

            // Act
            IEnumerable<ErrorReport> reportsList = _service.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);


            Assert.AreEqual(3, reportsList.Count(), "Expected 3 unmatched records to be returned.");

            foreach (var r in reportsList)
            {
                Assert.AreEqual(runId, r.CalculatorRunId);
                Assert.AreEqual(ErrorCodes.MissingRegistrationData, r.ErrorCode);
                Assert.AreEqual(createdBy, r.CreatedBy);
            }

            Assert.IsTrue(reportsList.Any(r => r.ProducerId == 1 && r.SubsidiaryId == "11"));
            Assert.IsTrue(reportsList.Any(r => r.ProducerId == 2 && r.SubsidiaryId == "22"));
            Assert.IsTrue(reportsList.Any(r => r.ProducerId == 3 && r.SubsidiaryId == "33"));
        }

        [TestMethod]
        public void HandleMissingRegistrationData_DeduplicatesMultiplePomsForSameOrgSub()
        {
            // Arrange
            var runId = 200;
            var createdBy = "dedup test";

            var pomDetails = new[]
            {
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 10,
                    SubsidiaryId = "101",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 10,
                    SubsidiaryId = "101",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 10,
                    SubsidiaryId = "102",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                }
            };

            var orgDetails = Array.Empty<CalculatorRunOrganisationDataDetail>();

            // Act
            IEnumerable<ErrorReport> reportsList = _service.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            Assert.AreEqual(2, reportsList.Count(), "Expected 1 error per unique Org+Sub combination.");
            Assert.IsTrue(reportsList.Any(r => r.ProducerId == 10 && r.SubsidiaryId == "101"));
            Assert.IsTrue(reportsList.Any(r => r.ProducerId == 10 && r.SubsidiaryId == "102"));
        }

        [TestMethod]
        public void HandleMissingRegistrationData_DoesNotInsert_WhenAllMatched()
        {
            // Arrange
            var runId = 300;
            var createdBy = "no error";

            var pomDetails = new[]
            {
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 1,
                    SubsidiaryId = "101",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 2,
                    SubsidiaryId = "102",
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
                    SubsidiaryId = "101",
                    OrganisationName = "Test"
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 2,
                    SubsidiaryId = "102",
                    OrganisationName = "Test1"
                }
            };

            // Act
            IEnumerable<ErrorReport> reportsList = _service.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            Assert.AreEqual(0, reportsList.Count(), "Expected no unmatched records to be returned.");
        }

        [TestMethod]
        public void HandleMissingRegistrationData_DeduplicatesMultiplePomsForSameOrgSub_Issue3()
        {
            // Arrange
            var runId = 400;
            var createdBy = "dedup issue3";

            // Simulate 41 POMs for the same Org/Sub which does NOT exist in org table
            var pomDetails = Enumerable.Range(1, 41)
                .Select(i => new CalculatorRunPomDataDetail
                {
                    OrganisationId = 100,
                    SubsidiaryId = "200",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                })
                .ToArray();

            // Org table is empty → all POMs are unmatched
            var orgDetails = Array.Empty<CalculatorRunOrganisationDataDetail>();

            // Act
            IEnumerable<ErrorReport> reportsList = _service.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            // Should insert ONLY 1 error for the unique Org/Sub combination
            Assert.AreEqual(1, reportsList.Count(), "Expected only 1 error for the unique Org/Sub combination.");

            var report = reportsList.First();
            Assert.AreEqual(100, report.ProducerId);
            Assert.AreEqual("200", report.SubsidiaryId);
            Assert.AreEqual(runId, report.CalculatorRunId);
            Assert.AreEqual(ErrorCodes.MissingRegistrationData, report.ErrorCode);
            Assert.AreEqual(createdBy, report.CreatedBy);
        }

        [TestMethod]
        public void HandleMissingRegistrationData_DoesNotInsert_WhenNoUnmatched()
        {
            // Arrange
            var runId = 500;
            var createdBy = "no unmatched test";

            var pomDetails = new[]
            {
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 1,
                    SubsidiaryId = "101",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 2,
                    SubsidiaryId = "202",
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
                    SubsidiaryId = "101",
                    OrganisationName = "Test"
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 2,
                    SubsidiaryId = "202",
                    OrganisationName = "Test1"
                }
            };

            // Act
            IEnumerable<ErrorReport> reportsList = _service.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            Assert.AreEqual(0, reportsList.Count(), "Expected no unmatched records to be returned.");
        }

        [TestMethod]
        public void HandleMissingRegistrationData_WhenMissing_ErrorAllInOrganisation()
        {
            // Arrange
            var runId = 500;
            var createdBy = "no unmatched test";
            var timestamp = DateTime.UtcNow;

            var pomDetails = new[]
            {
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 1,
                    SubsidiaryId = null,
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = timestamp,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 1,
                    SubsidiaryId = "101",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = timestamp,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 1,
                    SubsidiaryId = "202",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = timestamp,
                    SubmissionPeriodDesc = "July to December 2023"
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 2,
                    SubsidiaryId = "303",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = timestamp,
                    SubmissionPeriodDesc = "July to December 2023"
                }
            };

            var orgDetails = new[]
            {
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 1,
                    SubsidiaryId = "101",
                    OrganisationName = "Test"
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 2,
                    SubsidiaryId = "303",
                    OrganisationName = "Test1"
                }
            };

            // Act
            IEnumerable<ErrorReport> reportsList = _service.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            Assert.AreEqual(3, reportsList.Count(), "Expected 3 error messages as Org 1 SubsidiaryId 202 is missing Reg data - so errors applies to all in Org 1");
            CollectionAssert.AreEquivalent(new[]
                {
                    (ProducerId: 1, SubsidiaryId: null , ErrorCode: ErrorCodes.MissingRegistrationData, leaverCode: ""),
                    (ProducerId: 1, SubsidiaryId: "101", ErrorCode: ErrorCodes.MissingRegistrationData, leaverCode: ""),
                    (ProducerId: 1, SubsidiaryId: "202", ErrorCode: ErrorCodes.MissingRegistrationData, leaverCode: "")
                },
                reportsList
                    .Select(r => (r.ProducerId, r.SubsidiaryId, r.ErrorCode, r.LeaverCode))
                    .ToList()
            );

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HandleMissingRegistrationData_Throws_WhenPomDetailsNull()
        {
            // Arrange
            IEnumerable<CalculatorRunPomDataDetail>? pomDetails = null;
            var orgDetails = new[] { new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 1,
                    OrganisationName="Test1"
                },
            };

            // Act
            _service.HandleMissingRegistrationData(pomDetails!, orgDetails, 1, "u");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HandleMissingRegistrationData_Throws_WhenOrgDetailsNull()
        {
            // Arrange
            var pomDetails = new[] { new CalculatorRunPomDataDetail
                {
                    OrganisationId = 1,
                    SubsidiaryId = "11",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023"
                },
            };
            IEnumerable<CalculatorRunOrganisationDataDetail>? orgDetails = null;

            // Act
            _service.HandleMissingRegistrationData(pomDetails, orgDetails!, 1, "u");
        }

        [TestMethod]
        public void HandleMissingRegistrationData_DoesNotInsert_WhenSubmitterIdsMatched()
        {
            // Arrange
            var runId = 300;
            var createdBy = "no error";

            Guid pom1ID = Guid.NewGuid();
            Guid pom2ID = Guid.NewGuid();
            var pomDetails = new[] {
                CreatePomData(1, "2023-P2", pom1ID, "", "", 0, submissionPeriodDesc:"July to December 2023",subsidiaryId:"101"),
                CreatePomData(2, "2023-P2", pom2ID, "", "", 0, submissionPeriodDesc:"July to December 2023",subsidiaryId:"102")
             };

            var orgDetails = new[] {
                CreateOrganisationData(1,"101","Test",pom1ID, submissionPeriodDesc:"July to December 2023"),
                CreateOrganisationData(2,"102","Test1",pom2ID, submissionPeriodDesc:"July to December 2023")
            };

            // Act
            IEnumerable<ErrorReport> reportsList = _service.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            Assert.AreEqual(0, reportsList.Count(), "Expected no unmatched records to be returned.");
        }

        [TestMethod]
        public void HandleMissingRegistrationData_Inserts_WhenSubmitterIdsDoNotMatch()
        {
            // Arrange
            var runId = 300;
            var createdBy = "no error";

            Guid pom1SumbitterId = Guid.NewGuid();
            Guid pom2SubmitterId = Guid.NewGuid();

            var pomDetails = new[] {
                CreatePomData(1, "2023-P2", pom1SumbitterId, "", "", 0, submissionPeriodDesc:"July to December 2023",subsidiaryId:"101"),
                CreatePomData(2, "2023-P2", pom1SumbitterId, "", "", 0, submissionPeriodDesc:"July to December 2023",subsidiaryId:"102")
            };

            var orgDetails = new[] {
                CreateOrganisationData(1,"101","Test",pom1SumbitterId, submissionPeriodDesc:"July to December 2023"),
                CreateOrganisationData(2,"102","Test1",pom2SubmitterId, submissionPeriodDesc:"July to December 2023")
            };

            // Act
            IEnumerable<ErrorReport> capturedReports = _service.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            var reportsList = capturedReports!.ToList();
            Assert.AreEqual(1, reportsList.Count, "Expected 1 unmatched records to be returned.");
            var error = reportsList.First();
            Assert.AreEqual(ErrorCodes.MissingRegistrationData, error.ErrorCode, "Incorrect Error Type");
            Assert.AreEqual(2, error.ProducerId, "Incorrect Producer Id");
        }

        [TestMethod]
        public void HandleMissingPOMErrors_WherePreviousPOMsExists()
        {
            var submitterId1 = Guid.NewGuid();
            var submitterId2 = Guid.NewGuid();

            var orgDetails = new[] {
                CreateOrganisationData(100101,null,"ECOLTD",submitterId1, "N"),
                CreateOrganisationData(200202,null,"Green holdings",submitterId2, "O"),
                CreateOrganisationData(200202,"100500","Pure leaf drinks",submitterId2, "O"),
                CreateOrganisationData(200202,"100101","ECOLTD",submitterId2, "O", "01")
            };

            var pomDetails = new[] {
                CreatePomData(100101, "2024-P1",submitterId1,"HH","ST",5000),
                CreatePomData(100101, "2024-P1",submitterId1,"HH","PL",3000),
                CreatePomData(100101, "2024-P4",submitterId1,"HH","ST",5000),
                CreatePomData(100101, "2024-P4",submitterId1,"HH","PL",3000),
                CreatePomData(200202, "2024-P1",submitterId2,"HH","PL",2000),
                CreatePomData(200202, "2024-P1",submitterId2,"HH","AL",4500),
                CreatePomData(200202, "2024-P4",submitterId2,"HH","PL",2000),
                CreatePomData(200202, "2024-P1",submitterId2,"HH","PL",3500,subsidiaryId:"100500"),
                CreatePomData(200202, "2024-P1",submitterId2,"HH","PL",4000,subsidiaryId:"100500"),
                CreatePomData(200202, "2024-P4",submitterId2,"HH","PL",3000,subsidiaryId:"100500")
            };

            // Arrange
            var runId = 300;
            var createdBy = "no error";

            // Act
            IEnumerable<ErrorReport> reportsList = _service.HandleMissingPomData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            Assert.AreEqual(1, reportsList.Count(), "Expected 1 unmatched records to be returned.");
            var error = reportsList.First();
            Assert.AreEqual(ErrorCodes.MissingPOMData, error.ErrorCode, "Incorrect Error Code");
            Assert.AreEqual(200202, error.ProducerId, "Incorrect Producer Id");
            Assert.AreEqual("100101", error.SubsidiaryId, "Incorrect Subsidiary Id");
            Assert.AreEqual("01", error.LeaverCode, "Incorrect Leaver Code");
        }

        [TestMethod]
        public void HandleMissingPOMErrors_WhereNoPreviousPOMsExists()
        {
            var submitterId1 = Guid.NewGuid();
            var submitterId2 = Guid.NewGuid();

            var orgDetails = new[] {
                CreateOrganisationData(100101,null,"ECOLTD",submitterId1, "N"),
                CreateOrganisationData(200202,null,"Green holdings",submitterId2, "O"),
                CreateOrganisationData(200202,"100500","Pure leaf drinks",submitterId2, "O"),
                CreateOrganisationData(200202,"100101","ECOLTD",submitterId2, "O", "01")
            };

            var pomDetails = new[] {
                CreatePomData(200202, "2024-P1",submitterId2,"HH","PL",2000),
                CreatePomData(200202, "2024-P1",submitterId2,"HH","AL",4500),
                CreatePomData(200202, "2024-P4",submitterId2,"HH","PL",2000),
                CreatePomData(200202, "2024-P1",submitterId2,"HH","PL",3500,subsidiaryId:"100500"),
                CreatePomData(200202, "2024-P1",submitterId2,"HH","PL",4000,subsidiaryId:"100500"),
                CreatePomData(200202, "2024-P4",submitterId2,"HH","PL",3000,subsidiaryId:"100500")
            };

            // Arrange
            var runId = 300;
            var createdBy = "no error";

            // Act
            IEnumerable<ErrorReport> capturedReports = _service.HandleMissingPomData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            Assert.IsTrue(capturedReports.IsNullOrEmpty());
        }

        [TestMethod]
        public void HandleObligatedErrors_ErrorsExistInRegData()
        {
            var submitterId1 = Guid.NewGuid();
            var submitterId2 = Guid.NewGuid();
            var error1 = "Some error";
            var error2 = "Some other error";

            var orgDetails = new[] {
                CreateOrganisationData(100101,null,"ECOLTD",submitterId1, "E", errorCode: error1),
                CreateOrganisationData(200202,null,"Green holdings",submitterId2, "O"),
                CreateOrganisationData(200202,"100500","Pure leaf drinks",submitterId2, "E", errorCode: error2, statusCode: "some status code"),
                CreateOrganisationData(200202,"100101","ECOLTD",submitterId2, "E", errorCode: null)
            };

            // Arrange
            var runId = 300;
            var createdBy = "no error";

            // Act
            IEnumerable<ErrorReport> reportsList = _service.HandleObligatedErrors(orgDetails, runId, createdBy);

            // Assert
            Assert.AreEqual(3, reportsList.Count(), "Expected 3 unmatched records to be returned.");
            Assert.IsTrue(reportsList.Any(p => p.ProducerId == 100101 && p.SubsidiaryId == null && p.ErrorCode == error1 && p.LeaverCode == ""));
            Assert.IsTrue(reportsList.Any(p => p.ProducerId == 200202 && p.SubsidiaryId == "100500" && p.ErrorCode == error2 && p.LeaverCode == "some status code"));
            Assert.IsTrue(reportsList.Any(p => p.ProducerId == 200202 && p.SubsidiaryId == "100101" && p.ErrorCode == ErrorCodes.Empty && p.LeaverCode == ""));
        }

        public void HandleObligatedErrors_NoErrorsExistInRegData()
        {
            var submitterId1 = Guid.NewGuid();
            var submitterId2 = Guid.NewGuid();

            var orgDetails = new[]
            {
                CreateOrganisationData(100101,null,"ECOLTD",submitterId1, "N"),
                CreateOrganisationData(200202,null,"Green holdings",submitterId2, "O"),
                CreateOrganisationData(200202,"100500","Pure leaf drinks",submitterId2, "O"),
                CreateOrganisationData(200202,"100101","ECOLTD",submitterId2, "O", "01")
            };

            // Arrange
            var runId = 300;
            var createdBy = "no error";

            // Act
            IEnumerable<ErrorReport> reportsList = _service.HandleObligatedErrors(orgDetails, runId, createdBy);

            // Assert
            Assert.AreEqual(0, reportsList.Count(), "Expected 0 unmatched records to be returned.");
        }

        [TestMethod]
        public async Task HandleErrors_ForMissingRegAndMissingPoms()
        {
            var submitterId1 = Guid.NewGuid();
            var submitterId2 = Guid.NewGuid();

            var orgDetails = new[]
            {
                CreateOrganisationData(100101,null,"ECOLTD",submitterId1, "N"),
                CreateOrganisationData(200202,null,"Green holdings",submitterId2, "O"),
                CreateOrganisationData(200202,"100500","Pure leaf drinks",submitterId2, "O"),
                CreateOrganisationData(200202,"100101","ECOLTD",submitterId2, "O", "01")
            };

            var pomDetails = new[] {
                CreatePomData(100101, "2024-P1",submitterId1,"HH","ST",5000),
                CreatePomData(100101, "2024-P1",submitterId1,"HH","PL",3000),
                CreatePomData(100101, "2024-P4",submitterId1,"HH","ST",5000),
                CreatePomData(100101, "2024-P4",submitterId1,"HH","PL",3000),
                CreatePomData(200202, "2024-P1",submitterId2,"HH","PL",2000),
                CreatePomData(200202, "2024-P1",submitterId2,"HH","AL",4500),
                CreatePomData(200202, "2024-P4",submitterId2,"HH","PL",2000),
                CreatePomData(200202, "2024-P1",submitterId2,"HH","PL",3500,subsidiaryId:"100500"),
                CreatePomData(200202, "2024-P1",submitterId2,"HH","PL",4000,subsidiaryId:"100500"),
                CreatePomData(200202, "2024-P4",submitterId2,"HH","PL",3000,subsidiaryId:"100500"),
                CreatePomData(100200, "2024-P1",submitterId1,"HH","ST",5000)
            };

            // Arrange
            var runId = 300;
            var createdBy = "no error";

            IEnumerable<ErrorReport> errorReports = Enumerable.Empty<ErrorReport>();
            mockErrorReport.Setup(m => m.InsertRecords(It.IsAny<IEnumerable<ErrorReport>>())).Callback<IEnumerable<ErrorReport>>(arg => errorReports = arg).Returns(Task.CompletedTask);

            // Act
            var reportsList = await _service.HandleErrors(pomDetails, orgDetails, runId, createdBy, CancellationToken.None);

            Assert.AreEqual(3, errorReports.Count(), "Expected 3 unmatched records to be inserted.");
            Assert.IsTrue(errorReports.Any(p => p.ProducerId == 100200 && p.SubsidiaryId == null && p.ErrorCode == ErrorCodes.MissingRegistrationData));
            Assert.IsTrue(errorReports.Any(p => p.ProducerId == 200202 && p.SubsidiaryId == null && p.ErrorCode == ErrorCodes.Empty));
            Assert.IsTrue(errorReports.Any(p => p.ProducerId == 200202 && p.SubsidiaryId == "100101" && p.ErrorCode == ErrorCodes.MissingPOMData && p.LeaverCode == "01"));
        }

        private static CalculatorRunPomDataDetail CreatePomData(int orgId, string submissionPeriod, Guid submitterId, string packagingType, string packagingMaterial, int packagingMaterialWeight, string submissionPeriodDesc = "Jan to December 2025", string? subsidiaryId = null)
        {
            return new CalculatorRunPomDataDetail
            {
                OrganisationId = orgId,
                SubmissionPeriod = submissionPeriod,
                LoadTimeStamp = DateTime.UtcNow,
                SubmissionPeriodDesc = submissionPeriodDesc,
                SubmitterId = submitterId,
                PackagingType = packagingType,
                PackagingMaterial = packagingMaterial,
                PackagingMaterialWeight = packagingMaterialWeight,
                SubsidiaryId = subsidiaryId
            };
        }

        private CalculatorRunOrganisationDataDetail CreateOrganisationData(int orgId, string? subId, string orgName, Guid submitterId, string obligationStatus = "O", string statusCode = "", string submissionPeriodDesc = "Jan to December 2025", string? errorCode = null)
        {
            return new CalculatorRunOrganisationDataDetail
            {
                OrganisationId = orgId,
                SubsidiaryId = subId,
                OrganisationName = orgName,
                ObligationStatus = obligationStatus,
                StatusCode = statusCode,
                SubmitterId = submitterId,
                ErrorCode = errorCode
            };
        }
    }
}