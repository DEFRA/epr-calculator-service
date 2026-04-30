using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Features.Billing.Constants;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class InvoicedProducerServiceTests
    {
        private ApplicationDBContext _dbContext = null!;
        private InvoicedProducerService _invoicedProducerService = null!;

        [TestInitialize]
        public void Init()
        {
            var fixture = TestFixtures.New();
            _dbContext = fixture.Freeze<ApplicationDBContext>();
            _invoicedProducerService = fixture.Freeze<InvoicedProducerService>();
        }

        [TestCleanup]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task GetProducerDetailsTest_Returns_CancelledProducers()
        {
            TestDataHelper.SeedDatabaseForInitialRun(_dbContext);

            // Arrange
            var producerIds = ImmutableHashSet.Create(1);

            // Act
            var result = await _invoicedProducerService.GetInvoicedProducerRecords(new RelativeYear(2025), producerIds);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task GetProducerDetailsTest_DoesNotReturns_CancelledProducers()
        {
            TestDataHelper.SeedDatabaseForUnclassified(_dbContext);

            // Arrange
            var producerIds = ImmutableHashSet.Create(1);

            // Act
            var result = await _invoicedProducerService.GetInvoicedProducerRecords(new RelativeYear(2025), producerIds);

            // Assert
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetProducers_Returns_Producers()
        {
            TestDataHelper.SeedDatabaseForUnclassified(_dbContext);

            // Act
            var result = await _invoicedProducerService.GetProducerIdsForRun(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task GetProducerDetails_Empty()
        {
            // Arrange
            var producerIds = ImmutableHashSet.Create(1);

            // Act
            var result = await _invoicedProducerService.GetInvoicedProducerRecords(new RelativeYear(2025), producerIds);

            // Assert
            Assert.IsNotNull(result);
        }

        // GetLatestAcceptedInvoicedProducerRecords inner-joins on CalculatorRunOrganisationDataDetails to populate
        // ProducerName/TradingName. Without a master + a detail row per producer (with SubsidiaryId null),
        // the join yields zero rows and every assertion below fails with "Sequence contains no elements".
        private void SeedProducerOrgDetails()
        {
            _dbContext.CalculatorRunOrganisationDataMaster.Add(new CalculatorRunOrganisationDataMaster
            {
                Id = 1,
                RelativeYear = new RelativeYear(2024),
                EffectiveFrom = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user"
            });

            _dbContext.CalculatorRunOrganisationDataDetails.AddRange(
                new CalculatorRunOrganisationDataDetail
                {
                    Id = 1,
                    OrganisationId = 101001,
                    SubsidiaryId = null,
                    OrganisationName = "Allied Packaging",
                    TradingName = "Allied Trading",
                    LoadTimeStamp = DateTime.UtcNow,
                    CalculatorRunOrganisationDataMasterId = 1
                },
                new CalculatorRunOrganisationDataDetail
                {
                    Id = 2,
                    OrganisationId = 101002,
                    SubsidiaryId = null,
                    OrganisationName = "Beeline Materials",
                    TradingName = "Beeline Trading",
                    LoadTimeStamp = DateTime.UtcNow,
                    CalculatorRunOrganisationDataMasterId = 1
                },
                new CalculatorRunOrganisationDataDetail
                {
                    Id = 3,
                    OrganisationId = 101003,
                    SubsidiaryId = null,
                    OrganisationName = "Cloud Boxes",
                    TradingName = "Cloud Trading",
                    LoadTimeStamp = DateTime.UtcNow,
                    CalculatorRunOrganisationDataMasterId = 1
                });
        }

        [TestMethod]
        public async Task GetLatestAcceptedInvoicedProducerRecords_NoCancels_UsesLatestAcceptedRun() // US608960 happy path
        {
            // Arrange
            const int AL = 1;  // Aluminium
            const int FC = 2;  // Fibre composite

            const int P1 = 101001; // Allied Packaging
            const int P2 = 101002; // Beeline Materials
            const int P3 = 101003; // Cloud Boxes

            // Materials
            _dbContext.Material.AddRange(
                new Material { Id = AL, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                new Material { Id = FC, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" }
            );

            _dbContext.CalculatorRuns.AddRange(
                new CalculatorRun { Id = 1, Name = "R1", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted },
                new CalculatorRun { Id = 2, Name = "R2", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted }
            );

            // Previous invoiced net tonnage
            _dbContext.ProducerInvoicedMaterialNetTonnage.AddRange(
                // R1
                new ProducerInvoicedMaterialNetTonnage { Id = 101, CalculatorRunId = 1, ProducerId = P1, MaterialId = AL, InvoicedNetTonnage = 6316.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 102, CalculatorRunId = 1, ProducerId = P1, MaterialId = FC, InvoicedNetTonnage = 1983.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 103, CalculatorRunId = 1, ProducerId = P2, MaterialId = AL, InvoicedNetTonnage = 839.999m },
                new ProducerInvoicedMaterialNetTonnage { Id = 104, CalculatorRunId = 1, ProducerId = P2, MaterialId = FC, InvoicedNetTonnage = 396.341m },
                new ProducerInvoicedMaterialNetTonnage { Id = 105, CalculatorRunId = 1, ProducerId = P3, MaterialId = AL, InvoicedNetTonnage = 2880.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 106, CalculatorRunId = 1, ProducerId = P3, MaterialId = FC, InvoicedNetTonnage = 43.500m },

                // R2 (latest)
                new ProducerInvoicedMaterialNetTonnage { Id = 201, CalculatorRunId = 2, ProducerId = P1, MaterialId = AL, InvoicedNetTonnage = 6316.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 202, CalculatorRunId = 2, ProducerId = P1, MaterialId = FC, InvoicedNetTonnage = 1983.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 203, CalculatorRunId = 2, ProducerId = P2, MaterialId = AL, InvoicedNetTonnage = 839.999m },
                new ProducerInvoicedMaterialNetTonnage { Id = 204, CalculatorRunId = 2, ProducerId = P2, MaterialId = FC, InvoicedNetTonnage = 396.341m },
                new ProducerInvoicedMaterialNetTonnage { Id = 205, CalculatorRunId = 2, ProducerId = P3, MaterialId = AL, InvoicedNetTonnage = 2880.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 206, CalculatorRunId = 2, ProducerId = P3, MaterialId = FC, InvoicedNetTonnage = 43.500m }
            );

            // Results file Suggested billing instructions – all Accepted, no Cancels
            _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
                // R1
                new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = P1, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 2, CalculatorRunId = 1, ProducerId = P2, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 3, CalculatorRunId = 1, ProducerId = P3, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },

                // R2
                new ProducerResultFileSuggestedBillingInstruction { Id = 4, CalculatorRunId = 2, ProducerId = P1, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 5, CalculatorRunId = 2, ProducerId = P2, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 6, CalculatorRunId = 2, ProducerId = P3, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
            );

            // Producer designated invoice instructions
            _dbContext.ProducerDesignatedRunInvoiceInstruction.AddRange(
                // R1
                new ProducerDesignatedRunInvoiceInstruction { Id = 11, ProducerId = P1, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 126157.44m, InvoiceAmount = 126157.44m, BillingInstructionId = "1_101001" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 12, ProducerId = P2, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 103966.48m, InvoiceAmount = 103966.48m, BillingInstructionId = "1_101002" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 13, ProducerId = P3, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 23712.34m, InvoiceAmount = 23712.34m, BillingInstructionId = "1_101003" },

                // R2
                new ProducerDesignatedRunInvoiceInstruction { Id = 14, ProducerId = P1, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 211377.83m, InvoiceAmount = 85220.39m, BillingInstructionId = "2_101001" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 15, ProducerId = P2, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 150000.00m, InvoiceAmount = 46033.52m, BillingInstructionId = "2_101002" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 16, ProducerId = P3, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 40367.29m, InvoiceAmount = 16654.95m, BillingInstructionId = "2_101003" }
            );

            SeedProducerOrgDetails();

            await _dbContext.SaveChangesAsync();

            // Act
            var invoicedProducerRecords = await _invoicedProducerService.GetLatestAcceptedInvoicedProducerRecords(new RelativeYear(2024));

            // Assert

            // From latest accepted run (R2)
            foreach (var mat in new[] { AL, FC })
            {
                var p1 = invoicedProducerRecords.Single(r => r.ProducerId == P1 && r.MaterialId == mat);
                Assert.AreEqual(2, p1.CalculatorRunId);
                Assert.AreEqual(211377.83m, p1.CurrentYearInvoicedTotalAfterThisRun);

                var p2 = invoicedProducerRecords.Single(r => r.ProducerId == P2 && r.MaterialId == mat);
                Assert.AreEqual(2, p2.CalculatorRunId);
                Assert.AreEqual(150000.00m, p2.CurrentYearInvoicedTotalAfterThisRun);

                var p3 = invoicedProducerRecords.Single(r => r.ProducerId == P3 && r.MaterialId == mat);
                Assert.AreEqual(2, p3.CalculatorRunId);
                Assert.AreEqual(40367.29m, p3.CurrentYearInvoicedTotalAfterThisRun);
            }

            //3 producers * 2 materials = 6
            Assert.AreEqual(6, invoicedProducerRecords.Length);
        }

        [TestMethod]
        public async Task GetLatestAcceptedInvoicedProducerRecords_CancelAcceptedInRun2_ExcludesPreviousInvoiceInRun1ForReappearingProducer() // US608960 AC1
        {
            //Arrange
            const int AL = 1;  // Aluminium
            const int FC = 2;  // Fibre composite

            const int P1 = 101001; // Allied Packaging
            const int P2 = 101002; // Beeline Materials (cancel Accepted in R2 reappears in R3)
            const int P3 = 101003; // Cloud Boxes

            // Materials
            _dbContext.Material.AddRange(
                new Material { Id = AL, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                new Material { Id = FC, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" }
            );

            _dbContext.CalculatorRuns.AddRange(
                new CalculatorRun { Id = 1, Name = "R1", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted },
                new CalculatorRun { Id = 2, Name = "R2", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted },
                new CalculatorRun { Id = 3, Name = "R3", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted }
            );

            // Previous invoiced net tonnage
            _dbContext.ProducerInvoicedMaterialNetTonnage.AddRange(

                // R1
                new ProducerInvoicedMaterialNetTonnage { Id = 101, CalculatorRunId = 1, ProducerId = P1, MaterialId = AL, InvoicedNetTonnage = 6316.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 102, CalculatorRunId = 1, ProducerId = P1, MaterialId = FC, InvoicedNetTonnage = 1983.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 103, CalculatorRunId = 1, ProducerId = P2, MaterialId = AL, InvoicedNetTonnage = 839.999m },
                new ProducerInvoicedMaterialNetTonnage { Id = 104, CalculatorRunId = 1, ProducerId = P2, MaterialId = FC, InvoicedNetTonnage = 396.341m },
                new ProducerInvoicedMaterialNetTonnage { Id = 105, CalculatorRunId = 1, ProducerId = P3, MaterialId = AL, InvoicedNetTonnage = 2880.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 106, CalculatorRunId = 1, ProducerId = P3, MaterialId = FC, InvoicedNetTonnage = 43.500m },

                // R2 (No data for P2)
                new ProducerInvoicedMaterialNetTonnage { Id = 201, CalculatorRunId = 2, ProducerId = P1, MaterialId = AL, InvoicedNetTonnage = 6316.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 202, CalculatorRunId = 2, ProducerId = P1, MaterialId = FC, InvoicedNetTonnage = 1983.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 203, CalculatorRunId = 2, ProducerId = P3, MaterialId = AL, InvoicedNetTonnage = 2880.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 204, CalculatorRunId = 2, ProducerId = P3, MaterialId = FC, InvoicedNetTonnage = 43.500m }
            );

            // Results file Suggested billing instructions
            // R1: all producers Accepted
            _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = P1, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 2, CalculatorRunId = 1, ProducerId = P2, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 3, CalculatorRunId = 1, ProducerId = P3, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
            );

            // R2: P1 Accepted, P2 Cancel Accepted, P3 Accepted
            _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 4, CalculatorRunId = 2, ProducerId = P1, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 5, CalculatorRunId = 2, ProducerId = P2, SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 6, CalculatorRunId = 2, ProducerId = P3, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
            );

            // Producer designated invoice instructions
            _dbContext.ProducerDesignatedRunInvoiceInstruction.AddRange(

                //run 1
                new ProducerDesignatedRunInvoiceInstruction { Id = 11, ProducerId = P1, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 126157.44m, InvoiceAmount = 126157.44m, BillingInstructionId = "1_101001" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 12, ProducerId = P2, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 103966.48m, InvoiceAmount = 103966.48m, BillingInstructionId = "1_101002" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 13, ProducerId = P3, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 23712.34m, InvoiceAmount = 23712.34m, BillingInstructionId = "1_101003" },


                // run 2 (P2 Cancelled null records)
                new ProducerDesignatedRunInvoiceInstruction { Id = 14, ProducerId = P1, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 211377.83m, InvoiceAmount = 85220.39m, BillingInstructionId = "2_101001" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 15, ProducerId = P2, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = null, InvoiceAmount = null, BillingInstructionId = "2_101002" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 16, ProducerId = P3, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 40367.29m, InvoiceAmount = 16654.95m, BillingInstructionId = "2_101003" }
            );

            SeedProducerOrgDetails();

            await _dbContext.SaveChangesAsync();

            // Act
            var invoicedProducerRecords = await _invoicedProducerService.GetLatestAcceptedInvoicedProducerRecords(new RelativeYear(2024));

            //Assert

            //PreviousInvoicedTonnage for P2 must not be present from R1 as R2 is Cancel (Accepted)
            Assert.IsFalse(invoicedProducerRecords.Any(r => r.ProducerId == P2));

            foreach (var mat in new[] { AL, FC })
            {
                //PreviousInvoicedTonnage for P1 and P3 are from Run 2
                var p1 = invoicedProducerRecords.Single(r => r.ProducerId == P1 && r.MaterialId == mat);
                Assert.AreEqual(2, p1.CalculatorRunId);
                Assert.AreEqual(211377.83m, p1.CurrentYearInvoicedTotalAfterThisRun);

                var p3 = invoicedProducerRecords.Single(r => r.ProducerId == P3 && r.MaterialId == mat);
                Assert.AreEqual(2, p3.CalculatorRunId);
                Assert.AreEqual(40367.29m, p3.CurrentYearInvoicedTotalAfterThisRun);
            }

            // 2 producers * 2 materials = 4 rows
            Assert.AreEqual(4, invoicedProducerRecords.Length);
        }

        [TestMethod]
        public async Task GetLatestAcceptedInvoicedProducerRecords_CancelRejectedInRun2_UsesInvoiceFromRun1ForReappearingProducer() // US608960 AC2
        {
            // Arrange
            const int AL = 1;  // Aluminium
            const int FC = 2;  // Fibre composite

            const int P1 = 101001; // Allied Packaging
            const int P2 = 101002; // Beeline Materials (cancel REJECTED in R2 reappears in R3)
            const int P3 = 101003; // Cloud Boxes

            // 2 materials only
            _dbContext.Material.AddRange(
                new Material { Id = AL, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                new Material { Id = FC, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" }
            );

            _dbContext.CalculatorRuns.AddRange(
                new CalculatorRun { Id = 1, Name = "R1", RelativeYear = new RelativeYear(2025), Classification = RunClassification.FinalRunCompleted },
                new CalculatorRun { Id = 2, Name = "R2", RelativeYear = new RelativeYear(2025), Classification = RunClassification.FinalRunCompleted }
            );

            // Previous invoiced net tonnage
            _dbContext.ProducerInvoicedMaterialNetTonnage.AddRange(
                // R1
                new ProducerInvoicedMaterialNetTonnage { Id = 101, CalculatorRunId = 1, ProducerId = P1, MaterialId = AL, InvoicedNetTonnage = 6316.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 102, CalculatorRunId = 1, ProducerId = P1, MaterialId = FC, InvoicedNetTonnage = 1983.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 103, CalculatorRunId = 1, ProducerId = P2, MaterialId = AL, InvoicedNetTonnage = 839.999m },
                new ProducerInvoicedMaterialNetTonnage { Id = 104, CalculatorRunId = 1, ProducerId = P2, MaterialId = FC, InvoicedNetTonnage = 396.341m },
                new ProducerInvoicedMaterialNetTonnage { Id = 105, CalculatorRunId = 1, ProducerId = P3, MaterialId = AL, InvoicedNetTonnage = 2880.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 106, CalculatorRunId = 1, ProducerId = P3, MaterialId = FC, InvoicedNetTonnage = 43.500m },

                new ProducerInvoicedMaterialNetTonnage { Id = 201, CalculatorRunId = 2, ProducerId = P1, MaterialId = AL, InvoicedNetTonnage = 6316.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 202, CalculatorRunId = 2, ProducerId = P1, MaterialId = FC, InvoicedNetTonnage = 1983.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 203, CalculatorRunId = 2, ProducerId = P3, MaterialId = AL, InvoicedNetTonnage = 2880.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 204, CalculatorRunId = 2, ProducerId = P3, MaterialId = FC, InvoicedNetTonnage = 43.500m }
            );

            // Results file Suggested billing instructions
            // R1: all INITIAL Accepted
            _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = P1, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 2, CalculatorRunId = 1, ProducerId = P2, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 3, CalculatorRunId = 1, ProducerId = P3, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
            );

            // R2: P1 DELTA Accepted, P2 Cancel REJECTED, P3 DELTA Accepted
            _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 4, CalculatorRunId = 2, ProducerId = P1, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 5, CalculatorRunId = 2, ProducerId = P2, SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel, BillingInstructionAcceptReject = BillingConstants.Action.Rejected },
                new ProducerResultFileSuggestedBillingInstruction { Id = 6, CalculatorRunId = 2, ProducerId = P3, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
            );

            // Producer designated invoice instructions
            _dbContext.ProducerDesignatedRunInvoiceInstruction.AddRange(
                // R1
                new ProducerDesignatedRunInvoiceInstruction { Id = 11, ProducerId = P1, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 126157.44m, InvoiceAmount = 126157.44m, BillingInstructionId = "1_101001" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 12, ProducerId = P2, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 103966.48m, InvoiceAmount = 103966.48m, BillingInstructionId = "1_101002" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 13, ProducerId = P3, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 23712.34m, InvoiceAmount = 23712.34m, BillingInstructionId = "1_101003" },

                // R2
                new ProducerDesignatedRunInvoiceInstruction { Id = 14, ProducerId = P1, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 211377.83m, InvoiceAmount = 85220.39m, BillingInstructionId = "2_101001" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 15, ProducerId = P2, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = null, InvoiceAmount = null, BillingInstructionId = "2_101002" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 16, ProducerId = P3, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 40367.29m, InvoiceAmount = 16654.95m, BillingInstructionId = "2_101003" }
            );

            SeedProducerOrgDetails();

            await _dbContext.SaveChangesAsync();

            // Act
            var invoicedProducerRecords = await _invoicedProducerService.GetLatestAcceptedInvoicedProducerRecords(new RelativeYear(2025));

            // Assert

            // P2 must be present and fetched from R1 as R2 cancel REJECTED
            foreach (var mat in new[] { AL, FC })
            {
                var p2 = invoicedProducerRecords.Single(r => r.ProducerId == P2 && r.MaterialId == mat);
                Assert.AreEqual(1, p2.CalculatorRunId);
                Assert.AreEqual(103966.48m, p2.CurrentYearInvoicedTotalAfterThisRun);
            }

            // P1 & P3 fetched from R2
            foreach (var mat in new[] { AL, FC })
            {
                var p1 = invoicedProducerRecords.Single(r => r.ProducerId == P1 && r.MaterialId == mat);
                Assert.AreEqual(2, p1.CalculatorRunId);
                Assert.AreEqual(211377.83m, p1.CurrentYearInvoicedTotalAfterThisRun);

                var p3 = invoicedProducerRecords.Single(r => r.ProducerId == P3 && r.MaterialId == mat);
                Assert.AreEqual(2, p3.CalculatorRunId);
                Assert.AreEqual(40367.29m, p3.CurrentYearInvoicedTotalAfterThisRun);
            }

            // 3 producers * 2 materials = 6 rows
            Assert.AreEqual(6, invoicedProducerRecords.Length);
        }

        [TestMethod]
        public async Task GetLatestAcceptedInvoicedProducerRecords_ReappearAcceptedInRun3_UsesRun3_IgnoringEarlierCancelAccepted() // US608960 AC 1
        {
            // Arrange
            const int AL = 1;  // Aluminium
            const int FC = 2;  // Fibre composite

            const int P1 = 101001; // Allied Packaging
            const int P2 = 101002; // Beeline Materials (R2 Cancel Accepted, R3 Reappear Accepted)
            const int P3 = 101003; // Cloud Boxes

            _dbContext.Material.AddRange(
                new Material { Id = AL, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                new Material { Id = FC, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" }
            );

            _dbContext.CalculatorRuns.AddRange(
                new CalculatorRun { Id = 1, Name = "R1", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted },
                new CalculatorRun { Id = 2, Name = "R2", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted },
                new CalculatorRun { Id = 3, Name = "R3", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted }
            );

            // Previous invoiced net tonnage
            _dbContext.ProducerInvoicedMaterialNetTonnage.AddRange(
                // R1
                new ProducerInvoicedMaterialNetTonnage { Id = 101, CalculatorRunId = 1, ProducerId = P1, MaterialId = AL, InvoicedNetTonnage = 6316.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 102, CalculatorRunId = 1, ProducerId = P1, MaterialId = FC, InvoicedNetTonnage = 1983.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 103, CalculatorRunId = 1, ProducerId = P2, MaterialId = AL, InvoicedNetTonnage = 839.999m },
                new ProducerInvoicedMaterialNetTonnage { Id = 104, CalculatorRunId = 1, ProducerId = P2, MaterialId = FC, InvoicedNetTonnage = 396.341m },
                new ProducerInvoicedMaterialNetTonnage { Id = 105, CalculatorRunId = 1, ProducerId = P3, MaterialId = AL, InvoicedNetTonnage = 2880.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 106, CalculatorRunId = 1, ProducerId = P3, MaterialId = FC, InvoicedNetTonnage = 43.500m },

                // R2 (no tonnage for P2 because Cancel Accepted)
                new ProducerInvoicedMaterialNetTonnage { Id = 201, CalculatorRunId = 2, ProducerId = P1, MaterialId = AL, InvoicedNetTonnage = 6316.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 202, CalculatorRunId = 2, ProducerId = P1, MaterialId = FC, InvoicedNetTonnage = 1983.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 203, CalculatorRunId = 2, ProducerId = P3, MaterialId = AL, InvoicedNetTonnage = 2880.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 204, CalculatorRunId = 2, ProducerId = P3, MaterialId = FC, InvoicedNetTonnage = 43.500m },

                // R3 (P2 reappears Accepted)
                new ProducerInvoicedMaterialNetTonnage { Id = 301, CalculatorRunId = 3, ProducerId = P1, MaterialId = AL, InvoicedNetTonnage = 6316.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 302, CalculatorRunId = 3, ProducerId = P1, MaterialId = FC, InvoicedNetTonnage = 1983.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 303, CalculatorRunId = 3, ProducerId = P2, MaterialId = AL, InvoicedNetTonnage = 839.999m },
                new ProducerInvoicedMaterialNetTonnage { Id = 304, CalculatorRunId = 3, ProducerId = P2, MaterialId = FC, InvoicedNetTonnage = 396.341m },
                new ProducerInvoicedMaterialNetTonnage { Id = 305, CalculatorRunId = 3, ProducerId = P3, MaterialId = AL, InvoicedNetTonnage = 2880.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 306, CalculatorRunId = 3, ProducerId = P3, MaterialId = FC, InvoicedNetTonnage = 43.500m }
            );

            // Results file Suggested billing instructions
            // R1: All Accepted
            _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = P1, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 2, CalculatorRunId = 1, ProducerId = P2, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 3, CalculatorRunId = 1, ProducerId = P3, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
            );

            // R2: P2 Cancel Accepted
            _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 4, CalculatorRunId = 2, ProducerId = P1, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 5, CalculatorRunId = 2, ProducerId = P2, SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 6, CalculatorRunId = 2, ProducerId = P3, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
            );

            // R3: P2 Reappear Accepted (non-cancel)
            _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 7, CalculatorRunId = 3, ProducerId = P1, SuggestedBillingInstruction = BillingConstants.Suggestion.Rebill, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 8, CalculatorRunId = 3, ProducerId = P2, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 9, CalculatorRunId = 3, ProducerId = P3, SuggestedBillingInstruction = BillingConstants.Suggestion.Rebill, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
            );

            // Designated invoice instructions
            _dbContext.ProducerDesignatedRunInvoiceInstruction.AddRange(
                // R1
                new ProducerDesignatedRunInvoiceInstruction { Id = 11, ProducerId = P1, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 126157.44m, InvoiceAmount = 126157.44m, BillingInstructionId = "1_101001" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 12, ProducerId = P2, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 103966.48m, InvoiceAmount = 103966.48m, BillingInstructionId = "1_101002" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 13, ProducerId = P3, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 23712.34m, InvoiceAmount = 23712.34m, BillingInstructionId = "1_101003" },

                // R2 (P2 cancelled)
                new ProducerDesignatedRunInvoiceInstruction { Id = 14, ProducerId = P1, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 211377.83m, InvoiceAmount = 85220.39m, BillingInstructionId = "2_101001" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 15, ProducerId = P2, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = null, InvoiceAmount = null, BillingInstructionId = "2_101002" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 16, ProducerId = P3, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 40367.29m, InvoiceAmount = 16654.95m, BillingInstructionId = "2_101003" },

                // R3 (latest accepted for all three)
                new ProducerDesignatedRunInvoiceInstruction { Id = 17, ProducerId = P1, CalculatorRunId = 3, CurrentYearInvoicedTotalAfterThisRun = 338317.32m, InvoiceAmount = 126242.54m, BillingInstructionId = "3_101001" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 18, ProducerId = P2, CalculatorRunId = 3, CurrentYearInvoicedTotalAfterThisRun = 104368.07m, InvoiceAmount = 401.59m, BillingInstructionId = "3_101002" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 19, ProducerId = P3, CalculatorRunId = 3, CurrentYearInvoicedTotalAfterThisRun = 62909.94m, InvoiceAmount = 23239.82m, BillingInstructionId = "3_101003" }
            );

            SeedProducerOrgDetails();

            await _dbContext.SaveChangesAsync();

            // Act
            var invoicedProducerRecords = await _invoicedProducerService.GetLatestAcceptedInvoicedProducerRecords(new RelativeYear(2024));

            // Assert – all producers now come from R3
            foreach (var mat in new[] { AL, FC })
            {
                var p1 = invoicedProducerRecords.Single(r => r.ProducerId == P1 && r.MaterialId == mat);
                Assert.AreEqual(3, p1.CalculatorRunId);
                Assert.AreEqual(338317.32m, p1.CurrentYearInvoicedTotalAfterThisRun);

                var p2 = invoicedProducerRecords.Single(r => r.ProducerId == P2 && r.MaterialId == mat);
                Assert.AreEqual(3, p2.CalculatorRunId);
                Assert.AreEqual(104368.07m, p2.CurrentYearInvoicedTotalAfterThisRun);

                var p3 = invoicedProducerRecords.Single(r => r.ProducerId == P3 && r.MaterialId == mat);
                Assert.AreEqual(3, p3.CalculatorRunId);
                Assert.AreEqual(62909.94m, p3.CurrentYearInvoicedTotalAfterThisRun);
            }

            // 3 producers * 2 materials = 6
            Assert.AreEqual(6, invoicedProducerRecords.Length);
        }

        [TestMethod]
        public async Task GetLatestAcceptedInvoicedProducerRecords_CancelRejectedInRun4_AfterReappearInRun3_StillUsesRun3() // US608960 AC 2
        {
            // Arrange
            const int AL = 1;  // Aluminium
            const int FC = 2;  // Fibre composite

            const int P1 = 101001; // Allied Packaging
            const int P2 = 101002; // Beeline Materials (R2 Cancel Accepted, R3 Reappear Accepted, R4 Cancel Rejected)
            const int P3 = 101003; // Cloud Boxes

            _dbContext.Material.AddRange(
                new Material { Id = AL, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                new Material { Id = FC, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" }
            );

            _dbContext.CalculatorRuns.AddRange(
                new CalculatorRun { Id = 1, Name = "R1", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted },
                new CalculatorRun { Id = 2, Name = "R2", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted },
                new CalculatorRun { Id = 3, Name = "R3", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted },
                new CalculatorRun { Id = 4, Name = "R4", RelativeYear = new RelativeYear(2024), Classification = RunClassification.FinalRunCompleted }
            );

            // Invoiced net tonnage
            _dbContext.ProducerInvoicedMaterialNetTonnage.AddRange(
                // R1
                new ProducerInvoicedMaterialNetTonnage { Id = 101, CalculatorRunId = 1, ProducerId = P1, MaterialId = AL, InvoicedNetTonnage = 6316.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 102, CalculatorRunId = 1, ProducerId = P1, MaterialId = FC, InvoicedNetTonnage = 1983.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 103, CalculatorRunId = 1, ProducerId = P2, MaterialId = AL, InvoicedNetTonnage = 839.999m },
                new ProducerInvoicedMaterialNetTonnage { Id = 104, CalculatorRunId = 1, ProducerId = P2, MaterialId = FC, InvoicedNetTonnage = 396.341m },
                new ProducerInvoicedMaterialNetTonnage { Id = 105, CalculatorRunId = 1, ProducerId = P3, MaterialId = AL, InvoicedNetTonnage = 2880.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 106, CalculatorRunId = 1, ProducerId = P3, MaterialId = FC, InvoicedNetTonnage = 43.500m },

                // R2 (no P2)
                new ProducerInvoicedMaterialNetTonnage { Id = 201, CalculatorRunId = 2, ProducerId = P1, MaterialId = AL, InvoicedNetTonnage = 6316.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 202, CalculatorRunId = 2, ProducerId = P1, MaterialId = FC, InvoicedNetTonnage = 1983.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 203, CalculatorRunId = 2, ProducerId = P3, MaterialId = AL, InvoicedNetTonnage = 2880.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 204, CalculatorRunId = 2, ProducerId = P3, MaterialId = FC, InvoicedNetTonnage = 43.500m },

                // R3 (all present again)
                new ProducerInvoicedMaterialNetTonnage { Id = 301, CalculatorRunId = 3, ProducerId = P1, MaterialId = AL, InvoicedNetTonnage = 6316.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 302, CalculatorRunId = 3, ProducerId = P1, MaterialId = FC, InvoicedNetTonnage = 1983.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 303, CalculatorRunId = 3, ProducerId = P2, MaterialId = AL, InvoicedNetTonnage = 839.999m },
                new ProducerInvoicedMaterialNetTonnage { Id = 304, CalculatorRunId = 3, ProducerId = P2, MaterialId = FC, InvoicedNetTonnage = 396.341m },
                new ProducerInvoicedMaterialNetTonnage { Id = 305, CalculatorRunId = 3, ProducerId = P3, MaterialId = AL, InvoicedNetTonnage = 2880.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 306, CalculatorRunId = 3, ProducerId = P3, MaterialId = FC, InvoicedNetTonnage = 43.500m },

                // R4 (no P2)
                new ProducerInvoicedMaterialNetTonnage { Id = 307, CalculatorRunId = 4, ProducerId = P1, MaterialId = AL, InvoicedNetTonnage = 6316.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 308, CalculatorRunId = 4, ProducerId = P1, MaterialId = FC, InvoicedNetTonnage = 1983.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 309, CalculatorRunId = 4, ProducerId = P3, MaterialId = AL, InvoicedNetTonnage = 2880.000m },
                new ProducerInvoicedMaterialNetTonnage { Id = 310, CalculatorRunId = 4, ProducerId = P3, MaterialId = FC, InvoicedNetTonnage = 43.500m }
            );

            // Suggested billing instructions:
            // R1: all Accepted
            _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = P1, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 2, CalculatorRunId = 1, ProducerId = P2, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 3, CalculatorRunId = 1, ProducerId = P3, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
            );

            // R2: P2 Cancel Accepted
            _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 4, CalculatorRunId = 2, ProducerId = P1, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 5, CalculatorRunId = 2, ProducerId = P2, SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 6, CalculatorRunId = 2, ProducerId = P3, SuggestedBillingInstruction = BillingConstants.Suggestion.Delta, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
            );

            // R3: P2 Reappear Accepted (non-cancel)
            _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 7, CalculatorRunId = 3, ProducerId = P1, SuggestedBillingInstruction = BillingConstants.Suggestion.Rebill, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 8, CalculatorRunId = 3, ProducerId = P2, SuggestedBillingInstruction = BillingConstants.Suggestion.Initial, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 9, CalculatorRunId = 3, ProducerId = P3, SuggestedBillingInstruction = BillingConstants.Suggestion.Rebill, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
            );

            // R4: P2 Cancel REJECTED
            _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 10, CalculatorRunId = 4, ProducerId = P1, SuggestedBillingInstruction = BillingConstants.Suggestion.Rebill, BillingInstructionAcceptReject = BillingConstants.Action.Accepted },
                new ProducerResultFileSuggestedBillingInstruction { Id = 11, CalculatorRunId = 4, ProducerId = P2, SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel, BillingInstructionAcceptReject = BillingConstants.Action.Rejected },
                new ProducerResultFileSuggestedBillingInstruction { Id = 12, CalculatorRunId = 4, ProducerId = P3, SuggestedBillingInstruction = BillingConstants.Suggestion.Rebill, BillingInstructionAcceptReject = BillingConstants.Action.Accepted }
            );

            // Designated invoice instructions
            _dbContext.ProducerDesignatedRunInvoiceInstruction.AddRange(
                // R1
                new ProducerDesignatedRunInvoiceInstruction { Id = 11, ProducerId = P1, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 126157.44m, InvoiceAmount = 126157.44m, BillingInstructionId = "1_101001" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 12, ProducerId = P2, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 103966.48m, InvoiceAmount = 103966.48m, BillingInstructionId = "1_101002" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 13, ProducerId = P3, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 23712.34m, InvoiceAmount = 23712.34m, BillingInstructionId = "1_101003" },

                // R2 (P2 cancel accepted)
                new ProducerDesignatedRunInvoiceInstruction { Id = 14, ProducerId = P1, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 211377.83m, InvoiceAmount = 85220.39m, BillingInstructionId = "2_101001" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 15, ProducerId = P2, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = null, InvoiceAmount = null, BillingInstructionId = "2_101002" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 16, ProducerId = P3, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 40367.29m, InvoiceAmount = 16654.95m, BillingInstructionId = "2_101003" },

                // R3 (P2 reappear accepted)
                new ProducerDesignatedRunInvoiceInstruction { Id = 17, ProducerId = P1, CalculatorRunId = 3, CurrentYearInvoicedTotalAfterThisRun = 338317.32m, InvoiceAmount = 126242.54m, BillingInstructionId = "3_101001" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 18, ProducerId = P2, CalculatorRunId = 3, CurrentYearInvoicedTotalAfterThisRun = 104368.07m, InvoiceAmount = 401.59m, BillingInstructionId = "3_101002" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 19, ProducerId = P3, CalculatorRunId = 3, CurrentYearInvoicedTotalAfterThisRun = 62909.94m, InvoiceAmount = 23239.82m, BillingInstructionId = "3_101003" },

                // R4 P2 cancel rejected
                new ProducerDesignatedRunInvoiceInstruction { Id = 20, ProducerId = P1, CalculatorRunId = 4, CurrentYearInvoicedTotalAfterThisRun = 350000.00m, InvoiceAmount = 11682.68m, BillingInstructionId = "4_101001" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 21, ProducerId = P2, CalculatorRunId = 4, CurrentYearInvoicedTotalAfterThisRun = null, InvoiceAmount = null, BillingInstructionId = "4_101002" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 22, ProducerId = P3, CalculatorRunId = 4, CurrentYearInvoicedTotalAfterThisRun = 65000.00m, InvoiceAmount = 208.71m, BillingInstructionId = "4_101003" }
            );

            SeedProducerOrgDetails();

            await _dbContext.SaveChangesAsync();

            // Act
            var invoicedProducerRecords = await _invoicedProducerService.GetLatestAcceptedInvoicedProducerRecords(new RelativeYear(2024));

            // P2 comes from R3 as rejected in R4; P1 & P3 are from R4
            foreach (var mat in new[] { AL, FC })
            {
                var p2 = invoicedProducerRecords.Single(r => r.ProducerId == P2 && r.MaterialId == mat);
                Assert.AreEqual(3, p2.CalculatorRunId);
                Assert.AreEqual(104368.07m, p2.CurrentYearInvoicedTotalAfterThisRun);

                var p1 = invoicedProducerRecords.Single(r => r.ProducerId == P1 && r.MaterialId == mat);
                Assert.AreEqual(4, p1.CalculatorRunId);
                Assert.AreEqual(350000.00m, p1.CurrentYearInvoicedTotalAfterThisRun);

                var p3 = invoicedProducerRecords.Single(r => r.ProducerId == P3 && r.MaterialId == mat);
                Assert.AreEqual(4, p3.CalculatorRunId);
                Assert.AreEqual(65000.00m, p3.CurrentYearInvoicedTotalAfterThisRun);
            }

            // 3 producers * 2 materials = 6
            Assert.AreEqual(6, invoicedProducerRecords.Length);
        }
    }
}
