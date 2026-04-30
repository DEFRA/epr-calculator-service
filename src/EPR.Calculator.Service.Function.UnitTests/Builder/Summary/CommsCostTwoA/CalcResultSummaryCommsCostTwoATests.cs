using AutoFixture;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoA;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.CommsCostTwoA
{
    [TestClass]
  public class CalcResultSummaryCommsCostTwoATests
  {
        private readonly List<ProducerDetail> producers;
        private readonly MaterialDetail material;
        private readonly CalcResult calcResult;
        private readonly IEnumerable<CalcResultScaledupProducer> scaledupProducers;
        private readonly IEnumerable<CalcResultPartialObligation> partialObligations;

        private Fixture Fixture { get; init; } = new Fixture();

        public CalcResultSummaryCommsCostTwoATests()
        {
            material = GetMaterial();
            producers = GetProducers();
            scaledupProducers = new List<CalcResultScaledupProducer>();
            partialObligations = new List<CalcResultPartialObligation>();

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
                CalcResultProjectedProducers = new CalcResultProjectedProducers()
            };
        }

        public static List<Dictionary<string, CalcResultScaledupProducerTonnage>> GetScaledUpProducers()
        {
            return new List<Dictionary<string, CalcResultScaledupProducerTonnage>>
            {
                new Dictionary<string, CalcResultScaledupProducerTonnage>
                {
                    {
                        "10001",
                        new CalcResultScaledupProducerTonnage
                        {
                            ReportedHouseholdPackagingWasteTonnage = 0,
                            ReportedPublicBinTonnage = 0,
                            TotalReportedTonnage = 0,
                            ReportedSelfManagedConsumerWasteTonnage = 0,
                            NetReportedTonnage = 0,
                            ScaledupReportedHouseholdPackagingWasteTonnage = 0,
                            ScaledupReportedPublicBinTonnage = 0,
                            ScaledupTotalReportedTonnage = 0,
                            ScaledupReportedSelfManagedConsumerWasteTonnage = 0,
                            ScaledupNetReportedTonnage = 0,
                        }
                    },
                },
            };
        }

        [TestMethod]
        public void GetEnglandWithBadDebtProvisionForCommsTotal_WhenNoProducers_ShouldReturn0()
        {
            // Arrange
            producers.Clear();
            decimal expectedCost1 = 0m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForCommsTotal(producers, material, calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetEnglandWithBadDebtProvisionForCommsTotal_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 1139.71200m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForCommsTotal(producers, material, calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetWalesWithBadDebtProvisionForCommsTotal_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 284.92800m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForCommsTotal(producers, material, calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetScotlandWithBadDebtProvisionForCommsTotal_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 427.39200m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForCommsTotal(producers, material, calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetNorthernIrelandWithBadDebtProvisionForCommsTotal_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 997.24800m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForCommsTotal(producers, material, calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetProducerTotalCostWithoutBadDebtProvisionTotal_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 1344.00m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvisionTotal(producers, material, calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetBadDebtProvisionForCommsCostTotal_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 80.64m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetBadDebtProvisionForCommsCostTotal(producers, material, calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetEnglandWithBadDebtProvisionForComms_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 427.392m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForComms(producers[0], material, calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetWalesWithBadDebtProvisionForComms_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 106.848m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForComms(producers[0], material, calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetScotlandWithBadDebtProvisionForComms_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 160.272m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForComms(producers[0], material, calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetNorthernIrelandWithBadDebtProvisionForComms_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 373.96800m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForComms(producers[0], material, calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetPriceperTonneForComms_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 0.42m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetPriceperTonneForComms_WhenNoMaterialMatch_ShouldReturn0()
        {
            // Arrange
            material.Name = "Aluminium";
            decimal expectedCost1 = 0m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetPriceperTonneForComms_WhenCostIsNotDecimal_ShouldReturn0()
        {
            // Arrange
            calcResult.CalcResultCommsCostReportDetail.CalcResultCommsCostCommsCostByMaterial = [
                        new ()
                        {
                            CommsCostByMaterialPricePerTonne="0.42A",
                            Name = "Household",
                        }
                          ];
            decimal expectedCost1 = 0m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetProducerTotalCostwithBadDebtProvision_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 534.2400m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvision(producers[0], material, calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetProducerTotalCostWithoutBadDebtProvision_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 504.00m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvision(producers[0], material, calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetProducerTotalCostwithBadDebtProvisionTotal_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 1424.6400m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvisionTotal(producers, material, calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetTotalReportedTonnageTotalForHDCShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 280;
            var material = GetHDCMaterial();

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetTotalReportedTonnageTotal(producers, material);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetTotalReportedTonnageTotalShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 3200;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetTotalReportedTonnageTotal(producers, material);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        private static List<ProducerDetail> GetProducers()
        {
            var producers = TestDataHelper.GetProducers();

            foreach (var subPeriod in new[] {"2025-H1", "2025-H2"})
            {
                producers[0].ProducerReportedMaterials.Add(new ProducerReportedMaterial
                {
                    MaterialId = 1,
                    ProducerDetailId = 1,
                    PackagingType = "HH",
                    PackagingTonnage = 50,
                    SubmissionPeriod = subPeriod,
                    Material = new Material
                    {
                        Id = 1,
                        Code = "HH",
                        Name = "Material1",
                        Description = "Material1",
                    },
                });

                producers[0].ProducerReportedMaterials.Add(new ProducerReportedMaterial
                {
                    MaterialId = 2,
                    ProducerDetailId = 2,
                    PackagingType = "HDC",
                    PackagingTonnage = 50,
                    SubmissionPeriod = subPeriod,
                    Material = new Material
                    {
                        Id = 2,
                        Code = "GL",
                        Name = "Material2",
                        Description = "Material2",
                    },
                });

                producers[0].ProducerReportedMaterials.Add(new ProducerReportedMaterial
                {
                    MaterialId = 3,
                    ProducerDetailId = 3,
                    PackagingType = "PB",
                    PackagingTonnage = 100,
                    SubmissionPeriod = subPeriod,
                    Material = new Material
                    {
                        Id = 1,
                        Code = "AL",
                        Name = "Material1",
                        Description = "Material1",
                    },
                });
            }

            return producers;
        }

        private static MaterialDetail GetMaterial()
        {
            var material = new MaterialDetail
            {
                Id = 1,
                Code = "AL",
                Name = "Material1",
                Description = "Material1",
            };
            return material;
        }

        private static MaterialDetail GetHDCMaterial()
        {
            var material = new MaterialDetail
            {
                Id = 2,
                Code = "GL",
                Name = "Material2",
                Description = "Material2",
            };
            return material;
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
