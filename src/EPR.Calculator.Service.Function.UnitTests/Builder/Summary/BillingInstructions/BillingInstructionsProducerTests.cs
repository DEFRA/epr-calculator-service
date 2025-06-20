﻿namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.BillingInstructions
{
    using AutoFixture;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Builder.Summary.BillingInstructions;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Defines the <see cref="BillingInstructionsProducerTests" />
    /// </summary>
    [TestClass]
    public class BillingInstructionsProducerTests
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

        /// <summary>
        /// Defines the _materialCostSummary
        /// </summary>
        private readonly Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> _materialCostSummary;

        /// <summary>
        /// Defines the _commsCostSummary
        /// </summary>
        private readonly Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> _commsCostSummary;

        /// <summary>
        /// Gets the Fixture
        /// </summary>
        private Fixture Fixture { get; init; } = new Fixture();

        /// <summary>
        /// Initializes a new instance of the <see cref="BillingInstructionsProducerTests"/> class.
        /// </summary>
        public BillingInstructionsProducerTests()
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

            _calcResult = new CalcResult
            {
                CalcResultScaledupProducers = new CalcResultScaledupProducers(),
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    BadDebtProvision = new KeyValuePair<string, string>("key1", "6%"),
                    BadDebtValue = 6m,
                    Details = [
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = "4 LA Data Prep Charge",
                            OrderId = 1,
                            England = "£40.00",
                            EnglandValue = 40,
                            Wales = "£30.00",
                            WalesValue = 30,
                            Scotland = "£20.00",
                            ScotlandValue = 20,
                            NorthernIreland = "£10.00",
                            NorthernIrelandValue = 10,
                            Total = "£100.00",
                            TotalValue = 100,
                        }
                    ],
                    Materiality = [
                        new CalcResultMateriality
                        {
                            Amount = "Amount £s",
                            AmountValue = 0,
                            Percentage = "%",
                            PercentageValue = 0,
                            SevenMateriality = "7 Materiality",
                        }
                    ],
                    Name = "Parameters - Other",
                    SaOperatingCost = [
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = string.Empty,
                            OrderId = 0,
                            England = "England",
                            EnglandValue = 0,
                            Wales = "Wales",
                            WalesValue = 0,
                            Scotland = "Scotland",
                            ScotlandValue = 0,
                            NorthernIreland = "Northern Ireland",
                            NorthernIrelandValue = 0,
                            Total = "Total",
                            TotalValue = 0,
                        }
                    ],
                    SchemeSetupCost = {
                        Name = "5 Scheme set up cost Yearly Cost",
                        OrderId = 1,
                        England = "£40.00",
                        EnglandValue = 40,
                        Wales = "£30.00",
                        WalesValue = 30,
                        Scotland = "£20.00",
                        ScotlandValue = 20,
                        NorthernIreland = "£10.00",
                        NorthernIrelandValue = 10,
                        Total = "£100.00",
                        TotalValue = 100,
                    },
                },
                CalcResultDetail = new CalcResultDetail() { },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData()
                {
                    Name = Fixture.Create<string>(),
                    CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>()
                    {
                        new()
                        {
                            DisposalCostPricePerTonne="20",
                            England="EnglandTest",
                            Wales="WalesTest",
                            Name="ScotlandTest",
                            Scotland="ScotlandTest",
                            Material = "Material1",
                            NorthernIreland = "NorthernIrelandTest",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ProducerReportedTotalTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                        },
                        new()
                        {
                            DisposalCostPricePerTonne="20",
                            England="EnglandTest",
                            Wales="WalesTest",
                            Name="Material1",
                            Scotland="ScotlandTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ProducerReportedTotalTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                        },
                        new()
                        {
                            DisposalCostPricePerTonne="10",
                            England="EnglandTest",
                            Wales="WalesTest",
                            Name="Material2",
                            Scotland="ScotlandTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ProducerReportedTotalTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                        },
                    },
                },
                CalcResultLapcapData = new CalcResultLapcapData()
                {
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>()
                    {
                    },
                },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment()
                {
                    Name = Fixture.Create<string>(),
                    CalcResultOnePlusFourApportionmentDetails =
                    [
                        new()
                        {
                            EnglandDisposalTotal = "80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=0.10M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="1 + 4 Apportionment %s",
                        },
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=0.10M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=0.10M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=14.53M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=14.53M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                    ],
                },
                CalcResultParameterCommunicationCost = Fixture.Create<CalcResultParameterCommunicationCost>(),
                CalcResultSummary = new CalcResultSummary
                {
                    ProducerDisposalFees = GetProducerDisposalFees(),
                },
                CalcResultCommsCostReportDetail = new CalcResultCommsCost()
                {
                    CalcResultCommsCostCommsCostByMaterial =
                    [
                        new ()
                        {
                            CommsCostByMaterialPricePerTonne="0.42",
                            Name ="Aluminium",

                        },
                        new ()
                        {
                            CommsCostByMaterialPricePerTonne="0.3",
                            Name ="Glass",
                        },
                    ],
                },
                CalcResultLateReportingTonnageData = Fixture.Create<CalcResultLateReportingTonnage>(),
            };

            _materialCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>();
            _commsCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>();

            foreach (var material in _materials)
            {
                _materialCostSummary.Add(material, new CalcResultSummaryProducerDisposalFeesByMaterial
                {
                    HouseholdPackagingWasteTonnage = 1000,
                    ManagedConsumerWasteTonnage = 90,
                    NetReportedTonnage = 910,
                    PricePerTonne = 0.6676m,
                    ProducerDisposalFee = 607.52m,
                    BadDebtProvision = 36.45m,
                    ProducerDisposalFeeWithBadDebtProvision = 643.97m,
                    EnglandWithBadDebtProvision = 348.06m,
                    WalesWithBadDebtProvision = 78.46m,
                    ScotlandWithBadDebtProvision = 156.28m,
                    NorthernIrelandWithBadDebtProvision = 61.18m,
                });

                _commsCostSummary.Add(material, new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdPackagingWasteTonnage = 1000,
                    PriceperTonne = 0.6676m,
                    ProducerTotalCostWithoutBadDebtProvision = 607.52m,
                    BadDebtProvision = 36.45m,
                    ProducerTotalCostwithBadDebtProvision = 643.97m,
                    EnglandWithBadDebtProvision = 348.06m,
                    WalesWithBadDebtProvision = 78.46m,
                    ScotlandWithBadDebtProvision = 156.28m,
                    NorthernIrelandWithBadDebtProvision = 61.18m,
                });
            }
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
            var result = BillingInstructionsProducer.GetHeaders().ToList();
            var columnIndex = 292;

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.CurrentYearInvoicedTotalToDate, ColumnIndex = columnIndex },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.TonnageChangeSinceLastInvoice, ColumnIndex = columnIndex + 1 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.LiabilityDifference, ColumnIndex = columnIndex + 2 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.MaterialThresholdBreached, ColumnIndex = columnIndex + 3 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.TonnageThresholdBreached, ColumnIndex = columnIndex + 4 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.PercentageLiabilityDifference, ColumnIndex = columnIndex + 5 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.MaterialPercentageThresholdBreached, ColumnIndex = columnIndex + 6 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.TonnagePercentagThresholdBreached, ColumnIndex = columnIndex + 6 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.SuggestedBillingInstruction, ColumnIndex = columnIndex + 6 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.SuggestedInvoiceAmount, ColumnIndex = columnIndex + 6 }
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
            var result = BillingInstructionsProducer.GetSummaryHeaders().ToList();

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.Title, ColumnIndex = 292 }
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
            BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary);
            var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0];

            // Assert
            Assert.AreEqual("-", fee.CurrentYearInvoiceTotalToDate);
            Assert.AreEqual("-", fee.TonnageChangeSinceLastInvoice);
            Assert.AreEqual("-", fee.LiabilityDifference);
            Assert.AreEqual("-", fee.MaterialThresholdBreached);
            Assert.AreEqual("-", fee.TonnageThresholdBreached);
            Assert.AreEqual("-", fee.PercentageLiabilityDifference);
            Assert.AreEqual("-", fee.MaterialPercentageThresholdBreached);
            Assert.AreEqual("-", fee.TonnagePercentageThresholdBreached);
            Assert.AreEqual("-", fee.SuggestedBillingInstruction);
            Assert.AreEqual("-", fee.SuggestedInvoiceAmount);
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

        /// <summary>
        /// The GetProducerDisposalFees
        /// </summary>
        /// <returns>The <see cref="List{CalcResultSummaryProducerDisposalFees}"/></returns>
        private static List<CalcResultSummaryProducerDisposalFees> GetProducerDisposalFees()
        {
            return new List<CalcResultSummaryProducerDisposalFees>()
            {
                new()
                {
                    ProducerCommsFeesByMaterial =
                        new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>() { },
                    ProducerDisposalFeesByMaterial =
                        new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>() { },
                    ProducerId = "1",
                    ProducerName = "Test",
                    TotalProducerDisposalFeeWithBadDebtProvision = 100,
                    TotalProducerCommsFeeWithBadDebtProvision = 100,
                    SubsidiaryId = "1",
                    BadDebtProvision = 6.0m,
                    BadDebtProvisionComms = 6.5m,
                    BadDebtProvisionFor1 = 5.2m,
                    BadDebtProvisionFor2A = 6.7m,
                    BadDebtProvisionFor2bComms = 7.9m,
                    TwoCBadDebtProvision = 9.2m,
                    BadDebtProvisionFor3 = 8.3m,
                    LaDataPrepCostsBadDebtProvisionSection4 = 6.4m,
                    BadDebtProvisionSection5 = 8.5m,
                    TotalProducerFeeforLADisposalCostswoBadDebtprovision = 106.23m,
                    TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision = 207.45m,
                    TotalProducerFeeWithoutBadDebtFor2bComms = 302.56m,
                    TwoCTotalProducerFeeForCommsCostsWithoutBadDebt = 206.63m,
                    Total3SAOperatingCostwoBadDebtprovision = 190.02m,
                    LaDataPrepCostsTotalWithoutBadDebtProvisionSection4 = 129,
                    TotalProducerFeeWithoutBadDebtProvisionSection5 = 109,
                    EnglandTotalWithBadDebtProvision = 1403,
                    EnglandTotalWithBadDebtProvision2A = 1563,
                    EnglandTotalWithBadDebtFor2bComms = 521,
                    TwoCEnglandTotalWithBadDebt = 695,
                    EnglandTotalWithBadDebtProvision3 = 745,
                    LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4 = 562,
                    EnglandTotalWithBadDebtProvisionSection5 = 562,
                    WalesTotalWithBadDebtProvision = 1203,
                    WalesTotalWithBadDebtProvision2A = 1154,
                    WalesTotalWithBadDebtFor2bComms = 632,
                    TwoCWalesTotalWithBadDebt = 452,
                    WalesTotalWithBadDebtProvision3 = 558,
                    LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4 = 964,
                    WalesTotalWithBadDebtProvisionSection5 = 254,
                    ScotlandTotalWithBadDebtProvision = 752,
                    ScotlandTotalWithBadDebtProvision2A = 632,
                    ScotlandTotalWithBadDebtFor2bComms = 541,
                    TwoCScotlandTotalWithBadDebt = 448,
                    ScotlandTotalWithBadDebtProvision3 = 652,
                    LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4 = 648,
                    ScotlandTotalWithBadDebtProvisionSection5 = 845,
                    NorthernIrelandTotalWithBadDebtProvision = 832,
                    NorthernIrelandTotalWithBadDebtProvision2A = 842,
                    NorthernIrelandTotalWithBadDebtFor2bComms = 746,
                    TwoCNorthernIrelandTotalWithBadDebt = 335,
                    NorthernIrelandTotalWithBadDebtProvision3 = 451,
                    LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4 = 468,
                    NorthernIrelandTotalWithBadDebtProvisionSection5 = 365,
                    TotalProducerFeeforLADisposalCostswithBadDebtprovision = 105,
                    TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision = 204,
                    TotalProducerFeeWithBadDebtFor2bComms = 98,
                    TwoCTotalProducerFeeForCommsCostsWithBadDebt = 23.54m,
                    Total3SAOperatingCostswithBadDebtprovision = 35.87m,
                    LaDataPrepCostsTotalWithBadDebtProvisionSection4 = 65,
                    TotalProducerFeeWithBadDebtProvisionSection5 = 49.32m,
                    CurrentYearInvoiceTotalToDate = "1250.89m",
                    TonnageChangeSinceLastInvoice = string.Empty,
                    LiabilityDifference = "580.73m",
                    MaterialThresholdBreached = string.Empty,
                    TonnageThresholdBreached = string.Empty,
                    PercentageLiabilityDifference = string.Empty,
                    MaterialPercentageThresholdBreached = string.Empty,
                    TonnagePercentageThresholdBreached = string.Empty,
                    SuggestedBillingInstruction = string.Empty,
                    SuggestedInvoiceAmount = "4039"
                },
            };
        }
    }
}
