﻿using EPR.Calculator.Service.Function.Data.DataModels;
using EPR.Calculator.Service.Function.Data;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPR.Calculator.Service.Function.Builder.Summary.SaSetupCosts;
using Microsoft.EntityFrameworkCore.Diagnostics;
using AutoFixture;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.SaSetupCosts
{
    [TestClass]
    public class SaSetupCostsProducerTests
    {
        private readonly ApplicationDBContext dbContext;
        private readonly IEnumerable<MaterialDetail> _materials;
        private readonly CalcResult _calcResult;
        private readonly Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> _materialCostSummary;
        private readonly Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> _commsCostSummary;

        private Fixture Fixture { get; init; } = new Fixture();

        public SaSetupCostsProducerTests()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();

            CreateMaterials();
            CreateProducerDetail();

            _materials = [
                new MaterialDetail
                {
                    Id = 1,
                    Code = "AL",
                    Name = "Aluminium",
                    Description = "Aluminium"
                },
                new MaterialDetail
                {
                    Id = 2,
                    Code = "FC",
                    Name = "Fibre composite",
                    Description = "Fibre composite"
                },
                new MaterialDetail
                {
                    Id = 3,
                    Code = "GL",
                    Name = "Glass",
                    Description = "Glass"
                },
                new MaterialDetail
                {
                    Id = 4,
                    Code = "PC",
                    Name = "Paper or card",
                    Description = "Paper or card"
                },
                new MaterialDetail
                {
                    Id = 5,
                    Code = "PL",
                    Name = "Plastic",
                    Description = "Plastic"
                },
                new MaterialDetail
                {
                    Id = 6,
                    Code = "ST",
                    Name = "Steel",
                    Description = "Steel"
                },
                new MaterialDetail
                {
                    Id = 7,
                    Code = "WD",
                    Name = "Wood",
                    Description = "Wood"
                },
                new MaterialDetail
                {
                    Id = 8,
                    Code = "OT",
                    Name = "Other materials",
                    Description = "Other materials"
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
                            TotalValue = 100
                        }
                    ],
                    Materiality = [
                        new CalcResultMateriality
                        {
                            Amount = "Amount £s",
                            AmountValue = 0,
                            Percentage = "%",
                            PercentageValue = 0,
                            SevenMateriality = "7 Materiality"
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
                            TotalValue = 0
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
                        TotalValue = 100
                    }
                },
                CalcResultDetail = new CalcResultDetail() { },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData()
                {
                    Name = Fixture.Create<string>(),
                    CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>()
                    {
                        new CalcResultLaDisposalCostDataDetail()
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
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                            ProducerReportedTotalTonnage = Fixture.Create<string>(),
                        },
                        new CalcResultLaDisposalCostDataDetail()
                        {
                            DisposalCostPricePerTonne="20",
                            England="EnglandTest",
                            Wales="WalesTest",
                            Name="Material1",
                            Scotland="ScotlandTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                            ProducerReportedTotalTonnage = Fixture.Create<string>(),
                        },
                        new CalcResultLaDisposalCostDataDetail()
                        {
                            DisposalCostPricePerTonne="10",
                            England="EnglandTest",
                            Wales="WalesTest",
                            Name="Material2",
                            Scotland="ScotlandTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                            ProducerReportedTotalTonnage = Fixture.Create<string>(),
                        }
                    }
                },
                CalcResultLapcapData = new CalcResultLapcapData()
                {
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>()
                    {
                    }
                },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment()
                {
                    Name = Fixture.Create<string>(),
                    CalcResultOnePlusFourApportionmentDetails =
                    [
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=14.53M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=1.15M,
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
                            ScotlandTotal=1.15M,
                            WalesTotal=020M,
                            Name="Test",
                            OrderId=4
                        }
                    ]
                },
                CalcResultParameterCommunicationCost = Fixture.Create<CalcResultParameterCommunicationCost>(),
                CalcResultSummary = new CalcResultSummary
                {
                    ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>()
                    {
                        new()
                        {
                            ProducerCommsFeesByMaterial =  new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>(){ },
                            ProducerDisposalFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>(){ },
                            ProducerId ="1",
                            ProducerName ="Test",
                            TotalProducerDisposalFeeWithBadDebtProvision =100,
                            TotalProducerCommsFeeWithBadDebtProvision =100,
                            SubsidiaryId ="1",
                            ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 1
                        }
                    }
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

                        }
                    ]
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
                    NorthernIrelandWithBadDebtProvision = 61.18m
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
                    NorthernIrelandWithBadDebtProvision = 61.18m
                });
            }
        }

        [TestCleanup]
        public void TearDown()
        {
            dbContext?.Database.EnsureDeleted();
        }

        [TestMethod]
        public void CanCallGetHeaders()
        {
            // Act
            var result = SaSetupCostsProducer.GetHeaders().ToList();

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.ProducerOneOffFeeWithoutBadDebtProvision , ColumnIndex = 224 },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.BadDebtProvision, ColumnIndex = 225 },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.ProducerOneOffFeeWithBadDebtProvision, ColumnIndex = 226 },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.EnglandTotalWithBadDebtProvision, ColumnIndex = 227 },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.WalesTotalWithBadDebtProvision, ColumnIndex = 228 },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.ScotlandTotalWithBadDebtProvision, ColumnIndex = 229 },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.NorthernIrelandTotalWithBadDebtProvision, ColumnIndex = 230 }
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

        [TestMethod]
        public void CanCallSaSetupCostsProducerFeeWithoutBadDebtProvision()
        {
            // Act
            SaSetupCostsProducer.GetProducerSetUpCosts(_calcResult, _calcResult.CalcResultSummary);

            // Assert
            Assert.AreEqual(100, _calcResult.CalcResultSummary.SaSetupCostsTitleSection5);
            Assert.AreEqual(6, _calcResult.CalcResultSummary.SaSetupCostsBadDebtProvisionTitleSection5);
            Assert.AreEqual(106, _calcResult.CalcResultSummary.SaSetupCostsWithBadDebtProvisionTitleSection5);
            Assert.AreEqual(1, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerFeeWithoutBadDebtProvisionSection5);
            Assert.AreEqual(0.06m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BadDebtProvisionSection5);
            Assert.AreEqual(1.06m, _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].TotalProducerFeeWithBadDebtProvisionSection5);

        }


        [TestMethod]
        public void CanCallGetSaSetupCostsEnglandOverallTotalWithBadDebtProvision()
        {
            SaSetupCostsProducer.GetProducerSetUpCosts(_calcResult, _calcResult.CalcResultSummary);
            // Act
            var result = SaSetupCostsProducer.GetCountryTotalWithBadDebtProvision(_calcResult, _calcResult.CalcResultSummary.SaSetupCostsTitleSection5, SaSetupCostsSummary.GetSetUpBadDebtProvision(_calcResult), _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.England);

            // Assert
            Assert.AreEqual(0.15m, Math.Round(result, 2));
        }

        [TestMethod]
        public void CanCallGetSaSetupCostsScotlandOverallTotalWithBadDebtProvision()
        {
            SaSetupCostsProducer.GetProducerSetUpCosts(_calcResult, _calcResult.CalcResultSummary);

            // Act
            var result = SaSetupCostsProducer.GetCountryTotalWithBadDebtProvision(_calcResult, _calcResult.CalcResultSummary.SaSetupCostsTitleSection5, SaSetupCostsSummary.GetSetUpBadDebtProvision(_calcResult), _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Scotland);

            // Assert
            Assert.AreEqual(0.01m, Math.Round(result, 2));
        }


        [TestMethod]
        public void CanCallGetSaSetupCostsWalesOverallTotalWithBadDebtProvision()
        {
            SaSetupCostsProducer.GetProducerSetUpCosts(_calcResult, _calcResult.CalcResultSummary);

            // Act
            var result = SaSetupCostsProducer.GetCountryTotalWithBadDebtProvision(_calcResult, _calcResult.CalcResultSummary.SaSetupCostsTitleSection5, SaSetupCostsSummary.GetSetUpBadDebtProvision(_calcResult), _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Wales);

            // Assert
            Assert.AreEqual(0.21m, Math.Round(result, 2));
        }


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
                dbContext.Material.Add(new Material
                {
                    Name = materialKv.Value,
                    Code = materialKv.Key,
                    Description = "Some"
                });
            }

            dbContext.SaveChanges();
        }

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
                "Jumbo Box Store"
            };

            var producerId = 1;
            foreach (var producerName in producerNames)
            {
                dbContext.ProducerDetail.Add(new ProducerDetail
                {
                    ProducerId = producerId++,
                    SubsidiaryId = $"{producerId}-Sub",
                    ProducerName = producerName,
                    CalculatorRunId = 1,
                });
            }

            dbContext.SaveChanges();

            for (int producerDetailId = 1; producerDetailId <= 10; producerDetailId++)
            {
                for (int materialId = 1; materialId < 9; materialId++)
                {
                    dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        MaterialId = materialId,
                        ProducerDetailId = producerDetailId,
                        PackagingType = "HH",
                        PackagingTonnage = (materialId * 100)
                    });
                    dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        MaterialId = materialId,
                        ProducerDetailId = producerDetailId,
                        PackagingType = "CW",
                        PackagingTonnage = (materialId * 50)
                    });
                }
            }
            dbContext.SaveChanges();
        }
    }
}
