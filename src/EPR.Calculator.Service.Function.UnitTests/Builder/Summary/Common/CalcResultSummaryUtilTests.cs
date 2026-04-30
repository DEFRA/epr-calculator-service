using AutoFixture;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.Common
{
    [TestClass]
    public class CalcResultSummaryUtilTests
    {
        private readonly CalcResult calcResult;

        private Fixture Fixture { get; init; } = new Fixture();

        public CalcResultSummaryUtilTests()
        {
            calcResult = new CalcResult
            {
                ShowModulations = false,
                CalcResultScaledupProducers = new CalcResultScaledupProducers(),
                CalcResultPartialObligations = new CalcResultPartialObligations(),
                CalcResultParameterOtherCost = TestDataHelper.GetCalcResultParameterOtherCost(),
                CalcResultDetail = TestDataHelper.GetCalcResultDetail(),
                CalcResultLaDisposalCostData = TestDataHelper.GetCalcResultLaDisposalCostData(),
                CalcResultLapcapData = TestDataHelper.GetCalcResultLapcapData(),
                CalcResultOnePlusFourApportionment = TestDataHelper.GetCalcResultOnePlusFourApportionment(),
                CalcResultParameterCommunicationCost = GetCalcResultParameterCommunicationCost(),
                CalcResultSummary = TestDataHelper.GetCalcResultSummary(),
                CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
                CalcResultLateReportingTonnageData = this.GetCalcResultLateReportingTonnage(),
                CalcResultProjectedProducers = new CalcResultProjectedProducers(),
            };
        }

        [TestMethod]
        public void CanGetNonTotalRowLevelIndex()
        {
            // Arrange
            var producerDisposalFeesLookup = TestDataHelper.GetProducerDisposalFees();
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);

            // Act
            var result = CalcResultSummaryUtil.GetLevelIndex(producerDisposalFeesLookup, producer);

            // Assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void CanGetHouseholdPackagingWasteTonnage()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.Household, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(1000.00m, result);
        }

        [TestMethod]
        public void CanGetPublicBinTonnage()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "PL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.PublicBin, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(20.00m, result);
        }

        [TestMethod]
        public void CanGetHouseholdDrinksContainersTonnage()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "GL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.HouseholdDrinksContainers, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(20.00m, result);
        }

        [TestMethod]
        public void CanGetReportedTonnage()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetReportedTonnage(producer, material, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(1000.00m, result);
        }

        [TestMethod]
        public void CanGetHouseholdPackagingWasteTonnageProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTonnageTotal(producers, material, PackagingTypes.Household, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(3000.00m, result);
        }

        [TestMethod]
        public void CanGetPublicBinTonnageProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "PL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTonnageTotal(producers, material, PackagingTypes.PublicBin, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(60.00m, result);
        }

        [TestMethod]
        public void CanGetReportedTonnageProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetReportedTonnageTotal(producers, material, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(3000.00m, result);
        }

        [TestMethod]
        public void CanGetHouseholdDrinksContainersTonnageProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "GL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTonnageTotal(producers, material, PackagingTypes.HouseholdDrinksContainers, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(60.00m, result);
        }

        [TestMethod]
        public void CanGetTonnageForScaledupProducer()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = TestDataHelper.GetScaledupProducers().ScaledupProducers;
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.Household, scaledupProducers!, partialObligations);

            // Assert
            Assert.AreEqual(1000.00m, result);
        }

        [TestMethod]
        public void CanGetHouseholdTonnageForScaledupProducer()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            producer.SubsidiaryId = string.Empty;
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = TestDataHelper.GetScaledupProducers().ScaledupProducers;
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.Household, scaledupProducers!, partialObligations);

            // Assert
            Assert.AreEqual(200.00m, result);
        }

        [TestMethod]
        public void CanGetHouseholdTonnageForScaledUpPartialObligation()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = TestDataHelper.GetScaledupProducers().ScaledupProducers;
            var partialObligations = TestDataHelper.GetPartialObligations().PartialObligations;

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.Household, scaledupProducers!, partialObligations!);

            // Assert
            Assert.AreEqual(50.00m, result);
        }

         [TestMethod]
        public void CanGetHouseholdTonnageForPartialObligation()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = TestDataHelper.GetPartialObligations().PartialObligations;

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.Household, scaledupProducers, partialObligations!);

            // Assert
            Assert.AreEqual(50.00m, result);
        }

        [TestMethod]
        public void CanGetPublicBinTonnageForScaledupProducer()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            producer.SubsidiaryId = string.Empty;
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = TestDataHelper.GetScaledupProducers().ScaledupProducers;
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.PublicBin, scaledupProducers!, partialObligations);

            // Assert
            Assert.AreEqual(40, result);
        }

        [TestMethod]
        public void CanGetPublicBinTonnageForPartialObligation()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = TestDataHelper.GetPartialObligations().PartialObligations;

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.PublicBin, scaledupProducers, partialObligations!);

            // Assert
            Assert.AreEqual(10.00m, result);
        }

        [TestMethod]
        public void CanGetConsumerWasteTonnageForScaledupProducer()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            producer.SubsidiaryId = string.Empty;
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = TestDataHelper.GetScaledupProducers().ScaledupProducers;
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.ConsumerWaste, scaledupProducers!, partialObligations);

            // Assert
            Assert.AreEqual(120.00m, result);
        }

        [TestMethod]
        public void CanGetConsumerWasteTonnageForPartialObligation()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = TestDataHelper.GetPartialObligations().PartialObligations;

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.ConsumerWaste, scaledupProducers, partialObligations!);

            // Assert
            Assert.AreEqual(30.00m, result);
        }

        [TestMethod]
        public void CanGetHDCTonnageForScaledupProducer()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            producer.SubsidiaryId = string.Empty;
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "GL");
            var scaledupProducers = TestDataHelper.GetScaledupProducers().ScaledupProducers;
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.HouseholdDrinksContainers, scaledupProducers!, partialObligations);

            // Assert
            Assert.AreEqual(140m, result);
        }

        [TestMethod]
        public void CanGetHDCTonnageForPartialObligation()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "GL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = TestDataHelper.GetPartialObligations().PartialObligations;

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.HouseholdDrinksContainers, scaledupProducers, partialObligations!);

            // Assert
            Assert.AreEqual(35m, result);
        }

        [TestMethod]
        public void CanGetDefaultTonnageForScaledupProducer()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            producer.SubsidiaryId = string.Empty;
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "GL");
            var scaledupProducers = TestDataHelper.GetScaledupProducers().ScaledupProducers;
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(producer, material, "Default", scaledupProducers!, partialObligations);

            // Assert
            Assert.AreEqual(0.00m, result);
        }

        [TestMethod]
        public void CanGetDefaultTonnageForPartialObligation()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            producer.SubsidiaryId = string.Empty;
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "GL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = TestDataHelper.GetPartialObligations().PartialObligations;

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(producer, material, "Default", scaledupProducers, partialObligations!);

            // Assert
            Assert.AreEqual(0.00m, result);
        }


        [TestMethod]
        public void CanGetZeroTonnageForInvalidPackagingTypeForScaledupProducer()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "GL");
            var scaledupProducers = TestDataHelper.GetScaledupProducers().ScaledupProducers;
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(producer, material, "Invalid packaging type", scaledupProducers!, partialObligations);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void CanGetManagedConsumerWasteTonnage()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.ConsumerWaste, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(20.00m, result);
        }

        [TestMethod]
        public void CanGetManagedConsumerWasteTonnageProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTonnageTotal(producers, material, PackagingTypes.ConsumerWaste, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(60.00m, result);
        }

        [TestMethod]
        public void CanGetPricePerTonne_NonMatchingMaterial()
        {
            // Arrange
            var material = Fixture.Create<MaterialDetail>();

            // Act
            var result = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult);

            // Assert
            Assert.AreEqual((total: null, red: null, amber: null, green: null), result);
        }

        [TestMethod]
        public void CanGetCountryApportionmentPercentage()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCountryApportionmentPercentage(calcResult);

            // Assert
            Assert.AreEqual(54.04873246369677m, result?.EnglandCost);
            Assert.AreEqual(12.183115924193945m, result?.WalesCost);
            Assert.AreEqual(24.267782426778243m, result?.ScotlandCost);
            Assert.AreEqual(9.500369185331037m, result?.NorthernIrelandCost);
        }

        [TestMethod]
        public void CanGetCommsCostHeaderWithoutBadDebtFor2bTitle()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);

            // Assert
            Assert.AreEqual(2530, result);
        }

        [TestMethod]
        public void CanGetCommsCostHeaderBadDebtProvisionFor2bTitle()
        {
            calcResult.CalcResultSummary.CommsCostHeaderWithoutBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);

            // Act
            var result = CalcResultSummaryUtil.GetCommsCostHeaderBadDebtProvisionFor2bTitle(calcResult, calcResult.CalcResultSummary);

            // Assert
            Assert.AreEqual(151.80m, result);
        }

        [TestMethod]
        public void CanGetCommsCostHeaderWithBadDebtFor2bTitle()
        {
            calcResult.CalcResultSummary.CommsCostHeaderWithoutBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);
            calcResult.CalcResultSummary.CommsCostHeaderBadDebtProvisionFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderBadDebtProvisionFor2bTitle(calcResult, calcResult.CalcResultSummary);

            // Act
            var result = CalcResultSummaryUtil.GetCommsCostHeaderWithBadDebtFor2bTitle(calcResult.CalcResultSummary);

            // Assert
            Assert.AreEqual(2681.80m, result);
        }

        [TestMethod]
        public void CanGetCountryOnePlusFourApportionmentForEngland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(calcResult, Countries.England);

            // Assert
            Assert.AreEqual(14.53m, result);
        }

        [TestMethod]
        public void CanGetCountryOnePlusFourApportionmentForWales()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(calcResult, Countries.Wales);

            // Assert
            Assert.AreEqual(20, result);
        }

        [TestMethod]
        public void CanGetCountryOnePlusFourApportionmentForScotland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(calcResult, Countries.Scotland);

            // Assert
            Assert.AreEqual(0.15m, result);
        }

        [TestMethod]
        public void CanGetCountryOnePlusFourApportionmentForNorthernIreland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(calcResult, Countries.NorthernIreland);

            // Assert
            Assert.AreEqual(0.15m, result);
        }

        [TestMethod]
        public void CanGetDefaultOnePlusFourApportionment()
        {
            // Act
            var result = CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(calcResult, (Countries)(-1));

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void GetParamsOtherFourCountryApportionmentPercentageForEngland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetParamsOtherFourCountryApportionmentPercentage(calcResult, Countries.England);

            // Assert
            Assert.AreEqual(43.83561643835616m, result);
        }

        [TestMethod]
        public void GetParamsOtherFourCountryApportionmentPercentageForWales()
        {
            // Act
            var result = CalcResultSummaryUtil.GetParamsOtherFourCountryApportionmentPercentage(calcResult, Countries.Wales);

            // Assert
            Assert.AreEqual(19.17808219178082m, result);
        }

        [TestMethod]
        public void GetParamsOtherFourCountryApportionmentPercentageForScotland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetParamsOtherFourCountryApportionmentPercentage(calcResult, Countries.Scotland);

            // Assert
            Assert.AreEqual(24.65753424657534m, result);
        }

        [TestMethod]
        public void GetParamsOtherFourCountryApportionmentPercentageForNorthernIreland()
        {
            // Act
            var result = CalcResultSummaryUtil.GetParamsOtherFourCountryApportionmentPercentage(calcResult, Countries.NorthernIreland);

            // Assert
            Assert.AreEqual(12.32876712328767m, result);
        }

        [TestMethod]
        public void CanGetReportedPublicBinTonnage()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 2);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "PL");
            var scaledupProducers = TestDataHelper.GetScaledupProducers().ScaledupProducers;
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.PublicBin, scaledupProducers!, partialObligations);

            // Assert
            Assert.AreEqual(20.00m, result);
        }

        [TestMethod]
        public void CanGetReportedPublicBinTonnageTotal()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "PL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTonnageTotal(producer, material, PackagingTypes.PublicBin, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(60.00m, result);
        }

        [TestMethod]
        public void CanGetReportedHDCTonnage()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "GL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.HouseholdDrinksContainers, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(20.00m, result);
        }

        [TestMethod]
        public void CanGetReportedHDCTonnageTotal()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "GL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTonnageTotal(producer, material, PackagingTypes.HouseholdDrinksContainers, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(60.00m, result);
        }

        private CalcResultParameterCommunicationCost GetCalcResultParameterCommunicationCost()
        {
            return Fixture.Create<CalcResultParameterCommunicationCost>();
        }

        private CalcResultLateReportingTonnage GetCalcResultLateReportingTonnage()
        {
            return Fixture.Create<CalcResultLateReportingTonnage>();
        }
    }
}
