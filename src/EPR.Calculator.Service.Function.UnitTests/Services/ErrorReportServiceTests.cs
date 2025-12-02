using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Services;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Collections.Generic;

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
        public void HandleUnmatchedPomAsync_InsertsCorrectErrorReports_WhenUnmatchedExists()
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

            // Act
            IEnumerable<ErrorReport> reportsList = _service.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);


            Assert.AreEqual(4, reportsList.Count(), "Expected 4 unmatched records to be inserted (OrganisationId 1, 2 and 3).");

            foreach (var r in reportsList)
            {
                Assert.AreEqual(runId, r.CalculatorRunId);
                Assert.AreEqual((int)ErrorTypes.MissingRegistrationData, r.ErrorTypeId);
                Assert.AreEqual(createdBy, r.CreatedBy);
            }

            Assert.IsTrue(reportsList.Any(r => r.ProducerId == 1 && r.SubsidiaryId == "11"));
            Assert.IsTrue(reportsList.Any(r => r.ProducerId == 2 && r.SubsidiaryId == "22"));
            Assert.IsTrue(reportsList.Any(r => r.ProducerId == 3 && r.SubsidiaryId == "33"));
        }

        [TestMethod]
        public void HandleUnmatchedPomAsync_DeduplicatesMultiplePomsForSameOrgSub()
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

            // Act
            IEnumerable <ErrorReport> reportsList = _service.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            Assert.AreEqual(2, reportsList.Count(), "Expected 1 error per unique Org+Sub combination.");
            Assert.IsTrue(reportsList.Any(r => r.ProducerId == 10 && r.SubsidiaryId == "101"));
            Assert.IsTrue(reportsList.Any(r => r.ProducerId == 10 && r.SubsidiaryId == "102"));
        }

        [TestMethod]
        public void HandleUnmatchedPomAsync_DoesNotInsert_WhenAllMatched()
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
            IEnumerable<ErrorReport> reportsList = _service.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            Assert.AreEqual(0, reportsList.Count(), "Expected no unmatched records to be inserted.");
        }

        [TestMethod]
        public void HandleUnmatchedPomAsync_DeduplicatesMultiplePomsForSameOrgSub_Issue3()
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

            // Act
            IEnumerable<ErrorReport> reportsList = _service.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            // Should insert ONLY 1 error for the unique Org/Sub combination
            Assert.AreEqual(1, reportsList.Count(), "Expected only 1 error for the unique Org/Sub combination.");

            var report = reportsList.First();
            Assert.AreEqual(100, report.ProducerId);
            Assert.AreEqual("200", report.SubsidiaryId);
            Assert.AreEqual(runId, report.CalculatorRunId);
            Assert.AreEqual((int)ErrorTypes.MissingRegistrationData, report.ErrorTypeId);
            Assert.AreEqual(createdBy, report.CreatedBy);
        }

        [TestMethod]
        public void HandleUnmatchedPomAsync_DoesNotInsert_WhenNoUnmatched()
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
            IEnumerable<ErrorReport> reportsList = _service.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            Assert.AreEqual(0, reportsList.Count(), "Expected no unmatched records to be inserted.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HandleUnmatchedPomAsync_Throws_WhenPomDetailsNull()
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
            _service.HandleMissingRegistrationData(pomDetails!, orgDetails, 1, "u");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HandleUnmatchedPomAsync_Throws_WhenOrgDetailsNull()
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
            _service.HandleMissingRegistrationData(pomDetails, orgDetails!, 1, "u");
        }

        [TestMethod]
        public void HandleUnmatchedPomAsync_DoesNotInsert_WhenSubmitterIdsMatched()
        {
            // Arrange
            var runId = 300;
            var createdBy = "no error";

            Guid pom1ID = Guid.NewGuid();
            Guid pom2ID = Guid.NewGuid();
            var pomDetails = new[]
            {
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 1,
                    SubsidaryId = "101",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023",
                    SubmitterId = pom1ID,
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 2,
                    SubsidaryId = "102",
                     SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023",
                    SubmitterId = pom2ID

                }
            };

            var orgDetails = new[]
            {
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 1,
                    SubsidaryId = "101",
                    OrganisationName = "Test",
                    SubmissionPeriodDesc = "July to December 2023",
                    SubmitterId=pom1ID,
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 2,
                    SubsidaryId = "102",
                    OrganisationName = "Test1",
                    SubmissionPeriodDesc = "July to December 2023",
                    SubmitterId=pom2ID,
                }
            };

            // Act
            IEnumerable<ErrorReport> reportsList = _service.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            Assert.AreEqual(0, reportsList.Count(), "Expected no unmatched records to be inserted.");
        }

        [TestMethod]
        public void HandleUnmatchedPomAsync_Inserts_WhenSubmitterIdsDoNotMatch()
        {
            // Arrange
            var runId = 300;
            var createdBy = "no error";

            Guid pom1SumbitterId = Guid.NewGuid();
            Guid pom2SubmitterId = Guid.NewGuid();
            var pomDetails = new[]
            {
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 1,
                    SubsidaryId = "101",
                    SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023",
                    SubmitterId = pom1SumbitterId,
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 2,
                    SubsidaryId = "102",
                     SubmissionPeriod = "2023-P2",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "July to December 2023",
                    SubmitterId = pom1SumbitterId

                }
            };

            var orgDetails = new[]
            {
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 1,
                    SubsidaryId = "101",
                    OrganisationName = "Test",
                    SubmissionPeriodDesc = "July to December 2023",
                    SubmitterId=pom1SumbitterId,
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 2,
                    SubsidaryId = "102",
                    OrganisationName = "Test1",
                    SubmissionPeriodDesc = "July to December 2023",
                    SubmitterId=pom2SubmitterId,
                }
            };

            // Act
            IEnumerable<ErrorReport> capturedReports =  _service.HandleMissingRegistrationData(pomDetails, orgDetails, runId, createdBy);

            // Assert
            var reportsList = capturedReports!.ToList();
            Assert.AreEqual(2, reportsList.Count, "Expected 2 unmatched records to be inserted (2).");
            var error = reportsList.First();
            Assert.AreEqual((int)ErrorTypes.MissingRegistrationData, error.ErrorTypeId, "Incorrect Error Type" );
            Assert.AreEqual(2, error.ProducerId, "Incorrect Producer Id");
        }

        [TestMethod]
        public void HandleMissingPOMErrors_WherePreviousPOMsExists()
        {
            var submitterId1 = Guid.NewGuid();
            var submitterId2 = Guid.NewGuid();

            var orgDetails = new[]
                {
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 100101,
                    OrganisationName = "ECOLTD",
                    SubmissionPeriodDesc = "Jan to December 2025",
                    ObligationStatus="N",
                    SubmitterId =submitterId1
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 200202 ,
                    OrganisationName = "Green holdings",
                    SubmissionPeriodDesc = "Jan to December 2025",
                    ObligationStatus="Y",
                    SubmitterId =submitterId2
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 200202  ,
                    SubsidaryId = "100500",
                    OrganisationName = "Pure leaf drinks",
                    SubmissionPeriodDesc = "Jan to December 2025",
                    ObligationStatus="Y",
                    SubmitterId = submitterId2
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 200202,
                    SubsidaryId = "100101",
                    OrganisationName = "ECOLTD",
                    SubmissionPeriodDesc = "Jan to December 2025",
                    ObligationStatus="Y",
                    StatusCode="01",
                    SubmitterId = submitterId2
                }
            };

            var pomDetails = new[] {
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 100101,
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId1,
                    PackagingType="HH",
                    PackagingMaterial="ST",
                    PackagingMaterialWeight=5000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 100101,
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId1,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=3000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 100101,
                    SubmissionPeriod = "2024-P4",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId1,
                    PackagingType="HH",
                    PackagingMaterial="ST",
                    PackagingMaterialWeight=5000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 100101,
                    SubmissionPeriod = "2024-P4",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId1,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=3000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202 ,
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=2000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202 ,
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="AL",
                    PackagingMaterialWeight=4500
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202,
                    SubmissionPeriod = "2024-P4",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=2000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202,
                    SubsidaryId="100500",
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=3500
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202,
                    SubsidaryId="100500",
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=4000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202,
                    SubsidaryId="100500",
                    SubmissionPeriod = "2024-P4",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=3000
                }
            };

            // Arrange
            var runId = 300;
            var createdBy = "no error";

            // Act
            IEnumerable<ErrorReport> reportsList = _service.HandleMissingPomData(pomDetails, orgDetails, runId, createdBy);
           
            // Assert
            Assert.AreEqual(1, reportsList.Count(), "Expected 1 unmatched records to be inserted.");
            var error = reportsList.First();
            Assert.AreEqual((int)ErrorTypes.MissingPOMData, error.ErrorTypeId, "Incorrect Error Type");
            Assert.AreEqual(200202, error.ProducerId, "Incorrect Producer Id");
            Assert.AreEqual("100101", error.SubsidiaryId, "Incorrect Subsidiary Id");
            Assert.AreEqual("01", error.LeaverCode, "Incorrect Leaver Code");
        }

        [TestMethod]
        public void HandleMissingPOMErrors_WhereNoPreviousPOMsExists()
        {
            var submitterId1 = Guid.NewGuid();
            var submitterId2 = Guid.NewGuid();

            var orgDetails = new[]
                {
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 100101,
                    OrganisationName = "ECOLTD",
                    SubmissionPeriodDesc = "Jan to December 2025",
                    ObligationStatus="N",
                    SubmitterId =submitterId1
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 200202 ,
                    OrganisationName = "Green holdings",
                    SubmissionPeriodDesc = "Jan to December 2025",
                    ObligationStatus="Y",
                    SubmitterId =submitterId2
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 200202  ,
                    SubsidaryId = "100500",
                    OrganisationName = "Pure leaf drinks",
                    SubmissionPeriodDesc = "Jan to December 2025",
                    ObligationStatus="Y",
                    SubmitterId = submitterId2
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 200202,
                    SubsidaryId = "100101",
                    OrganisationName = "ECOLTD",
                    SubmissionPeriodDesc = "Jan to December 2025",
                    ObligationStatus="Y",
                    StatusCode="01",
                    SubmitterId = submitterId2
                }
            };

            var pomDetails = new[] {
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202 ,
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=2000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202 ,
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="AL",
                    PackagingMaterialWeight=4500
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202,
                    SubmissionPeriod = "2024-P4",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=2000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202,
                    SubsidaryId="100500",
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=3500
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202,
                    SubsidaryId="100500",
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=4000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202,
                    SubsidaryId="100500",
                    SubmissionPeriod = "2024-P4",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=3000
                }
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
        public void HandleMissingPOMErrors_WhereNoStatusCodesExists()
        {
            var submitterId1 = Guid.NewGuid();
            var submitterId2 = Guid.NewGuid();

            var orgDetails = new[]
                {
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 100101,
                    OrganisationName = "ECOLTD",
                    SubmissionPeriodDesc = "Jan to December 2025",
                    ObligationStatus="N",
                    SubmitterId =submitterId1
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 200202 ,
                    OrganisationName = "Green holdings",
                    SubmissionPeriodDesc = "Jan to December 2025",
                    ObligationStatus="Y",
                    SubmitterId =submitterId2
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 200202  ,
                    SubsidaryId = "100500",
                    OrganisationName = "Pure leaf drinks",
                    SubmissionPeriodDesc = "Jan to December 2025",
                    ObligationStatus="Y",
                    SubmitterId = submitterId2
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 200202,
                    SubsidaryId = "100101",
                    OrganisationName = "ECOLTD",
                    SubmissionPeriodDesc = "Jan to December 2025",
                    ObligationStatus="Y",
                    SubmitterId = submitterId2
                }
            };

            var pomDetails = new[] {
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 100101,
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId1,
                    PackagingType="HH",
                    PackagingMaterial="ST",
                    PackagingMaterialWeight=5000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 100101,
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId1,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=3000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 100101,
                    SubmissionPeriod = "2024-P4",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId1,
                    PackagingType="HH",
                    PackagingMaterial="ST",
                    PackagingMaterialWeight=5000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 100101,
                    SubmissionPeriod = "2024-P4",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId1,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=3000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202 ,
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=2000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202 ,
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="AL",
                    PackagingMaterialWeight=4500
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202,
                    SubmissionPeriod = "2024-P4",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=2000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202,
                    SubsidaryId="100500",
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=3500
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202,
                    SubsidaryId="100500",
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=4000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202,
                    SubsidaryId="100500",
                    SubmissionPeriod = "2024-P4",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=3000
                }
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
        public async Task HandleErrors_ForMissingRegAndMissingPoms()
        {
            var submitterId1 = Guid.NewGuid();
            var submitterId2 = Guid.NewGuid();

            var orgDetails = new[]
                {
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 100101,
                    OrganisationName = "ECOLTD",
                    SubmissionPeriodDesc = "Jan to December 2025",
                    ObligationStatus="N",
                    SubmitterId =submitterId1
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 200202 ,
                    OrganisationName = "Green holdings",
                    SubmissionPeriodDesc = "Jan to December 2025",
                    ObligationStatus="Y",
                    SubmitterId =submitterId2
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 200202  ,
                    SubsidaryId = "100500",
                    OrganisationName = "Pure leaf drinks",
                    SubmissionPeriodDesc = "Jan to December 2025",
                    ObligationStatus="Y",
                    SubmitterId = submitterId2
                },
                new CalculatorRunOrganisationDataDetail
                {
                    OrganisationId = 200202,
                    SubsidaryId = "100101",
                    OrganisationName = "ECOLTD",
                    SubmissionPeriodDesc = "Jan to December 2025",
                    ObligationStatus="Y",
                    StatusCode="01",
                    SubmitterId = submitterId2
                }
            };

            var pomDetails = new[] {
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 100101,
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId1,
                    PackagingType="HH",
                    PackagingMaterial="ST",
                    PackagingMaterialWeight=5000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 100101,
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId1,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=3000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 100101,
                    SubmissionPeriod = "2024-P4",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId1,
                    PackagingType="HH",
                    PackagingMaterial="ST",
                    PackagingMaterialWeight=5000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 100101,
                    SubmissionPeriod = "2024-P4",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId1,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=3000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202 ,
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=2000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202 ,
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="AL",
                    PackagingMaterialWeight=4500
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202,
                    SubmissionPeriod = "2024-P4",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=2000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202,
                    SubsidaryId="100500",
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=3500
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202,
                    SubsidaryId="100500",
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=4000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 200202,
                    SubsidaryId="100500",
                    SubmissionPeriod = "2024-P4",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId2,
                    PackagingType="HH",
                    PackagingMaterial="PL",
                    PackagingMaterialWeight=3000
                },
                new CalculatorRunPomDataDetail
                {
                    OrganisationId = 100200,
                    SubmissionPeriod = "2024-P1",
                    LoadTimeStamp = DateTime.UtcNow,
                    SubmissionPeriodDesc = "Jan to December 2025",
                    SubmitterId=submitterId1,
                    PackagingType="HH",
                    PackagingMaterial="ST",
                    PackagingMaterialWeight=5000
                }
            };

            // Arrange
            var runId = 300;
            var createdBy = "no error";

            IEnumerable<ErrorReport> errorReports = Enumerable.Empty<ErrorReport>();
            mockErrorReport.Setup(m => m.InsertRecords(It.IsAny<IEnumerable<ErrorReport>>())).Callback<IEnumerable<ErrorReport>>(arg => errorReports = arg).Returns(Task.CompletedTask);

            // Act
            var reportsList = await _service.HandleErrors(pomDetails, orgDetails, runId, createdBy, CancellationToken.None);

            // Assert
            Assert.AreEqual(2, errorReports.Count(), "Expected 2 errors to be inserted.");
            var regError = errorReports.First();

            Assert.AreEqual((int)ErrorTypes.MissingRegistrationData, regError.ErrorTypeId, "Incorrect Error Type");
            Assert.AreEqual(100200, regError.ProducerId, "Incorrect Producer Id");
            Assert.AreEqual(null, regError.SubsidiaryId, "Incorrect Subsidiary Id");

            var pomError = errorReports.Last();
            Assert.AreEqual((int)ErrorTypes.MissingPOMData, pomError.ErrorTypeId, "Incorrect Error Type");
            Assert.AreEqual(200202, pomError.ProducerId, "Incorrect Producer Id");
            Assert.AreEqual("100101", pomError.SubsidiaryId, "Incorrect Subsidiary Id");
            Assert.AreEqual("01", pomError.LeaverCode);
        }
    }
}
