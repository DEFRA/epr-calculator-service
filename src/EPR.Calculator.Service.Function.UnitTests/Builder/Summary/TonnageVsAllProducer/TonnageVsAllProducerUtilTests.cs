using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Builder.Summary.TonnageVsAllProducer;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.TonnageVsAllProducer
{
    [TestClass]
    public class TonnageVsAllProducerUtilTests
    {
        [TestMethod]
        public void CanCallGetPercentageofProducerReportedHHTonnagevsAllProducersTotal()
        {
            // Arrange
            var producers = TestFixtures.Legacy.Create<List<ProducerDetail>>();

            var testProducerId = TestFixtures.Legacy.Create<int>();
            var testCalculatorRunId = TestFixtures.Legacy.Create<int>();
            var testSubsidaryId = TestFixtures.Legacy.Create<string>();
            var materialDetails = TestFixtures.Legacy.Create<ImmutableArray<MaterialDto>>();

            var allResults = GenerateAllResults(testProducerId, testCalculatorRunId, testSubsidaryId);

            producers.First().ProducerId = testProducerId;
            producers.First().SubsidiaryId = testSubsidaryId;
            producers.First().CalculatorRunId = testCalculatorRunId;
            allResults.First().ProducerReportedMaterial.MaterialId = materialDetails.First().Id;

            var scaledupProducers = TestFixtures.Legacy.Create<List<CalcResultScaledupProducer>>();
            var partialObligations = new List<CalcResultPartialObligation>();

            var TotalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(allResults, materialDetails, producers.First().CalculatorRunId, scaledupProducers, partialObligations);
            // Act
            var result = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducersTotal(producers, TotalPackagingTonnage);

            // Assert
            Assert.AreEqual(50, result);
        }

        /// <summary>
        /// Check that the percentage of HH tonnage returns zero if there is no producer details corresponding
        /// to the ID given in the materials details.
        /// </summary>
        [TestMethod]
        public void GetPercentageofProducerReportedHHTonnagevsAllProducersTotal_ReturnsZeroWhenNoMatchingProducer()
        {
            // Arrange
            var producers = TestFixtures.Legacy.Create<List<ProducerDetail>>();
            var allResults = TestFixtures.Legacy.Create<List<CalcResultProducerAndReportMaterialDetail>>();
            var materialDetails = TestFixtures.Legacy.Create<ImmutableArray<MaterialDto>>();

            var testProducerId = TestFixtures.Legacy.Create<int>();
            var testCalculatorRunId = TestFixtures.Legacy.Create<int>();

            allResults.First().ProducerReportedMaterial.ProducerDetailId = testProducerId;
            allResults.First().ProducerDetail.Id = testProducerId;
            allResults.First().ProducerDetail.ProducerId = testProducerId;
            allResults.First().ProducerDetail.CalculatorRunId = testCalculatorRunId;
            allResults.First().ProducerReportedMaterial.PackagingType = "HH";

            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();
            var TotalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(allResults, materialDetails, testCalculatorRunId, scaledupProducers, partialObligations);

            // Act
            var result = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducersTotal(producers, TotalPackagingTonnage);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void CanCallGetPercentageofProducerReportedHHTonnagevsAllProducers()
        {
            // Arrange
            var testProducerId = TestFixtures.Legacy.Create<int>();
            var testCalculatorRunId = TestFixtures.Legacy.Create<int>();
            var testSubsidaryId = TestFixtures.Legacy.Create<string>();
            var materialDetails = TestFixtures.Legacy.Create<ImmutableArray<MaterialDto>>();

            //CalcResultSummaryBuilder.ScaledupProducers = Fixture.Create<List<CalcResultScaledupProducer>>();

            var producer = TestFixtures.Legacy.Create<ProducerDetail>();
            var allResults = GenerateAllResults(testProducerId, testCalculatorRunId, testSubsidaryId);

            producer.ProducerId = testProducerId;
            producer.SubsidiaryId = testSubsidaryId;
            producer.CalculatorRunId = testCalculatorRunId;
            allResults.First().ProducerReportedMaterial.MaterialId = materialDetails.First().Id;

            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();
            var TotalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(allResults, materialDetails, testCalculatorRunId, scaledupProducers, partialObligations);

            // Act
            var result = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(
                producer,
                TotalPackagingTonnage);

            // Assert
            Assert.AreEqual(50, result);
        }

        [TestMethod]
        public void GetPercentageofProducerReportedTonnagevsAllProducersTotal_ReturnsZero_WhenNoMatchingProducer()
        {
            // Arrange
            var producers = TestFixtures.Legacy.Create<List<ProducerDetail>>();
            var allResults = TestFixtures.Legacy.Create<List<CalcResultProducerAndReportMaterialDetail>>();
            var materialDetails = TestFixtures.Legacy.Create<ImmutableArray<MaterialDto>>();

            var testProducerId = TestFixtures.Legacy.Create<int>();
            var testCalculatorRunId = TestFixtures.Legacy.Create<int>();

            allResults.First().ProducerReportedMaterial.ProducerDetailId = testProducerId;
            allResults.First().ProducerDetail.Id = testProducerId;
            allResults.First().ProducerDetail.ProducerId = testProducerId;
            allResults.First().ProducerDetail.CalculatorRunId = testCalculatorRunId;
            allResults.First().ProducerReportedMaterial.PackagingType = "PB";

            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();
            var totalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(allResults, materialDetails, testCalculatorRunId, scaledupProducers, partialObligations);

            // Act
            var result = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducersTotal(producers, totalPackagingTonnage);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void GetPercentageofProducerReportedTonnagevsAllProducersTotal_ReturnsValue_WhenMatchingProducer()
        {
            // Arrange
            var testProducerId = TestFixtures.Legacy.Create<int>();
            var testCalculatorRunId = TestFixtures.Legacy.Create<int>();
            var testSubsidaryId = TestFixtures.Legacy.Create<string>();
            var materialDetails = TestFixtures.Legacy.Create<ImmutableArray<MaterialDto>>();

            var producer = TestFixtures.Legacy.Create<ProducerDetail>();
            var allResults = GenerateAllResults(testProducerId, testCalculatorRunId, testSubsidaryId);

            allResults.First().ProducerReportedMaterial.MaterialId =  materialDetails.First().Id;
            allResults.First().ProducerReportedMaterial.PackagingType = "PB";

            producer.ProducerId = testProducerId;
            producer.SubsidiaryId = testSubsidaryId;
            producer.CalculatorRunId = testCalculatorRunId;

            //CalcResultSummaryBuilder.ScaledupProducers = Fixture.Create<List<CalcResultScaledupProducer>>();

            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();
            var totalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(allResults, materialDetails, testCalculatorRunId, scaledupProducers, partialObligations);

            // Act
            var result = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(
              producer,
              totalPackagingTonnage);

            // Assert
            Assert.AreEqual(50, result);
        }

        private List<CalcResultProducerAndReportMaterialDetail> GenerateAllResults(
                int testProducerId,
                int testCalculatorRunId,
                string testSubsidaryId)
        {
            var allResults = TestFixtures.Legacy.Create<List<CalcResultProducerAndReportMaterialDetail>>();
            allResults.First().ProducerReportedMaterial.ProducerDetailId = testProducerId;
            allResults.First().ProducerDetail.Id = testProducerId;
            allResults.First().ProducerDetail.ProducerId = testProducerId;
            allResults.First().ProducerDetail.CalculatorRunId = testCalculatorRunId;
            allResults.First().ProducerDetail.SubsidiaryId = testSubsidaryId;
            allResults.First().ProducerReportedMaterial.PackagingType = "HH";

            allResults.Last().ProducerReportedMaterial.ProducerDetailId = testProducerId;
            allResults.Last().ProducerDetail.Id = testProducerId;
            allResults.Last().ProducerDetail.ProducerId = testProducerId;
            allResults.Last().ProducerDetail.CalculatorRunId = testCalculatorRunId;
            allResults.Last().ProducerDetail.SubsidiaryId = TestFixtures.Legacy.Create<string>();
            allResults.Last().ProducerReportedMaterial.PackagingType = "HH";

            return allResults;
        }
    }
}
