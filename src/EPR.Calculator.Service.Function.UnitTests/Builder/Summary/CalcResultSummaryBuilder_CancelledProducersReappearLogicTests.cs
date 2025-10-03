using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary
{
    [TestClass]
    public class CalcResultSummaryBuilder_CancelledProducersReappearLogicTests
    {
        private readonly ApplicationDBContext context;
        private readonly CalcResultSummaryBuilder calcResultsService;

        public CalcResultSummaryBuilder_CancelledProducersReappearLogicTests()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase("CancelledProducersReappearLogic_DB")
                .Options;

            context = new ApplicationDBContext(dbContextOptions);
            calcResultsService = new CalcResultSummaryBuilder(context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }

        [TestMethod]
        public void GetPreviousInvoicedTonnage_NoCancels_UsesLatestAcceptedRun() // US608960 happy path
        {
            // Arrange
            const string financialYear = "2024-25";
            const int AL = 1;  // Aluminium
            const int FC = 2;  // Fibre composite

            const int P1 = 101001; // Allied Packaging
            const int P2 = 101002; // Beeline Materials
            const int P3 = 101003; // Cloud Boxes

            // Materials
            context.Material.AddRange(
                new Material { Id = AL, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                new Material { Id = FC, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" }
            );

            var fy = new CalculatorRunFinancialYear { Name = financialYear };

            context.CalculatorRuns.AddRange(
                new CalculatorRun { Id = 1, Name = "R1", FinancialYearId = financialYear, Financial_Year = fy, CalculatorRunClassificationId = RunClassificationStatusIds.FINALRUNCOMPLETEDID },
                new CalculatorRun { Id = 2, Name = "R2", FinancialYearId = financialYear, Financial_Year = fy, CalculatorRunClassificationId = RunClassificationStatusIds.FINALRUNCOMPLETEDID }
            );

            // Previous invoiced net tonnage
            context.ProducerInvoicedMaterialNetTonnage.AddRange(
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
            context.ProducerResultFileSuggestedBillingInstruction.AddRange(
                // R1
                new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = P1, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 2, CalculatorRunId = 1, ProducerId = P2, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 3, CalculatorRunId = 1, ProducerId = P3, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" },

                // R2
                new ProducerResultFileSuggestedBillingInstruction { Id = 4, CalculatorRunId = 2, ProducerId = P1, SuggestedBillingInstruction = "DELTA", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 5, CalculatorRunId = 2, ProducerId = P2, SuggestedBillingInstruction = "DELTA", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 6, CalculatorRunId = 2, ProducerId = P3, SuggestedBillingInstruction = "DELTA", BillingInstructionAcceptReject = "Accepted" }
            );

            // Producer designated invoice instructions
            context.ProducerDesignatedRunInvoiceInstruction.AddRange(
                // R1
                new ProducerDesignatedRunInvoiceInstruction { Id = 11, ProducerId = P1, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 126157.44m, InvoiceAmount = 126157.44m, BillingInstructionId = "1_101001" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 12, ProducerId = P2, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 103966.48m, InvoiceAmount = 103966.48m, BillingInstructionId = "1_101002" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 13, ProducerId = P3, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 23712.34m, InvoiceAmount = 23712.34m, BillingInstructionId = "1_101003" },

                // R2
                new ProducerDesignatedRunInvoiceInstruction { Id = 14, ProducerId = P1, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 211377.83m, InvoiceAmount = 85220.39m, BillingInstructionId = "2_101001" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 15, ProducerId = P2, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 150000.00m, InvoiceAmount = 46033.52m, BillingInstructionId = "2_101002" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 16, ProducerId = P3, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 40367.29m, InvoiceAmount = 16654.95m, BillingInstructionId = "2_101003" }
            );

            context.SaveChanges();

            // Act
            var previous = calcResultsService.GetPreviousInvoicedTonnageFromDb(financialYear).ToList();

            // Assert

            // From latest accepted run (R2)
            foreach (var mat in new[] { AL, FC })
            {
                var p1 = previous.Single(r => r.InvoicedTonnage!.ProducerId == P1 && r.InvoicedTonnage.MaterialId == mat);
                Assert.AreEqual(2, p1.CalculatorRunId);
                Assert.IsNotNull(p1.InvoiceInstruction);
                Assert.AreEqual(211377.83m, p1.InvoiceInstruction!.CurrentYearInvoicedTotalAfterThisRun);

                var p2 = previous.Single(r => r.InvoicedTonnage!.ProducerId == P2 && r.InvoicedTonnage.MaterialId == mat);
                Assert.AreEqual(2, p2.CalculatorRunId);
                Assert.IsNotNull(p2.InvoiceInstruction);
                Assert.AreEqual(150000.00m, p2.InvoiceInstruction!.CurrentYearInvoicedTotalAfterThisRun);

                var p3 = previous.Single(r => r.InvoicedTonnage!.ProducerId == P3 && r.InvoicedTonnage.MaterialId == mat);
                Assert.AreEqual(2, p3.CalculatorRunId);
                Assert.IsNotNull(p3.InvoiceInstruction);
                Assert.AreEqual(40367.29m, p3.InvoiceInstruction!.CurrentYearInvoicedTotalAfterThisRun);
            }

            //3 producers * 2 materials = 6
            Assert.AreEqual(6, previous.Count);
        }

        [TestMethod]
        public void GetPreviousInvoicedTonnage_CancelAcceptedInRun2_ExcludesPreviousInvoiceInRun1ForReappearingProducer() // US608960 AC1
        {
            //Arrange
            const string financialYear = "2024-25";
            const int AL = 1;  // Aluminium
            const int FC = 2;  // Fibre composite

            const int P1 = 101001; // Allied Packaging
            const int P2 = 101002; // Beeline Materials (cancel Accepted in R2 reappears in R3)
            const int P3 = 101003; // Cloud Boxes

            // Materials
            context.Material.AddRange(
                new Material { Id = AL, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                new Material { Id = FC, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" }
            );

            var fy = new CalculatorRunFinancialYear { Name = financialYear };

            context.CalculatorRuns.AddRange(
                new CalculatorRun { Id = 1, Name = "R1", FinancialYearId = financialYear, Financial_Year = fy, CalculatorRunClassificationId = RunClassificationStatusIds.FINALRUNCOMPLETEDID },
                new CalculatorRun { Id = 2, Name = "R2", FinancialYearId = financialYear, Financial_Year = fy, CalculatorRunClassificationId = RunClassificationStatusIds.FINALRUNCOMPLETEDID },
                new CalculatorRun { Id = 3, Name = "R3", FinancialYearId = financialYear, Financial_Year = fy, CalculatorRunClassificationId = RunClassificationStatusIds.FINALRUNCOMPLETEDID }
            );

            // Previous invoiced net tonnage
            context.ProducerInvoicedMaterialNetTonnage.AddRange(

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
            context.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = P1, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 2, CalculatorRunId = 1, ProducerId = P2, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 3, CalculatorRunId = 1, ProducerId = P3, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" }
            );

            // R2: P1 Accepted, P2 Cancel Accepted, P3 Accepted
            context.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 4, CalculatorRunId = 2, ProducerId = P1, SuggestedBillingInstruction = "DELTA", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 5, CalculatorRunId = 2, ProducerId = P2, SuggestedBillingInstruction = "Cancel", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 6, CalculatorRunId = 2, ProducerId = P3, SuggestedBillingInstruction = "DELTA", BillingInstructionAcceptReject = "Accepted" }
            );

            // Producer designated invoice instructions
            context.ProducerDesignatedRunInvoiceInstruction.AddRange(

                //run 1 
                new ProducerDesignatedRunInvoiceInstruction { Id = 11, ProducerId = P1, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 126157.44m, InvoiceAmount = 126157.44m, BillingInstructionId = "1_101001" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 12, ProducerId = P2, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 103966.48m, InvoiceAmount = 103966.48m, BillingInstructionId = "1_101002" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 13, ProducerId = P3, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 23712.34m, InvoiceAmount = 23712.34m, BillingInstructionId = "1_101003" },


                // run 2 (P2 Cancelled null records)
                new ProducerDesignatedRunInvoiceInstruction { Id = 14, ProducerId = P1, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 211377.83m, InvoiceAmount = 85220.39m, BillingInstructionId = "2_101001" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 15, ProducerId = P2, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = null, InvoiceAmount = null, BillingInstructionId = "2_101002" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 16, ProducerId = P3, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 40367.29m, InvoiceAmount = 16654.95m, BillingInstructionId = "2_101003" }
            );

            context.SaveChanges();

            // Act
            var producerInvoicedDtos = calcResultsService.GetPreviousInvoicedTonnageFromDb(financialYear).ToList();

            //Assert

            //PreviousInvoicedTonnage for P2 must not be present from R1 as R2 is Cancel (Accepted)
            Assert.IsFalse(producerInvoicedDtos.Any(r => r.InvoicedTonnage!.ProducerId == P2));

            foreach (var mat in new[] { AL, FC })
            {
                //PreviousInvoicedTonnage for P1 and P3 are from Run 2 
                var p1 = producerInvoicedDtos.Single(r => r.InvoicedTonnage!.ProducerId == P1 && r.InvoicedTonnage.MaterialId == mat);
                Assert.AreEqual(2, p1.CalculatorRunId);
                Assert.IsNotNull(p1.InvoiceInstruction);
                Assert.AreEqual(211377.83m, p1.InvoiceInstruction!.CurrentYearInvoicedTotalAfterThisRun);

                var p3 = producerInvoicedDtos.Single(r => r.InvoicedTonnage!.ProducerId == P3 && r.InvoicedTonnage.MaterialId == mat);
                Assert.AreEqual(2, p3.CalculatorRunId);
                Assert.IsNotNull(p3.InvoiceInstruction);
                Assert.AreEqual(40367.29m, p3.InvoiceInstruction!.CurrentYearInvoicedTotalAfterThisRun);
            }

            // 2 producers * 2 materials = 4 rows
            Assert.AreEqual(4, producerInvoicedDtos.Count);
        }

        [TestMethod]
        public void GetPreviousInvoicedTonnage_CancelRejectedInRun2_UsesInvoiceFromRun1ForReappearingProducer() // US608960 AC2
        {
            // Arrange
            const string financialYear = "2024-25";
            const int AL = 1;  // Aluminium
            const int FC = 2;  // Fibre composite

            const int P1 = 101001; // Allied Packaging
            const int P2 = 101002; // Beeline Materials (cancel REJECTED in R2 reappears in R3)
            const int P3 = 101003; // Cloud Boxes

            // 2 materials only
            context.Material.AddRange(
                new Material { Id = AL, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                new Material { Id = FC, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" }
            );

            var fy = new CalculatorRunFinancialYear { Name = financialYear };

            context.CalculatorRuns.AddRange(
                new CalculatorRun { Id = 1, Name = "R1", FinancialYearId = financialYear, Financial_Year = fy, CalculatorRunClassificationId = RunClassificationStatusIds.FINALRUNCOMPLETEDID },
                new CalculatorRun { Id = 2, Name = "R2", FinancialYearId = financialYear, Financial_Year = fy, CalculatorRunClassificationId = RunClassificationStatusIds.FINALRUNCOMPLETEDID }
            );

            // Previous invoiced net tonnage
            context.ProducerInvoicedMaterialNetTonnage.AddRange(
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
            context.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = P1, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 2, CalculatorRunId = 1, ProducerId = P2, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 3, CalculatorRunId = 1, ProducerId = P3, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" }
            );

            // R2: P1 DELTA Accepted, P2 Cancel REJECTED, P3 DELTA Accepted
            context.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 4, CalculatorRunId = 2, ProducerId = P1, SuggestedBillingInstruction = "DELTA", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 5, CalculatorRunId = 2, ProducerId = P2, SuggestedBillingInstruction = "Cancel", BillingInstructionAcceptReject = "Rejected" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 6, CalculatorRunId = 2, ProducerId = P3, SuggestedBillingInstruction = "DELTA", BillingInstructionAcceptReject = "Accepted" }
            );

            // Producer designated invoice instructions
            context.ProducerDesignatedRunInvoiceInstruction.AddRange(
                // R1
                new ProducerDesignatedRunInvoiceInstruction { Id = 11, ProducerId = P1, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 126157.44m, InvoiceAmount = 126157.44m, BillingInstructionId = "1_101001" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 12, ProducerId = P2, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 103966.48m, InvoiceAmount = 103966.48m, BillingInstructionId = "1_101002" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 13, ProducerId = P3, CalculatorRunId = 1, CurrentYearInvoicedTotalAfterThisRun = 23712.34m, InvoiceAmount = 23712.34m, BillingInstructionId = "1_101003" },

                // R2
                new ProducerDesignatedRunInvoiceInstruction { Id = 14, ProducerId = P1, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 211377.83m, InvoiceAmount = 85220.39m, BillingInstructionId = "2_101001" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 15, ProducerId = P2, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = null, InvoiceAmount = null, BillingInstructionId = "2_101002" },
                new ProducerDesignatedRunInvoiceInstruction { Id = 16, ProducerId = P3, CalculatorRunId = 2, CurrentYearInvoicedTotalAfterThisRun = 40367.29m, InvoiceAmount = 16654.95m, BillingInstructionId = "2_101003" }
            );

            context.SaveChanges();

            // Act
            var previous = calcResultsService.GetPreviousInvoicedTonnageFromDb(financialYear).ToList();

            // Assert

            // P2 must be present and fetched from R1 as R2 cancel REJECTED
            foreach (var mat in new[] { AL, FC })
            {
                var p2 = previous.Single(r => r.InvoicedTonnage!.ProducerId == P2 && r.InvoicedTonnage.MaterialId == mat);
                Assert.AreEqual(1, p2.CalculatorRunId);
                Assert.IsNotNull(p2.InvoiceInstruction);
                Assert.AreEqual(103966.48m, p2.InvoiceInstruction!.CurrentYearInvoicedTotalAfterThisRun);
            }

            // P1 & P3 fetched from R2
            foreach (var mat in new[] { AL, FC })
            {
                var p1 = previous.Single(r => r.InvoicedTonnage!.ProducerId == P1 && r.InvoicedTonnage.MaterialId == mat);
                Assert.AreEqual(2, p1.CalculatorRunId);
                Assert.IsNotNull(p1.InvoiceInstruction);
                Assert.AreEqual(211377.83m, p1.InvoiceInstruction!.CurrentYearInvoicedTotalAfterThisRun);

                var p3 = previous.Single(r => r.InvoicedTonnage!.ProducerId == P3 && r.InvoicedTonnage.MaterialId == mat);
                Assert.AreEqual(2, p3.CalculatorRunId);
                Assert.IsNotNull(p3.InvoiceInstruction);
                Assert.AreEqual(40367.29m, p3.InvoiceInstruction!.CurrentYearInvoicedTotalAfterThisRun);
            }

            // 3 producers * 2 materials = 6 rows
            Assert.AreEqual(6, previous.Count);
        }

        [TestMethod]
        public void GetPreviousInvoicedTonnage_ReappearAcceptedInRun3_UsesRun3_IgnoringEarlierCancelAccepted() // US608960 AC 1
        {
            // Arrange
            const string financialYear = "2024-25";
            const int AL = 1;  // Aluminium
            const int FC = 2;  // Fibre composite

            const int P1 = 101001; // Allied Packaging
            const int P2 = 101002; // Beeline Materials (R2 Cancel Accepted, R3 Reappear Accepted)
            const int P3 = 101003; // Cloud Boxes

            context.Material.AddRange(
                new Material { Id = AL, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                new Material { Id = FC, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" }
            );

            var fy = new CalculatorRunFinancialYear { Name = financialYear };

            context.CalculatorRuns.AddRange(
                new CalculatorRun { Id = 1, Name = "R1", FinancialYearId = financialYear, Financial_Year = fy, CalculatorRunClassificationId = RunClassificationStatusIds.FINALRUNCOMPLETEDID },
                new CalculatorRun { Id = 2, Name = "R2", FinancialYearId = financialYear, Financial_Year = fy, CalculatorRunClassificationId = RunClassificationStatusIds.FINALRUNCOMPLETEDID },
                new CalculatorRun { Id = 3, Name = "R3", FinancialYearId = financialYear, Financial_Year = fy, CalculatorRunClassificationId = RunClassificationStatusIds.FINALRUNCOMPLETEDID }
            );

            // Previous invoiced net tonnage
            context.ProducerInvoicedMaterialNetTonnage.AddRange(
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
            context.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = P1, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 2, CalculatorRunId = 1, ProducerId = P2, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 3, CalculatorRunId = 1, ProducerId = P3, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" }
            );

            // R2: P2 Cancel Accepted
            context.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 4, CalculatorRunId = 2, ProducerId = P1, SuggestedBillingInstruction = "DELTA", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 5, CalculatorRunId = 2, ProducerId = P2, SuggestedBillingInstruction = "Cancel", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 6, CalculatorRunId = 2, ProducerId = P3, SuggestedBillingInstruction = "DELTA", BillingInstructionAcceptReject = "Accepted" }
            );

            // R3: P2 Reappear Accepted (non-cancel)
            context.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 7, CalculatorRunId = 3, ProducerId = P1, SuggestedBillingInstruction = "REBILL", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 8, CalculatorRunId = 3, ProducerId = P2, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 9, CalculatorRunId = 3, ProducerId = P3, SuggestedBillingInstruction = "REBILL", BillingInstructionAcceptReject = "Accepted" }
            );

            // Designated invoice instructions
            context.ProducerDesignatedRunInvoiceInstruction.AddRange(
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

            context.SaveChanges();

            // Act
            var previous = calcResultsService.GetPreviousInvoicedTonnageFromDb(financialYear).ToList();

            // Assert – all producers now come from R3
            foreach (var mat in new[] { AL, FC })
            {
                var p1 = previous.Single(r => r.InvoicedTonnage!.ProducerId == P1 && r.InvoicedTonnage.MaterialId == mat);
                Assert.AreEqual(3, p1.CalculatorRunId);
                Assert.IsNotNull(p1.InvoiceInstruction);
                Assert.AreEqual(338317.32m, p1.InvoiceInstruction!.CurrentYearInvoicedTotalAfterThisRun);

                var p2 = previous.Single(r => r.InvoicedTonnage!.ProducerId == P2 && r.InvoicedTonnage.MaterialId == mat);
                Assert.AreEqual(3, p2.CalculatorRunId);
                Assert.IsNotNull(p2.InvoiceInstruction);
                Assert.AreEqual(104368.07m, p2.InvoiceInstruction!.CurrentYearInvoicedTotalAfterThisRun);

                var p3 = previous.Single(r => r.InvoicedTonnage!.ProducerId == P3 && r.InvoicedTonnage.MaterialId == mat);
                Assert.AreEqual(3, p3.CalculatorRunId);
                Assert.IsNotNull(p3.InvoiceInstruction);
                Assert.AreEqual(62909.94m, p3.InvoiceInstruction!.CurrentYearInvoicedTotalAfterThisRun);
            }

            // 3 producers * 2 materials = 6
            Assert.AreEqual(6, previous.Count);
        }

        [TestMethod]
        public void GetPreviousInvoicedTonnage_CancelRejectedInRun4_AfterReappearInRun3_StillUsesRun3() // US608960 AC 2
        {
            // Arrange
            const string financialYear = "2024-25";
            const int AL = 1;  // Aluminium
            const int FC = 2;  // Fibre composite

            const int P1 = 101001; // Allied Packaging
            const int P2 = 101002; // Beeline Materials (R2 Cancel Accepted, R3 Reappear Accepted, R4 Cancel Rejected)
            const int P3 = 101003; // Cloud Boxes

            context.Material.AddRange(
                new Material { Id = AL, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                new Material { Id = FC, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" }
            );

            var fy = new CalculatorRunFinancialYear { Name = financialYear };

            context.CalculatorRuns.AddRange(
                new CalculatorRun { Id = 1, Name = "R1", FinancialYearId = financialYear, Financial_Year = fy, CalculatorRunClassificationId = RunClassificationStatusIds.FINALRUNCOMPLETEDID },
                new CalculatorRun { Id = 2, Name = "R2", FinancialYearId = financialYear, Financial_Year = fy, CalculatorRunClassificationId = RunClassificationStatusIds.FINALRUNCOMPLETEDID },
                new CalculatorRun { Id = 3, Name = "R3", FinancialYearId = financialYear, Financial_Year = fy, CalculatorRunClassificationId = RunClassificationStatusIds.FINALRUNCOMPLETEDID },
                new CalculatorRun { Id = 4, Name = "R4", FinancialYearId = financialYear, Financial_Year = fy, CalculatorRunClassificationId = RunClassificationStatusIds.FINALRUNCOMPLETEDID }
            );

            // Invoiced net tonnage
            context.ProducerInvoicedMaterialNetTonnage.AddRange(
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
            context.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = P1, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 2, CalculatorRunId = 1, ProducerId = P2, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 3, CalculatorRunId = 1, ProducerId = P3, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" }
            );

            // R2: P2 Cancel Accepted
            context.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 4, CalculatorRunId = 2, ProducerId = P1, SuggestedBillingInstruction = "DELTA", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 5, CalculatorRunId = 2, ProducerId = P2, SuggestedBillingInstruction = "Cancel", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 6, CalculatorRunId = 2, ProducerId = P3, SuggestedBillingInstruction = "DELTA", BillingInstructionAcceptReject = "Accepted" }
            );

            // R3: P2 Reappear Accepted (non-cancel)
            context.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 7, CalculatorRunId = 3, ProducerId = P1, SuggestedBillingInstruction = "REBILL", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 8, CalculatorRunId = 3, ProducerId = P2, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 9, CalculatorRunId = 3, ProducerId = P3, SuggestedBillingInstruction = "REBILL", BillingInstructionAcceptReject = "Accepted" }
            );

            // R4: P2 Cancel REJECTED
            context.ProducerResultFileSuggestedBillingInstruction.AddRange(
                new ProducerResultFileSuggestedBillingInstruction { Id = 10, CalculatorRunId = 4, ProducerId = P1, SuggestedBillingInstruction = "REBILL", BillingInstructionAcceptReject = "Accepted" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 11, CalculatorRunId = 4, ProducerId = P2, SuggestedBillingInstruction = "Cancel", BillingInstructionAcceptReject = "Rejected" },
                new ProducerResultFileSuggestedBillingInstruction { Id = 12, CalculatorRunId = 4, ProducerId = P3, SuggestedBillingInstruction = "REBILL", BillingInstructionAcceptReject = "Accepted" }
            );

            // Designated invoice instructions
            context.ProducerDesignatedRunInvoiceInstruction.AddRange(
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

            context.SaveChanges();

            // Act
            var previous = calcResultsService.GetPreviousInvoicedTonnageFromDb(financialYear).ToList();

            // P2 comes from R3 as rejected in R4; P1 & P3 are from R4
            foreach (var mat in new[] { AL, FC })
            {
                var p2 = previous.Single(r => r.InvoicedTonnage!.ProducerId == P2 && r.InvoicedTonnage.MaterialId == mat);
                Assert.AreEqual(3, p2.CalculatorRunId);
                Assert.IsNotNull(p2.InvoiceInstruction);
                Assert.AreEqual(104368.07m, p2.InvoiceInstruction!.CurrentYearInvoicedTotalAfterThisRun);

                var p1 = previous.Single(r => r.InvoicedTonnage!.ProducerId == P1 && r.InvoicedTonnage.MaterialId == mat);
                Assert.AreEqual(4, p1.CalculatorRunId);
                Assert.IsNotNull(p1.InvoiceInstruction);
                Assert.AreEqual(350000.00m, p1.InvoiceInstruction!.CurrentYearInvoicedTotalAfterThisRun);

                var p3 = previous.Single(r => r.InvoicedTonnage!.ProducerId == P3 && r.InvoicedTonnage.MaterialId == mat);
                Assert.AreEqual(4, p3.CalculatorRunId);
                Assert.IsNotNull(p3.InvoiceInstruction);
                Assert.AreEqual(65000.00m, p3.InvoiceInstruction!.CurrentYearInvoicedTotalAfterThisRun);
            }

            // 3 producers * 2 materials = 6
            Assert.AreEqual(6, previous.Count);
        }
    }
}