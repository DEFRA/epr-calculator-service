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
                CalcResultModulation = null,
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
        public void GetNetReportedTonnageCanBeNegative()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "GL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetNetReportedTonnageCanBeNegative(producer, material, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(-40m, result);
        }

        [TestMethod]
        public void CanGetNetReportedTonnageForNegativeTonnagesReturnZero()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().Where(p => p.Id == 1).Take(1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "GL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetNetReportedTonnage(producer, material, scaledupProducers, partialObligations, showModulations: false);

            // Assert
            Assert.AreEqual((total: 0, red: null, amber: null, green: null), result);
        }

        [TestMethod]
        public void CanGetNetReportedTonnage()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().Where(p => p.Id == 1).Take(1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result1 = CalcResultSummaryUtil.GetNetReportedTonnage(producer, material, scaledupProducers, partialObligations, showModulations: false, CommonConstants.LevelTwo);
            var result2 = CalcResultSummaryUtil.GetNetReportedTonnage(producer, material, scaledupProducers, partialObligations, showModulations: true, CommonConstants.LevelTwo);

            // Assert
            Assert.AreEqual((total: 980.00m, red: null, amber: null, green: null), result1);
            Assert.AreEqual((total: null, red: null, amber: null, green: null), result2);
        }

        [TestMethod]
        public void CanGetNetReportedTonnageOverallTotal()
        {
            // Arrange
            var producerDisposalFees = TestDataHelper.GetProducerDisposalFees(showModulations: false);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetNetReportedTonnageOverallTotal(producerDisposalFees, material, showModulations: false);

            // Assert
            Assert.AreEqual((total: 910, red: null, amber: null, green: null), result);
        }

        [TestMethod]
        public void CanGetNetReportedTonnageOverallTotal_WithModulations()
        {
            // Arrange
            var producerDisposalFees = TestDataHelper.GetProducerDisposalFees(showModulations: true);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetNetReportedTonnageOverallTotal(producerDisposalFees, material, showModulations: true);

            // Assert
            Assert.AreEqual((total: 910, red: 300, amber: 200, green: 410), result);
        }

        public static IEnumerable<object[]> NetReportedTonnageCases => new List<object[]>
        {
            //             hh        , red     , redM, amber     , amberM, green   , greenM, cw  ,                           expected tuple   (total     , red.    , amber     , green)       // ECV-430
            new object[] { 942.362m  , 464.266m, 0m  , 278.096m  , 0m    , 200m    , 0m    , 100m, ((decimal?, decimal?, decimal?, decimal?)) (842.362m  , 464.266m, 178.096m  , 200m    ) }, // AC1
            new object[] { 27522.359m, 11000m  , 0m  , 15899.754m, 0m    , 622.610m, 0m    , 500m, ((decimal?, decimal?, decimal?, decimal?)) (27022.359m, 11000m  , 15399.754m, 622.610m) }, // AC2
            new object[] { 3287.503m , 2190.39m, 0m  , 300m      , 0m    , 797.113m, 0m    , 500m, ((decimal?, decimal?, decimal?, decimal?)) (2787.503m , 1990.39m, 0m        , 797.113m) }, // AC3
            new object[] { 220m      , 25m     , 0m  , 50m       , 0m    , 145m    , 0m    , 100m, ((decimal?, decimal?, decimal?, decimal?)) (120m      , 0m      , 0m        , 120m    ) }, // AC4
            new object[] { 0m        , 0m      , 0m  , 0m        , 0m    , 0m      , 0m    , 100m, ((decimal?, decimal?, decimal?, decimal?)) (0m        , 0m      , 0m        , 0m      ) }, // AC5
            new object[] { 300m      , 100m    , 0m  , 100m      , 0m    , 100m    , 0m    , 50m , ((decimal?, decimal?, decimal?, decimal?)) (250m      , 100m    , 50m       , 100m    ) },
            new object[] { 300m      , 100m    , 0m  , 100m      , 0m    , 100m    , 0m    , 100m, ((decimal?, decimal?, decimal?, decimal?)) (200m      , 100m    , 0m        , 100m    ) },
            new object[] { 300m      , 0m      , 100m, 0m        , 100m  , 0m      , 100m  , 150m, ((decimal?, decimal?, decimal?, decimal?)) (150m      , 50m     , 0m        , 100m    ) }, // RAG Medical
            new object[] { 300m      , 50m     , 50m , 50m       , 50m   , 50m     , 50m   , 150m, ((decimal?, decimal?, decimal?, decimal?)) (150m      , 50m     , 0m        , 100m    ) }, // RAG + RAG Medical
            new object[] { 300m      , 100m    , 0m  , 100m      , 0m    , 100m    , 0m    , 200m, ((decimal?, decimal?, decimal?, decimal?)) (100m      , 0m      , 0m        , 100m    ) },
            new object[] { 300m      , 100m    , 0m  , 100m      , 0m    , 100m    , 0m    , 250m, ((decimal?, decimal?, decimal?, decimal?)) (50m       , 0m      , 0m        , 50m     ) },
            new object[] { 300m      , 100m    , 0m  , 100m      , 0m    , 100m    , 0m    , 300m, ((decimal?, decimal?, decimal?, decimal?)) (0m        , 0m      , 0m        , 0m      ) },
            new object[] { 300m      , 100m    , 0m  , 100m      , 0m    , 100m    , 0m    , 350m, ((decimal?, decimal?, decimal?, decimal?)) (0m        , 0m      , 0m        , 0m      ) },
        };

        [DataTestMethod]
        [DynamicData(nameof(NetReportedTonnageCases), DynamicDataSourceType.Property)]
        public void CanGetNetReportedTonnage_WithModulations(
            decimal hhTotal,
            decimal red,
            decimal redMedical,
            decimal amber,
            decimal amberMedical,
            decimal green,
            decimal greenMedical,
            decimal cw,
            (decimal? total, decimal? red, decimal? amber, decimal? green) expected)
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            producer.ProducerReportedMaterials.Clear();

            producer.ProducerReportedMaterials.Add(new ProducerReportedMaterial
            {
                Material = new Material { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                PackagingTonnage = hhTotal,
                PackagingType = "HH",
                SubmissionPeriod = "2025-H1",
                PackagingTonnageRed = red,
                PackagingTonnageRedMedical = redMedical,
                PackagingTonnageAmber = amber,
                PackagingTonnageAmberMedical = amberMedical,
                PackagingTonnageGreen = green,
                PackagingTonnageGreenMedical = greenMedical,
                MaterialId = material.Id
            });

            producer.ProducerReportedMaterials.Add(new ProducerReportedMaterial
            {
                Material = new Material { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                PackagingTonnage = cw,
                PackagingType = "CW",
                SubmissionPeriod = "2025-H1",
                MaterialId = material.Id
            });

            // Act
            var result = CalcResultSummaryUtil.GetNetReportedTonnage(
                [producer], material, scaledupProducers, partialObligations, showModulations: true);

            // Assert (with clear messages)
            Assert.AreEqual(expected.total, result.total, "Total mismatch");
            Assert.AreEqual(expected.red  , result.red  , "Red mismatch");
            Assert.AreEqual(expected.amber, result.amber, "Amber mismatch");
            Assert.AreEqual(expected.green, result.green, "Green mismatch");
        }


        [TestMethod]
        public void CanGetActionedSelfManagedConsumerWasteTonnage()
        {
            Assert.AreEqual(50  , CalcResultSummaryUtil.GetActionedSelfManagedConsumerWasteTonnage(totalReportedTonnage: 100 ,selfManagedConsumerWasteTonnage: 50 ));
            Assert.AreEqual(100 , CalcResultSummaryUtil.GetActionedSelfManagedConsumerWasteTonnage(totalReportedTonnage: 100, selfManagedConsumerWasteTonnage: 100));
            Assert.AreEqual(100 , CalcResultSummaryUtil.GetActionedSelfManagedConsumerWasteTonnage(totalReportedTonnage: 100, selfManagedConsumerWasteTonnage: 150));
            Assert.AreEqual(0   , CalcResultSummaryUtil.GetActionedSelfManagedConsumerWasteTonnage(totalReportedTonnage: 0  , selfManagedConsumerWasteTonnage: 150));
            Assert.AreEqual(null, CalcResultSummaryUtil.GetActionedSelfManagedConsumerWasteTonnage(totalReportedTonnage: 100, selfManagedConsumerWasteTonnage: 100, level: 2));
        }

        [TestMethod]
        public void CanGetActionedSelfManagedConsumerWasteTonnageOverallTotal()
        {
            var producerDisposalFees = TestDataHelper.GetProducerDisposalFees();
            var materials            = TestDataHelper.GetMaterials();
            Assert.AreEqual(90 , CalcResultSummaryUtil.GetActionedSelfManagedConsumerWasteTonnageOverallTotal(producerDisposalFees, materials.First(m => m.Code == "AL")));
            Assert.AreEqual(140, CalcResultSummaryUtil.GetActionedSelfManagedConsumerWasteTonnageOverallTotal(producerDisposalFees, materials.First(m => m.Code == "FC")));
            Assert.AreEqual(150, CalcResultSummaryUtil.GetActionedSelfManagedConsumerWasteTonnageOverallTotal(producerDisposalFees, materials.First(m => m.Code == "GL")));
            Assert.AreEqual(0  , CalcResultSummaryUtil.GetActionedSelfManagedConsumerWasteTonnageOverallTotal(producerDisposalFees, materials.First(m => m.Code == "ST")));
        }

        [TestMethod]
        public void CanGetPricePerTonne_NonMatchingMaterial()
        {
            // Arrange
            var material = Fixture.Create<MaterialDetail>();

            // Act
            var result = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult);

            // Assert
            Assert.AreEqual(0m, result);
        }

        [TestMethod]
        public void CanGetProducerDisposalFee()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var producerAndSubsidiaries = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetProducerDisposalFee(producer, producerAndSubsidiaries, material, calcResult, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(654.248000m, result);
        }

        [TestMethod]
        public void CanGetProducerDisposalFeeProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetProducerDisposalFeeProducerTotal(producers, material, calcResult, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(1962.744000m, result);
        }

        [TestMethod]
        public void CanGetProducerDisposalFeeOverallTotal()
        {
            // Arrange
            var producerDisposalFees = TestDataHelper.GetProducerDisposalFees(showModulations: false);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetProducerDisposalFeeOverallTotal(producerDisposalFees, material);

            // Assert
            Assert.AreEqual(607.52m, result);
        }

        [TestMethod]
        public void CanGetProducerDisposalFeeProducerTotalForNegativeTonnage()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "GL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetProducerDisposalFeeProducerTotal(producers, material, calcResult, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(CommonConstants.DefaultMinValue, result);
        }

        [TestMethod]
        public void CanGetBadDebtProvision()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var producerAndSubsidiaries = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetBadDebtProvision(producer, producerAndSubsidiaries, material, calcResult, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(39.254880m, result);
        }

        [TestMethod]
        public void CanGetBadDebtProvisionReturnsZeroForInvalidValue()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var producer = producers.First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "GL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();
            var calculatorResult = calcResult;
            calculatorResult.CalcResultParameterOtherCost.BadDebtProvision = new KeyValuePair<string, string>("6 Bad Debt Provision", "some value");

            // Act
            var result = CalcResultSummaryUtil.GetBadDebtProvision(producer, producers, material, calcResult, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(CommonConstants.DefaultMinValue, result);
        }

        [TestMethod]
        public void CanGetBadDebtProvisionProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetBadDebtProvisionProducerTotal(producers, material, calcResult, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(117.76464000m, result);
        }

        [TestMethod]
        public void CanGetBadDebtProvisionOverallTotal()
        {
            // Arrange
            var producerDisposalFees = TestDataHelper.GetProducerDisposalFees(showModulations: false);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetBadDebtProvisionOverallTotal(producerDisposalFees, material);

            // Assert
            Assert.AreEqual(36.45m, result);
        }

        [TestMethod]
        public void CanGetBadDebtProvisionProducerTotalReturnsZeroForInvalidValue()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();
            var calculatorResult = calcResult;
            calculatorResult.CalcResultParameterOtherCost.BadDebtProvision = new KeyValuePair<string, string>("6 Bad Debt Provision", "some value");

            // Act
            var result = CalcResultSummaryUtil.GetBadDebtProvisionProducerTotal(producers, material, calcResult, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(CommonConstants.DefaultMinValue, result);
        }

        [TestMethod]
        public void CanGetProducerDisposalFeeWithBadDebtProvision()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var producerAndSubsidiaries = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvision(producer, producerAndSubsidiaries, material, calcResult, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(693.50288000m, result);
        }

        [TestMethod]
        public void CanGetProducerDisposalFeeWithBadDebtProvisionReturnsZeroForInvalid()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var producer = producers.First(p => p.Id == 1);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();
            var calculatorResult = calcResult;
            calculatorResult.CalcResultParameterOtherCost.BadDebtProvision = new KeyValuePair<string, string>("6 Bad Debt Provision", "some value");


            // Act
            var result = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvision(producer, producers, material, calcResult, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(CommonConstants.DefaultMinValue, result);
        }

        [TestMethod]
        public void CanGetProducerDisposalFeeWithBadDebtProvisionProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvisionProducerTotal(producers, material, calcResult, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(2080.50864000m, result);
        }

        [TestMethod]
        public void CanGetProducerDisposalFeeWithBadDebtProvisionProducerTotalZero()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            calcResult.CalcResultParameterOtherCost.BadDebtProvision = new KeyValuePair<string, string>("6 Bad Debt Provision", "-%");
            // Act
            var result = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvisionProducerTotal(producers, material, calcResult, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void CanGetProducerDisposalFeeWithBadDebtProvisionOverallTotal()
        {
            // Arrange
            var producerDisposalFees = TestDataHelper.GetProducerDisposalFees(showModulations: false);
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");

            // Act
            var result = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvisionOverallTotal(producerDisposalFees, material);

            // Assert
            Assert.AreEqual(643.97m, result);
        }

        [TestMethod]
        public void CanGetEnglandWithBadDebtProvision()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var producerAndSubsidiaries = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetCountryBadDebtProvision(producer, producerAndSubsidiaries, material, calcResult, Countries.England, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(374.8295162135948480m, result);
        }

        [TestMethod]
        public void CanGetDefaultBadDebtProvision()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var producerAndSubsidiaries = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetCountryBadDebtProvision(producer, producerAndSubsidiaries, material, calcResult, (Countries)(-1), scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void CanGetWalesWithBadDebtProvision()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var producerAndSubsidiaries = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetCountryBadDebtProvision(producer, producerAndSubsidiaries, material, calcResult, Countries.Wales, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(84.4902597789384960m, result);
        }

        [TestMethod]
        public void CanGetScotlandWithBadDebtProvision()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var producerAndSubsidiaries = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetCountryBadDebtProvision(producer, producerAndSubsidiaries, material, calcResult, Countries.Scotland, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(168.2977700641839840m, result);
        }

        [TestMethod]
        public void CanGetNorthernIrelandWithBadDebtProvision()
        {
            // Arrange
            var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
            var producerAndSubsidiaries = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetCountryBadDebtProvision(producer, producerAndSubsidiaries, material, calcResult, Countries.NorthernIreland, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(65.8853339432826720m, result);
        }

        [TestMethod]
        public void CanGetEnglandWithBadDebtProvisionProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetCountryBadDebtProvisionTotal(producers, material, calcResult, Countries.England, scaledupProducers, partialObligations);
            var total = CalcResultSummaryUtil.GetCountryBadDebtProvisionOverallTotal(TestDataHelper.GetProducerDisposalFees(showModulations: false), material, Countries.England);

            // Assert
            Assert.AreEqual(1124.4885486407845440m, result);
            Assert.AreEqual(348.06m, total);
        }

        [TestMethod]
        public void CanGetWalesWithBadDebtProvisionProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetCountryBadDebtProvisionTotal(producers, material, calcResult, Countries.Wales, scaledupProducers, partialObligations);
            var total = CalcResultSummaryUtil.GetCountryBadDebtProvisionOverallTotal(TestDataHelper.GetProducerDisposalFees(showModulations: false), material, Countries.Wales);

            // Assert
            Assert.AreEqual(253.4707793368154880m, result);
            Assert.AreEqual(78.46m, total);

        }

        [TestMethod]
        public void CanGetScotlandWithBadDebtProvisionProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetCountryBadDebtProvisionTotal(producers, material, calcResult, Countries.Scotland, scaledupProducers, partialObligations);
            var total = CalcResultSummaryUtil.GetCountryBadDebtProvisionOverallTotal(TestDataHelper.GetProducerDisposalFees(showModulations: false), material, Countries.Scotland);

            // Assert
            Assert.AreEqual(504.8933101925519520m, result);
            Assert.AreEqual(156.28m, total);
        }

        [TestMethod]
        public void CanGetNorthernIrelandWithBadDebtProvisionProducerTotal()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var material = TestDataHelper.GetMaterials().First(m => m.Code == "AL");
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetCountryBadDebtProvisionTotal(producers, material, calcResult, Countries.NorthernIreland, scaledupProducers, partialObligations);
            var total = CalcResultSummaryUtil.GetCountryBadDebtProvisionOverallTotal(TestDataHelper.GetProducerDisposalFees(showModulations: false), material, Countries.NorthernIreland);

            // Assert
            Assert.AreEqual(197.6560018298480160m, result);
            Assert.AreEqual(61.18m, total);
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
        public void CanGetTotal1Plus2ABadDebt()
        {
            // Arrange
            var producers = TestDataHelper.GetProducers();
            var materials = TestDataHelper.GetMaterials();
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            var partialObligations = new List<CalcResultPartialObligation>();

            // Act
            var result = CalcResultSummaryUtil.GetTotal1Plus2ABadDebt(producers, materials, calcResult, scaledupProducers, partialObligations);

            // Assert
            Assert.AreEqual(2217.89100000m, result);
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