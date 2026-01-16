namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.TotalBillBreakdown
{
    using AutoFixture;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Builder.Summary.TotalBillBreakdown;
    using EPR.Calculator.Service.Function.Builder.Summary.TotalProducerBillBreakdown;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.EntityFrameworkCore.Query.Internal;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Defines the <see cref="TotalBillBreakdownProducerTests" />
    /// </summary>
    [TestClass]
    public class TotalBillBreakdownProducerTests
    {
        /// <summary>
        /// Defines the _dbContext
        /// </summary>
        private readonly ApplicationDBContext _dbContext;

        /// <summary>
        /// Defines the _materials
        /// </summary>
        private readonly IEnumerable<MaterialDetail> _materials;

        /// <summary>
        /// Defines the _calcResult
        /// </summary>
        private readonly CalcResult _calcResult;

        private readonly int columnIndex = 289;

        /// <summary>
        /// Initializes a new instance of the <see cref="TotalBillBreakdownProducerTests"/> class.
        /// </summary>
        public TotalBillBreakdownProducerTests()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _dbContext = new ApplicationDBContext(dbContextOptions);
            _dbContext.Database.EnsureCreated();

            CreateMaterials();
            CreateProducerDetail();

            _materials = [
                new MaterialDetail
                {
                    Id = 1,
                    Code = "AL",
                    Name = "Aluminium",
                    Description = "Aluminium",
                },
                new MaterialDetail
                {
                    Id = 2,
                    Code = "FC",
                    Name = "Fibre composite",
                    Description = "Fibre composite",
                },
                new MaterialDetail
                {
                    Id = 3,
                    Code = "GL",
                    Name = "Glass",
                    Description = "Glass",
                },
                new MaterialDetail
                {
                    Id = 4,
                    Code = "PC",
                    Name = "Paper or card",
                    Description = "Paper or card",
                },
                new MaterialDetail
                {
                    Id = 5,
                    Code = "PL",
                    Name = "Plastic",
                    Description = "Plastic",
                },
                new MaterialDetail
                {
                    Id = 6,
                    Code = "ST",
                    Name = "Steel",
                    Description = "Steel",
                },
                new MaterialDetail
                {
                    Id = 7,
                    Code = "WD",
                    Name = "Wood",
                    Description = "Wood",
                },
                new MaterialDetail
                {
                    Id = 8,
                    Code = "OT",
                    Name = "Other materials",
                    Description = "Other materials",
                }
            ];

            _calcResult = TestDataHelper.GetCalcResult();

            //_calcResult = new CalcResult
            //{
            //    CalcResultScaledupProducers = new CalcResultScaledupProducers(),
            //    CalcResultParameterOtherCost = new CalcResultParameterOtherCost
            //    {
            //        BadDebtProvision = new KeyValuePair<string, string>("key1", "6%"),
            //        BadDebtValue = 6m,
            //        Details = [
            //            new CalcResultParameterOtherCostDetail
            //            {
            //                Name = "4 LA Data Prep Charge",
            //                OrderId = 1,
            //                England = "£40.00",
            //                EnglandValue = 40,
            //                Wales = "£30.00",
            //                WalesValue = 30,
            //                Scotland = "£20.00",
            //                ScotlandValue = 20,
            //                NorthernIreland = "£10.00",
            //                NorthernIrelandValue = 10,
            //                Total = "£100.00",
            //                TotalValue = 100,
            //            }
            //        ],
            //        Materiality = [
            //            new CalcResultMateriality
            //            {
            //                Amount = "Amount £s",
            //                AmountValue = 0,
            //                Percentage = "%",
            //                PercentageValue = 0,
            //                SevenMateriality = "7 Materiality",
            //            }
            //        ],
            //        Name = "Parameters - Other",
            //        SaOperatingCost = [
            //            new CalcResultParameterOtherCostDetail
            //            {
            //                Name = string.Empty,
            //                OrderId = 0,
            //                England = "England",
            //                EnglandValue = 0,
            //                Wales = "Wales",
            //                WalesValue = 0,
            //                Scotland = "Scotland",
            //                ScotlandValue = 0,
            //                NorthernIreland = "Northern Ireland",
            //                NorthernIrelandValue = 0,
            //                Total = "Total",
            //                TotalValue = 0,
            //            }
            //        ],
            //        SchemeSetupCost = {
            //            Name = "5 Scheme set up cost Yearly Cost",
            //            OrderId = 1,
            //            England = "£40.00",
            //            EnglandValue = 40,
            //            Wales = "£30.00",
            //            WalesValue = 30,
            //            Scotland = "£20.00",
            //            ScotlandValue = 20,
            //            NorthernIreland = "£10.00",
            //            NorthernIrelandValue = 10,
            //            Total = "£100.00",
            //            TotalValue = 100,
            //        },
            //    },
            //    CalcResultDetail = new CalcResultDetail() { },
            //    CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData()
            //    {
            //        Name = Fixture.Create<string>(),
            //        CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>()
            //        {
            //            new()
            //            {
            //                DisposalCostPricePerTonne="20",
            //                England="EnglandTest",
            //                Wales="WalesTest",
            //                Name="ScotlandTest",
            //                Scotland="ScotlandTest",
            //                Material = "Material1",
            //                NorthernIreland = "NorthernIrelandTest",
            //                Total = "TotalTest",
            //                ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
            //                ProducerReportedTotalTonnage = Fixture.Create<string>(),
            //                ReportedPublicBinTonnage = Fixture.Create<string>(),
            //            },
            //            new()
            //            {
            //                DisposalCostPricePerTonne="20",
            //                England="EnglandTest",
            //                Wales="WalesTest",
            //                Name="Material1",
            //                Scotland="ScotlandTest",
            //                NorthernIreland = "NorthernIrelandTest",
            //                Total = "TotalTest",
            //                ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
            //                ProducerReportedTotalTonnage = Fixture.Create<string>(),
            //                ReportedPublicBinTonnage = Fixture.Create<string>(),
            //            },
            //            new()
            //            {
            //                DisposalCostPricePerTonne="10",
            //                England="EnglandTest",
            //                Wales="WalesTest",
            //                Name="Material2",
            //                Scotland="ScotlandTest",
            //                NorthernIreland = "NorthernIrelandTest",
            //                Total = "TotalTest",
            //                ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
            //                ProducerReportedTotalTonnage = Fixture.Create<string>(),
            //                ReportedPublicBinTonnage = Fixture.Create<string>(),
            //            },
            //        },
            //    },
            //    CalcResultLapcapData = new CalcResultLapcapData()
            //    {
            //        CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>()
            //        {
            //        },
            //    },
            //    CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment()
            //    {
            //        Name = Fixture.Create<string>(),
            //        CalcResultOnePlusFourApportionmentDetails =
            //        [
            //            new()
            //            {
            //                EnglandDisposalTotal = "80",
            //                NorthernIrelandDisposalTotal="70",
            //                ScotlandDisposalTotal="30",
            //                WalesDisposalTotal="20",
            //                AllTotal=0.1M,
            //                EnglandTotal=0.10M,
            //                NorthernIrelandTotal=0.15M,
            //                ScotlandTotal=0.15M,
            //                WalesTotal=020M,
            //                Name="1 + 4 Apportionment %s",
            //            },
            //            new()
            //            {
            //                EnglandDisposalTotal="80",
            //                NorthernIrelandDisposalTotal="70",
            //                ScotlandDisposalTotal="30",
            //                WalesDisposalTotal="20",
            //                AllTotal=0.1M,
            //                EnglandTotal=0.10M,
            //                NorthernIrelandTotal=0.15M,
            //                ScotlandTotal=0.15M,
            //                WalesTotal=020M,
            //                Name="Test",
            //            },
            //            new()
            //            {
            //                EnglandDisposalTotal="80",
            //                NorthernIrelandDisposalTotal="70",
            //                ScotlandDisposalTotal="30",
            //                WalesDisposalTotal="20",
            //                AllTotal=0.1M,
            //                EnglandTotal=0.10M,
            //                NorthernIrelandTotal=0.15M,
            //                ScotlandTotal=0.15M,
            //                WalesTotal=020M,
            //                Name="Test",
            //            },
            //            new()
            //            {
            //                EnglandDisposalTotal="80",
            //                NorthernIrelandDisposalTotal="70",
            //                ScotlandDisposalTotal="30",
            //                WalesDisposalTotal="20",
            //                AllTotal=0.1M,
            //                EnglandTotal=14.53M,
            //                NorthernIrelandTotal=0.15M,
            //                ScotlandTotal=0.15M,
            //                WalesTotal=020M,
            //                Name="Test",
            //            },
            //            new()
            //            {
            //                EnglandDisposalTotal="80",
            //                NorthernIrelandDisposalTotal="70",
            //                ScotlandDisposalTotal="30",
            //                WalesDisposalTotal="20",
            //                AllTotal=0.1M,
            //                EnglandTotal=14.53M,
            //                NorthernIrelandTotal=0.15M,
            //                ScotlandTotal=0.15M,
            //                WalesTotal=020M,
            //                Name="Test",
            //            },
            //        ],
            //    },
            //    CalcResultParameterCommunicationCost = Fixture.Create<CalcResultParameterCommunicationCost>(),
            //    CalcResultSummary = new CalcResultSummary
            //    {
            //        ProducerDisposalFees = TestDataHelper.GetProducerDisposalFees(),
            //    },
            //    CalcResultCommsCostReportDetail = new CalcResultCommsCost()
            //    {
            //        CalcResultCommsCostCommsCostByMaterial =
            //        [
            //            new ()
            //            {
            //                CommsCostByMaterialPricePerTonne="0.42",
            //                Name ="Aluminium",

            //            },
            //            new ()
            //            {
            //                CommsCostByMaterialPricePerTonne="0.3",
            //                Name ="Glass",
            //            },
            //        ],
            //    },
            //    CalcResultLateReportingTonnageData = Fixture.Create<CalcResultLateReportingTonnage>(),
            //};

            //_materialCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>();
            //_commsCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>();

            //foreach (var material in _materials)
            //{
            //    _materialCostSummary.Add(material, new CalcResultSummaryProducerDisposalFeesByMaterial
            //    {
            //        HouseholdPackagingWasteTonnage = 1000,
            //        ManagedConsumerWasteTonnage = 90,
            //        NetReportedTonnage = 910,
            //        PricePerTonne = 0.6676m,
            //        ProducerDisposalFee = 607.52m,
            //        BadDebtProvision = 36.45m,
            //        ProducerDisposalFeeWithBadDebtProvision = 643.97m,
            //        EnglandWithBadDebtProvision = 348.06m,
            //        WalesWithBadDebtProvision = 78.46m,
            //        ScotlandWithBadDebtProvision = 156.28m,
            //        NorthernIrelandWithBadDebtProvision = 61.18m,
            //    });

            //    _commsCostSummary.Add(material, new CalcResultSummaryProducerCommsFeesCostByMaterial
            //    {
            //        HouseholdPackagingWasteTonnage = 1000,
            //        PriceperTonne = 0.6676m,
            //        ProducerTotalCostWithoutBadDebtProvision = 607.52m,
            //        BadDebtProvision = 36.45m,
            //        ProducerTotalCostwithBadDebtProvision = 643.97m,
            //        EnglandWithBadDebtProvision = 348.06m,
            //        WalesWithBadDebtProvision = 78.46m,
            //        ScotlandWithBadDebtProvision = 156.28m,
            //        NorthernIrelandWithBadDebtProvision = 61.18m,
            //    });
            //}
        }

        /// <summary>
        /// The TearDown
        /// </summary>
        [TestCleanup]
        public void TearDown()
        {
            _dbContext?.Database.EnsureDeleted();
        }

        /// <summary>
        /// The CanCallGetHeaders
        /// </summary>
        [TestMethod]
        public void CanCallGetHeaders()
        {
            // Act
            var result = TotalBillBreakdownProducer.GetHeaders().ToList();

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.TotalProducerBillWithoutBadDebtProvision , ColumnIndex = columnIndex },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.BadDebtProvision, ColumnIndex = columnIndex+1 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.TotalProducerBillWithBadDebtProvision, ColumnIndex = columnIndex+2 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.EnglandTotalWithBadDebtProvision, ColumnIndex = columnIndex+3 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.WalesTotalWithBadDebtProvision, ColumnIndex = columnIndex+4 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.ScotlandTotalWithBadDebtProvision, ColumnIndex = columnIndex+5 },
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.NorthernIrelandTotalWithBadDebtProvision, ColumnIndex = columnIndex+6 }
            ]);

            // Assert
            Assert.AreEqual(expectedResult[0].Name, result[0].Name);
            Assert.AreEqual(expectedResult[0].ColumnIndex, result[0].ColumnIndex);
            Assert.AreEqual(expectedResult[1].Name, result[1].Name);
            Assert.AreEqual(expectedResult[1].ColumnIndex, result[1].ColumnIndex);
            Assert.AreEqual(expectedResult[2].Name, result[2].Name);
            Assert.AreEqual(expectedResult[2].ColumnIndex, result[2].ColumnIndex);
            Assert.AreEqual(expectedResult[3].Name, result[3].Name);
            Assert.AreEqual(expectedResult[3].ColumnIndex, result[3].ColumnIndex);
            Assert.AreEqual(expectedResult[4].Name, result[4].Name);
            Assert.AreEqual(expectedResult[4].ColumnIndex, result[4].ColumnIndex);
            Assert.AreEqual(expectedResult[5].Name, result[5].Name);
            Assert.AreEqual(expectedResult[5].ColumnIndex, result[5].ColumnIndex);
            Assert.AreEqual(expectedResult[6].Name, result[6].Name);
            Assert.AreEqual(expectedResult[6].ColumnIndex, result[6].ColumnIndex);
        }

        /// <summary>
        /// The CanCallGetSummaryHeaders
        /// </summary>
        [TestMethod]
        public void CanCallGetSummaryHeaders()
        {
            // Act
            var result = TotalBillBreakdownProducer.GetSummaryHeaders().ToList();

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = TotalBillBreakdownHeaders.TotalProducerBillBreakdown, ColumnIndex = columnIndex }
            ]);

            // Assert
            Assert.AreEqual(expectedResult[0].Name, result[0].Name);
            Assert.AreEqual(expectedResult[0].ColumnIndex, result[0].ColumnIndex);
        }

        /// <summary>
        /// The CanCallSetValues
        /// </summary>
        [TestMethod]
        public void CanCallSetValues()
        {
            // Act
            TotalBillBreakdownProducer.SetValues(_calcResult.CalcResultSummary);

            // Assert
            Assert.AreEqual(17673.2373499970378m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.TotalProducerFeeWithoutBadDebtProvision);
            Assert.AreEqual(1060.39424099982226m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.BadDebtProvision);
            Assert.AreEqual(18733.6315909968600m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.TotalProducerFeeWithBadDebtProvision);
            Assert.AreEqual(9610.6053147004709m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.EnglandTotalWithBadDebtProvision);
            Assert.AreEqual(2653.2546023494487m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.WalesTotalWithBadDebtProvision);
            Assert.AreEqual(4576.19121409722784m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.ScotlandTotalWithBadDebtProvision);
            Assert.AreEqual(1893.58045984971257m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.NorthernIrelandTotalWithBadDebtProvision);
        }

        /// <summary>
        /// The CanCallSetValues
        /// </summary>
        [TestMethod]
        public void CanCallSetValues_NullValues()
        {
            // Arrange
            var data = _calcResult.CalcResultSummary;
            data.ProducerDisposalFees.ToList()[0].LocalAuthorityDisposalCostsSectionOne = null;
            data.ProducerDisposalFees.ToList()[0].CommunicationCostsSectionTwoA = null;
            data.ProducerDisposalFees.ToList()[0].CommunicationCostsSectionTwoB = null;
            data.ProducerDisposalFees.ToList()[0].TwoCTotalProducerFeeForCommsCostsWithoutBadDebt = 0;
            data.ProducerDisposalFees.ToList()[0].TwoCBadDebtProvision = 0;
            data.ProducerDisposalFees.ToList()[0].TwoCTotalProducerFeeForCommsCostsWithBadDebt = 0;
            data.ProducerDisposalFees.ToList()[0].TwoCEnglandTotalWithBadDebt = 0;
            data.ProducerDisposalFees.ToList()[0].TwoCWalesTotalWithBadDebt = 0;
            data.ProducerDisposalFees.ToList()[0].TwoCScotlandTotalWithBadDebt = 0;
            data.ProducerDisposalFees.ToList()[0].TwoCNorthernIrelandTotalWithBadDebt = 0;
            data.ProducerDisposalFees.ToList()[0].SchemeAdministratorOperatingCosts = null;
            data.ProducerDisposalFees.ToList()[0].LocalAuthorityDataPreparationCosts = null;
            data.ProducerDisposalFees.ToList()[0].OneOffSchemeAdministrationSetupCosts = null;

            // Act
            TotalBillBreakdownProducer.SetValues(data);

            // Assert
            Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.TotalProducerFeeWithoutBadDebtProvision);
            Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.BadDebtProvision);
            Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.TotalProducerFeeWithBadDebtProvision);
            Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.EnglandTotalWithBadDebtProvision);
            Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.WalesTotalWithBadDebtProvision);
            Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.ScotlandTotalWithBadDebtProvision);
            Assert.AreEqual(0, data.ProducerDisposalFees.ToList()[0].TotalProducerBillBreakdownCosts!.NorthernIrelandTotalWithBadDebtProvision);
        }

        /// <summary>
        /// The CreateMaterials
        /// </summary>
        private void CreateMaterials()
        {
            var materialDictionary = new Dictionary<string, string>();
            materialDictionary.Add("AL", "Aluminium");
            materialDictionary.Add("FC", "Fibre composite");
            materialDictionary.Add("GL", "Glass");
            materialDictionary.Add("PC", "Paper or card");
            materialDictionary.Add("PL", "Plastic");
            materialDictionary.Add("ST", "Steel");
            materialDictionary.Add("WD", "Wood");
            materialDictionary.Add("OT", "Other materials");

            foreach (var materialKv in materialDictionary)
            {
                _dbContext.Material.Add(new Material
                {
                    Name = materialKv.Value,
                    Code = materialKv.Key,
                    Description = "Some",
                });
            }

            _dbContext.SaveChanges();
        }

        /// <summary>
        /// The CreateProducerDetail
        /// </summary>
        private void CreateProducerDetail()
        {
            var producerNames = new string[]
            {
                "Allied Packaging",
                "Beeline Materials",
                "Cloud Boxes",
                "Decking and Shed",
                "Electric Things",
                "French Flooring",
                "Good Fruit Co",
                "Happy Shopper",
                "Icicle Foods",
                "Jumbo Box Store",
            };

            var producerId = 1;
            foreach (var producerName in producerNames)
            {
                _dbContext.ProducerDetail.Add(new ProducerDetail
                {
                    ProducerId = producerId++,
                    SubsidiaryId = $"{producerId}-Sub",
                    ProducerName = producerName,
                    CalculatorRunId = 1,
                });
            }

            _dbContext.SaveChanges();

            for (int producerDetailId = 1; producerDetailId <= 10; producerDetailId++)
            {
                for (int materialId = 1; materialId < 9; materialId++)
                {
                    _dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        MaterialId = materialId,
                        ProducerDetailId = producerDetailId,
                        PackagingType = "HH",
                        PackagingTonnage = materialId * 100,
                    });
                    _dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        MaterialId = materialId,
                        ProducerDetailId = producerDetailId,
                        PackagingType = "CW",
                        PackagingTonnage = materialId * 50,
                    });
                }
            }

            _dbContext.SaveChanges();
        }
    }
}
