using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Builder.Summary.TonnageVsAllProducer;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Data;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.TonnageVsAllProducer
{
    [TestClass]
    public class TonnageVsAllProducerUtilTests
    {
        private IFixture _fixture = null;

        [TestInitialize]
        public void Init()
        {
            _fixture = TestFixtures.New();
        }

        [TestMethod]
        public void CanCallGetPercentageofProducerReportedHHTonnagevsAllProducersTotal()
        {
            // Arrange
            var producers = _fixture.Create<List<ProducerDetail>>();

            var testProducerId = _fixture.Create<int>();
            var testCalculatorRunId = _fixture.Create<int>();
            var testSubsidaryId = _fixture.Create<string>();
            var materials = DummyData.Materials;
            var allResults = GenerateAllResults(testProducerId, testCalculatorRunId, testSubsidaryId);

            producers.First().ProducerId = testProducerId;
            producers.First().SubsidiaryId = testSubsidaryId;
            producers.First().CalculatorRunId = testCalculatorRunId;
            allResults.First().ProducerReportedMaterialProjected.MaterialId = materials.First().Id;

            var scaledupProducers = _fixture.Create<List<CalcResultScaledupProducer>>();
            var partialObligations = new List<CalcResultPartialObligation>();

            var TotalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(allResults, materials, producers.First().CalculatorRunId);
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
            var producers = _fixture.Create<List<ProducerDetail>>();
            var allResults = _fixture.Create<List<CalcResultProducerAndReportMaterialDetail>>();
            var materialDetails = _fixture.Create<List<MaterialDetail>>();

            var testProducerId = _fixture.Create<int>();
            var testCalculatorRunId = _fixture.Create<int>();

            allResults.First().ProducerReportedMaterialProjected.ProducerDetailId = testProducerId;
            allResults.First().ProducerDetail.Id = testProducerId;
            allResults.First().ProducerDetail.ProducerId = testProducerId;
            allResults.First().ProducerDetail.CalculatorRunId = testCalculatorRunId;
            allResults.First().ProducerReportedMaterialProjected.PackagingType = "HH";

            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();
            var TotalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(allResults, materialDetails, testCalculatorRunId);

            // Act
            var result = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducersTotal(producers, TotalPackagingTonnage);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void CanCallGetPercentageofProducerReportedHHTonnagevsAllProducers()
        {
            // Arrange
            var testProducerId = _fixture.Create<int>();
            var testCalculatorRunId = _fixture.Create<int>();
            var testSubsidaryId = _fixture.Create<string>();
            var materials = DummyData.Materials;

            //CalcResultSummaryBuilder.ScaledupProducers = Fixture.Create<List<CalcResultScaledupProducer>>();

            var producer = _fixture.Create<ProducerDetail>();
            var allResults = GenerateAllResults(testProducerId, testCalculatorRunId, testSubsidaryId);


            producer.ProducerId = testProducerId;
            producer.SubsidiaryId = testSubsidaryId;
            producer.CalculatorRunId = testCalculatorRunId;
            allResults.First().ProducerReportedMaterialProjected.MaterialId = materials.First().Id;

            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();
            var TotalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(allResults, materials, testCalculatorRunId);

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
            var producers = _fixture.Create<List<ProducerDetail>>();
            var allResults = _fixture.Create<List<CalcResultProducerAndReportMaterialDetail>>();
            var materialDetails = _fixture.Create<List<MaterialDetail>>();

            var testProducerId = _fixture.Create<int>();
            var testCalculatorRunId = _fixture.Create<int>();

            allResults.First().ProducerReportedMaterialProjected.ProducerDetailId = testProducerId;
            allResults.First().ProducerDetail.Id = testProducerId;
            allResults.First().ProducerDetail.ProducerId = testProducerId;
            allResults.First().ProducerDetail.CalculatorRunId = testCalculatorRunId;
            allResults.First().ProducerReportedMaterialProjected.PackagingType = "PB";

            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();
            var totalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(allResults, materialDetails, testCalculatorRunId);

            // Act
            var result = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducersTotal(producers, totalPackagingTonnage);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void GetPercentageofProducerReportedTonnagevsAllProducersTotal_ReturnsValue_WhenMatchingProducer()
        {
            // Arrange
            var testProducerId = _fixture.Create<int>();
            var testCalculatorRunId = _fixture.Create<int>();
            var testMaterialId = _fixture.Create<int>();
            var testSubsidaryId = _fixture.Create<string>();
            var materials = DummyData.Materials;

            var producer = _fixture.Create<ProducerDetail>();
            var allResults = GenerateAllResults(testProducerId, testCalculatorRunId, testSubsidaryId);

            allResults.First().ProducerReportedMaterialProjected.MaterialId = materials.First().Id;
            allResults.First().ProducerReportedMaterialProjected.PackagingType = "PB";

            producer.ProducerId = testProducerId;
            producer.SubsidiaryId = testSubsidaryId;
            producer.CalculatorRunId = testCalculatorRunId;

            //CalcResultSummaryBuilder.ScaledupProducers = Fixture.Create<List<CalcResultScaledupProducer>>();

            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();
            var totalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(allResults, materials, testCalculatorRunId);

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
            var allResults = _fixture.Create<List<CalcResultProducerAndReportMaterialDetail>>();
            allResults.First().ProducerReportedMaterialProjected.ProducerDetailId = testProducerId;
            allResults.First().ProducerDetail.Id = testProducerId;
            allResults.First().ProducerDetail.ProducerId = testProducerId;
            allResults.First().ProducerDetail.CalculatorRunId = testCalculatorRunId;
            allResults.First().ProducerDetail.SubsidiaryId = testSubsidaryId;
            allResults.First().ProducerReportedMaterialProjected.PackagingType = "HH";

            allResults.Last().ProducerReportedMaterialProjected.ProducerDetailId = testProducerId;
            allResults.Last().ProducerDetail.Id = testProducerId;
            allResults.Last().ProducerDetail.ProducerId = testProducerId;
            allResults.Last().ProducerDetail.CalculatorRunId = testCalculatorRunId;
            allResults.Last().ProducerDetail.SubsidiaryId = _fixture.Create<string>();
            allResults.Last().ProducerReportedMaterialProjected.PackagingType = "HH";

            return allResults;
        }
    }
}
