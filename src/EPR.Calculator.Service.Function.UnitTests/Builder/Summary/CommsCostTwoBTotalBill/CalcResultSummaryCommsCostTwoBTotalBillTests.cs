namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.CommsCostTwoBTotalBill
{
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Builder.Summary;
    using EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoBTotalBill;
    using EPR.Calculator.Service.Function.Builder.Summary.TonnageVsAllProducer.cs;
    using EPR.Calculator.Service.Function.Models;

    [TestClass]
    public class CalcResultSummaryCommsCostTwoBTotalBillTests
    {
        public required IEnumerable<TotalPackagingTonnagePerRun> TotalPackagingTonnage;
        private CalcResult _calcResult;
        private List<ProducerDetail> _producers;
        private List<CalcResultsProducerAndReportMaterialDetail> _allResults;
        private Fixture Fixture { get; init; } = new Fixture();

        public CalcResultSummaryCommsCostTwoBTotalBillTests()
        {
            this._producers = GetProducers();

            this._calcResult = new CalcResult
            {
                CalcResultParameterOtherCost = TestDataHelper.GetCalcResultParameterOtherCost(),
                CalcResultDetail = TestDataHelper.GetCalcResultDetail(),
                CalcResultLaDisposalCostData = TestDataHelper.GetCalcResultLaDisposalCostData(),
                CalcResultLapcapData = TestDataHelper.GetCalcResultLapcapData(),
                CalcResultOnePlusFourApportionment = this.GetCalcResultOnePlusFourApportionment(),
                CalcResultParameterCommunicationCost = this.GetCalcResultParameterCommunicationCost(),
                CalcResultSummary = TestDataHelper.GetCalcResultSummary(),
                CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
                CalcResultLateReportingTonnageData = this.GetCalcResultLateReportingTonnage(),
                CalcResultScaledupProducers =  TestDataHelper.GetScaledupProducers(),
            };

            // Set up consistent data
            this._calcResult.CalcResultParameterOtherCost = Fixture.Create<CalcResultParameterOtherCost>();
            this._calcResult.CalcResultParameterOtherCost.BadDebtProvision = new KeyValuePair<string, string>("10 Bad Debt Provision", "10.00%");

            this._allResults = new List<CalcResultsProducerAndReportMaterialDetail>
            {
                new CalcResultsProducerAndReportMaterialDetail
                {
                    ProducerDetail = new ProducerDetail
                    {
                        Id = 1,
                        CalculatorRunId = 1,
                        SubsidiaryId = "1",
                        ProducerId = 1,
                        ProducerName = "Producer1",
                    },
                    ProducerReportedMaterial = new ProducerReportedMaterial
                    {
                        Id = 1,
                        MaterialId = 1,
                        ProducerDetailId = 1,
                        PackagingType = "HH",
                        PackagingTonnage = 100,
                        Material = new Material
                        {
                            Id = 1,
                            Code = "HH",
                            Name = "Material1",
                            Description = "Material1",
                        },
                    },
                },
                new CalcResultsProducerAndReportMaterialDetail
                {
                    ProducerDetail = new ProducerDetail
                    {
                        Id = 2,
                        CalculatorRunId = 1,
                        SubsidiaryId = "1",
                        ProducerId = 2,
                        ProducerName = "Producer2",
                    },
                    ProducerReportedMaterial = new ProducerReportedMaterial
                    {
                        Id = 2,
                        MaterialId = 1,
                        ProducerDetailId = 2,
                        PackagingType = "HH",
                        PackagingTonnage = 900,
                        Material = new Material
                        {
                            Id = 1,
                            Code = "HH",
                            Name = "Material1",
                            Description = "Material1",
                        },
                    },
                },
            };

            var materails = TestDataHelper.GetMaterials();
            CalcResultSummaryBuilder.ScaledupProducers = Fixture.Create<List<CalcResultScaledupProducer>>();
            this.TotalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(this._allResults, materails, 1);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this._producers = null!;
            this._calcResult = null!;
            this._allResults = null!;
        }

        [TestMethod]
        public void GetCommsProducerFeeWithBadDebtFor2bTotalsRow_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 278.300m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2bTotalsRow(this._calcResult, this._producers, this.TotalPackagingTonnage);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsEnglandWithBadDebtTotalsRow_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 139.1500m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebtTotalsRow(this._calcResult, this._producers, this.TotalPackagingTonnage);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsNorthernIrelandWithBadDebtTotalsRow_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 27.8300m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebtTotalsRow(this._calcResult, this._producers, this.TotalPackagingTonnage);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsEnglandWithBadDebt_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 139.1500m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebt(this._calcResult, this._producers[0], this.TotalPackagingTonnage);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsWalesWithBadDebt_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 55.6600m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWalesWithBadDebt(this._calcResult, this._producers[0], this.TotalPackagingTonnage);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsScotlandWithBadDebt_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 55.6600m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsScotlandWithBadDebt(this._calcResult, this._producers[0], this.TotalPackagingTonnage);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsNorthernIrelandWithBadDebt_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 27.8300m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebt(this._calcResult, this._producers[0], this.TotalPackagingTonnage);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsWithBadDebt_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 139.1500m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWithBadDebt(this._calcResult, this._producers[0], this.TotalPackagingTonnage, "England");

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetRegionApportionment_ShouldReturnCorrectValue()
        {
            // Arrange
            decimal expectedValue = 0.50m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetRegionApportionment(this._calcResult, "England");

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsBadDebtProvisionFor2b_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 25.300m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2b(this._calcResult, this._producers[0], this.TotalPackagingTonnage);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsProducerFeeWithBadDebtFor2b_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 278.300m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2b(this._calcResult, this._producers[0], this.TotalPackagingTonnage);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void CalculateProducerFee_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 253.0m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.CalculateProducerFee(this._calcResult, this._producers[0], this.TotalPackagingTonnage, false);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        [TestMethod]
        public void GetCommsProducerFeeWithoutBadDebtFor2b_ShouldReturnCorrectTotal()
        {
            // Arrange
            decimal expectedValue = 253.0m;

            // Act
            var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2b(this._calcResult, this._producers[0], this.TotalPackagingTonnage);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }

        private List<ProducerDetail> GetProducers()
        {
            var producers = Fixture.CreateMany<ProducerDetail>(2).ToList();
            producers[0].SubsidiaryId = "1";
            producers[0].CalculatorRunId = 1;
            producers[0].ProducerId = 1;

            producers[0].ProducerReportedMaterials.Add(new ProducerReportedMaterial
            {
                Id = 1,
                MaterialId = 1,
                ProducerDetailId = 1,
                PackagingType = "HH",
                PackagingTonnage = 100,
                Material = new Material
                {
                    Id = 1,
                    Code = "HH",
                    Name = "Material1",
                    Description = "Material1",
                },
            });

            return producers;
        }

        private CalcResultParameterCommunicationCost GetCalcResultParameterCommunicationCost()
        {
            return this.Fixture.Create<CalcResultParameterCommunicationCost>();
        }

        private CalcResultLateReportingTonnage GetCalcResultLateReportingTonnage()
        {
            return this.Fixture.Create<CalcResultLateReportingTonnage>();
        }

        private CalcResultOnePlusFourApportionment GetCalcResultOnePlusFourApportionment()
        {
            var calcResultOnePlusFourApportionment = this.Fixture.Create<CalcResultOnePlusFourApportionment>();

            // Ensure the lists have enough elements
            calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails = this.Fixture.CreateMany<CalcResultOnePlusFourApportionmentDetail>(5).ToList();

            // Set up consistent data
            calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Last().EnglandDisposalTotal = "50%";
            calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Last().WalesDisposalTotal = "20%";
            calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Last().ScotlandDisposalTotal = "20%";
            calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Last().NorthernIrelandDisposalTotal = "10%";

            return calcResultOnePlusFourApportionment;
        }
    }
}