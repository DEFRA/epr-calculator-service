namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.CommsCostTwoA
{
    using AutoFixture;
    using EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoA;
    using EPR.Calculator.Service.Function.Data.DataModels;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

  [TestClass]
  public class CalcResultSummaryCommsCostTwoATests
  {
        private readonly List<ProducerDetail> producers;
        private readonly MaterialDetail material;
        private readonly CalcResult calcResult;
        private readonly IEnumerable<CalcResultScaledupProducer> scaledupProducers;

        private Fixture Fixture { get; init; } = new Fixture();

        public CalcResultSummaryCommsCostTwoATests()
        {
            this.material = GetMaterial();
            this.producers = GetProducers();
            this.scaledupProducers = new List<CalcResultScaledupProducer>();

            this.calcResult = new CalcResult
            {
                CalcResultParameterOtherCost = TestDataHelper.GetCalcResultParameterOtherCost(),
                CalcResultDetail = TestDataHelper.GetCalcResultDetail(),
                CalcResultLaDisposalCostData = TestDataHelper.GetCalcResultLaDisposalCostData(),
                CalcResultLapcapData = TestDataHelper.GetCalcResultLapcapData(),
                CalcResultOnePlusFourApportionment = TestDataHelper.GetCalcResultOnePlusFourApportionment(),
                CalcResultParameterCommunicationCost = this.GetCalcResultParameterCommunicationCost(),
                CalcResultSummary = TestDataHelper.GetCalcResultSummary(),
                CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
                CalcResultLateReportingTonnageData = this.GetCalcResultLateReportingTonnage(),
            };
        }

        [TestMethod]
        public void GetEnglandWithBadDebtProvisionForCommsTotal_WhenNoProducers_ShouldReturn0()
        {
            // Arrange
            this.producers.Clear();
            decimal expectedCost1 = 0m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForCommsTotal(this.producers, this.material, this.calcResult, this.scaledupProducers);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetEnglandWithBadDebtProvisionForCommsTotal_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 1139.71200m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForCommsTotal(this.producers, this.material, this.calcResult, this.scaledupProducers);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetWalesWithBadDebtProvisionForCommsTotal_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 284.92800m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForCommsTotal(this.producers, this.material, this.calcResult, this.scaledupProducers);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetScotlandWithBadDebtProvisionForCommsTotal_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 427.39200m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForCommsTotal(this.producers, this.material, this.calcResult, this.scaledupProducers);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetNorthernIrelandWithBadDebtProvisionForCommsTotal_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 997.24800m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForCommsTotal(this.producers, this.material, this.calcResult, this.scaledupProducers);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetProducerTotalCostWithoutBadDebtProvisionTotal_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 1344.00m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvisionTotal(this.producers, this.material, this.calcResult, this.scaledupProducers);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetBadDebtProvisionForCommsCostTotal_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 80.64m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetBadDebtProvisionForCommsCostTotal(this.producers, this.material, this.calcResult, this.scaledupProducers);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetEnglandWithBadDebtProvisionForComms_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 427.392m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForComms(this.producers[0], this.material, this.calcResult, this.scaledupProducers);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetWalesWithBadDebtProvisionForComms_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 106.848m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForComms(this.producers[0], this.material, this.calcResult, this.scaledupProducers);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetScotlandWithBadDebtProvisionForComms_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 160.272m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForComms(this.producers[0], this.material, this.calcResult, this.scaledupProducers);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetNorthernIrelandWithBadDebtProvisionForComms_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 373.96800m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForComms(this.producers[0], this.material, this.calcResult, this.scaledupProducers);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetPriceperTonneForComms_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 0.42m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(this.material, this.calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetPriceperTonneForComms_WhenNoMaterialMatch_ShouldReturn0()
        {
            // Arrange
            this.material.Name = "Aluminium";
            decimal expectedCost1 = 0m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(this.material, this.calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetPriceperTonneForComms_WhenCostIsNotDecimal_ShouldReturn0()
        {
            // Arrange
            this.calcResult.CalcResultCommsCostReportDetail.CalcResultCommsCostCommsCostByMaterial = [
                        new ()
                        {
                            CommsCostByMaterialPricePerTonne="0.42A",
                            Name = "Household",
                        }
                          ];
            decimal expectedCost1 = 0m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(this.material, this.calcResult);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetProducerTotalCostwithBadDebtProvision_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 534.2400m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvision(this.producers[0], this.material, this.calcResult, this.scaledupProducers);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetProducerTotalCostWithoutBadDebtProvision_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 504.00m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvision(this.producers[0], this.material, this.calcResult, this.scaledupProducers);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetProducerTotalCostwithBadDebtProvisionTotal_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 1424.6400m;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvisionTotal(this.producers, this.material, this.calcResult, this.scaledupProducers);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetTotalReportedTonnageTotalForHDCShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 150;
            var material = GetHDCMaterial();

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetTotalReportedTonnageTotal(this.producers, material, this.scaledupProducers);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
        }

        [TestMethod]
        public void GetTotalReportedTonnageTotalShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedCost1 = 3200;

            // Act
            decimal totalCost = CalcResultSummaryCommsCostTwoA.GetTotalReportedTonnageTotal(this.producers, this.material, this.scaledupProducers);

            // Assert
            Assert.AreEqual(expectedCost1, totalCost);
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

        private static List<ProducerDetail> GetProducers()
        {
            var producers = TestDataHelper.GetProducers();

            producers[0].ProducerReportedMaterials.Add(new ProducerReportedMaterial
            {
                MaterialId = 1,
                ProducerDetailId = 1,
                PackagingType = "HH",
                PackagingTonnage = (1 * 100),
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
                PackagingTonnage = (1 * 100),
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
                PackagingTonnage = (2 * 100),
                Material = new Material
                {
                    Id = 1,
                    Code = "AL",
                    Name = "Material1",
                    Description = "Material1",
                },
            });

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
            return this.Fixture.Create<CalcResultParameterCommunicationCost>();
        }

        private CalcResultLateReportingTonnage GetCalcResultLateReportingTonnage()
        {
            return this.Fixture.Create<CalcResultLateReportingTonnage>();
        }
    }
}
