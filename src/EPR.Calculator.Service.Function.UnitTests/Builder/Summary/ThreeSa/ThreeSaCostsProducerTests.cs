using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.Summary.ThreeSa;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.ThreeSa
{
    [TestClass]
    public class ThreeSaCostsProducerTests
    {
        private ApplicationDBContext? _dbContext;
        private IEnumerable<MaterialDetail>? _materials;
        private CalcResult? _calcResult;
        private Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>? _materialCostSummary;
        private Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>? _commsCostSummary;

        [TestInitialize]
        public void TestInitialize()
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
                ApplyModulation = false,
                CalcResultScaledupProducers = new CalcResultScaledupProducers(),
                CalcResultPartialObligations = new CalcResultPartialObligations(),
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    BadDebtValue = 6m,
                    LaDataPrepCharge = new CalcResultParameterOtherCostDetail
                    {
                        England = 40,
                        Wales = 30,
                        Scotland = 20,
                        NorthernIreland = 10,
                        Total = 100
                    },
                    SaOperatingCost = new CalcResultParameterOtherCostDetail
                    {
                        England = 40,
                        Wales = 30,
                        Scotland = 20,
                        NorthernIreland = 10,
                        Total = 100
                    },
                    SchemeSetupCost =
                    {
                        England = 40,
                        Wales = 30,
                        Scotland = 20,
                        NorthernIreland = 10,
                        Total = 100
                    }
                },
                CalcResultDetail = new CalcResultDetail { RunId = 1, RelativeYear = new RelativeYear(2024) },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData
                {
                    ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
                    {
                        ["AL"] =
                            new CalcResultLaDisposalCostDataDetail
                            {
                                DisposalCostPricePerTonne = 20,
                                England = 0,
                                Wales = 0,
                                Scotland = 0,
                                NorthernIreland = 0,
                                Total = 0,
                                ProducerReportedHouseholdPackagingWasteTonnage = 0,
                                ReportedPublicBinTonnage = 0
                            },
                        ["PL"] =
                            new CalcResultLaDisposalCostDataDetail
                            {
                                DisposalCostPricePerTonne = 20,
                                England = 0,
                                Wales = 0,
                                Scotland = 0,
                                NorthernIreland = 0,
                                Total = 0,
                                ProducerReportedHouseholdPackagingWasteTonnage = 0,
                                ReportedPublicBinTonnage = 0
                            }
                    },
                    Total = new CalcResultLaDisposalCostDataDetail
                    {
                        England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0, Total = 0,
                        ProducerReportedHouseholdPackagingWasteTonnage = 0, ReportedPublicBinTonnage = 0
                    }
                },
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    ByMaterial = [],
                    Total = new ByCountryValue(),
                    CountryApportionment = new CountryApportionmentData()
                },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment
                {
                    LaDisposalCost = new()
                    {
                        England         = 0.10M,
                        Wales           = 020M,
                        NorthernIreland = 0.15M,
                        Scotland        = 0.15M,
                        Total           = 0.1M
                    },
                    LADataPrepCharge = new()
                    {
                        England         = 0.10M,
                        Wales           = 020M,
                        Scotland        = 0.15M,
                        NorthernIreland = 0.15M,
                        Total           = 0.1M
                    },
                    TotalOnePlusFour =  new()
                    {
                        EnglandTotal         = 14.53M,
                        WalesTotal           = 020M,
                        ScotlandTotal        = 0.15M,
                        NorthernIrelandTotal = 0.15M,
                        Total                = 0.1M
                    },
                    OnePlusFourApportionment = new()
                    {
                        England         = 40,
                        Wales           = 30,
                        Scotland        = 15,
                        NorthernIreland = 15
                    }
                },
                CalcResultParameterCommunicationCost = new CalcResultParameterCommunicationCost
                {
                    Name = "some test",
                },
                CalcResultSummary = new CalcResultSummary
                {
                    ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
                    {
                        new()
                        {
                            ProducerCommsFeesByMaterial =
                                new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>(),
                            ProducerDisposalFeesByMaterial =
                                new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>(),
                            ProducerId = "1",
                            ProducerName = "Test",
                            TotalProducerDisposalFeeWithBadDebtProvision = 100,
                            TotalProducerCommsFeeWithBadDebtProvision = 100,
                            SubsidiaryId = "1",
                            ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 1,
                        },
                    },
                },
                CalcResultCommsCostReportDetail = new CalcResultCommsCost
                {
                    CommsCostByMaterial =
                    {
                        ["AL"] = new()
                        {
                            CommsCostByMaterialPricePerTonne = 0.42m,
                            England = 0,
                            Scotland = 0,
                            NorthernIreland = 0,
                            Wales = 0,
                            Total = 0,
                            ProducerReportedHouseholdPackagingWasteTonnage = 0,
                            LateReportingTonnage = 0,
                            ReportedPublicBinTonnage = 0,
                            ProducerReportedTotalTonnage = 0
                        },
                        ["GL"] = new()
                        {
                            CommsCostByMaterialPricePerTonne = 0.3m,
                            England = 0,
                            Scotland = 0,
                            NorthernIreland = 0,
                            Wales = 0,
                            Total = 0,
                            ProducerReportedHouseholdPackagingWasteTonnage = 0,
                            LateReportingTonnage = 0,
                            ReportedPublicBinTonnage = 0,
                            ProducerReportedTotalTonnage = 0
                        }
                    }
                },
                CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage
                {
                    Name = string.Empty,
                    CalcResultLateReportingTonnageDetails = new List<CalcResultLateReportingTonnageDetail>(),
                    MaterialHeading = string.Empty,
                    TonnageHeading = string.Empty
                },
                CalcResultProjectedProducers = new CalcResultProjectedProducers(),
            };

            _materialCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>();
            _commsCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>();

            foreach (var material in _materials)
            {
                _materialCostSummary.Add(material, new CalcResultSummaryProducerDisposalFeesByMaterial
                {
                    HouseholdPackagingWasteTonnage = 1000,
                    SelfManagedConsumerWasteTonnage = 90,
                    NetReportedTonnage = (total: 910, red: null, amber: null, green: null),
                    PricePerTonne = (total: 0.6676m, red: null, amber: null, green: null),
                    ProducerDisposalFee =(total: 607.52m, red: null, amber: null, green: null),
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

        [TestCleanup]
        public void TearDown()
        {
            _dbContext?.Database.EnsureDeleted();
        }

        [TestMethod]
        public void CanCallGetHeaders()
        {
            // Act
            var result = ThreeSaCostsProducer.GetHeaders().ToList();

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.TotalSaOperatingCostsWoTitleSection3 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.BadDebtProvisionSection3 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.SaOperatingCostsWithTitleSection3 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.EnglandTotalWithBadDebtProvisionSection3 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.WalesTotalWithBadDebtProvisionSection3 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.ScotlandTotalWithBadDebtProvisionSection3 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.NorthernIrelandTotalWithBadDebtProvisionSection3 }
            ]);

            // Assert
            Assert.AreEqual(expectedResult[0].Name, result[0].Name);
            Assert.AreEqual(expectedResult[1].Name, result[1].Name);
            Assert.AreEqual(expectedResult[2].Name, result[2].Name);
            Assert.AreEqual(expectedResult[3].Name, result[3].Name);
            Assert.AreEqual(expectedResult[4].Name, result[4].Name);
            Assert.AreEqual(expectedResult[5].Name, result[5].Name);
            Assert.AreEqual(expectedResult[6].Name, result[6].Name);
        }

        [TestMethod]
        public void CanCallSaSetupCostsProducerFeeWithoutBadDebtProvision()
        {
            // Act
            ThreeSaCostsProducer.GetProducerSetUpCostsSection3(_calcResult!, _calcResult!.CalcResultSummary);

            // Assert
            Assert.AreEqual(100, _calcResult.CalcResultSummary.SaOperatingCostsWoTitleSection3);
            Assert.AreEqual(6, _calcResult.CalcResultSummary.BadDebtProvisionTitleSection3);
            Assert.AreEqual(106, _calcResult.CalcResultSummary.SaOperatingCostsWithTitleSection3);
            Assert.AreEqual(1,
                _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0]
                    .SchemeAdministratorOperatingCosts!.TotalProducerFeeWithoutBadDebtProvision);
            Assert.AreEqual(0.06m,
                _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].SchemeAdministratorOperatingCosts!.BadDebtProvision);
            Assert.AreEqual(1.06m,
                _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0]
                    .SchemeAdministratorOperatingCosts!.TotalProducerFeeWithBadDebtProvision);
        }

        [TestMethod]
        public void CanCallGetSaSetupCostsEnglandOverallTotalWithBadDebtProvision()
        {
            ThreeSaCostsProducer.GetProducerSetUpCostsSection3(_calcResult!, _calcResult!.CalcResultSummary);
            // Act
            var result = ThreeSaCostsProducer.GetCountryTotalWithBadDebtProvision(
                _calcResult,
                _calcResult.CalcResultSummary.SaOperatingCostsWoTitleSection3,
                _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C,
                Countries.England
            );

            // Assert
            Assert.AreEqual(0.42m, Math.Round(result, 2));
        }

        [TestMethod]
        public void CanCallGetSaSetupCostsScotlandOverallTotalWithBadDebtProvision()
        {
            ThreeSaCostsProducer.GetProducerSetUpCostsSection3(_calcResult!, _calcResult!.CalcResultSummary);

            // Act
            var result = ThreeSaCostsProducer.GetCountryTotalWithBadDebtProvision(
                _calcResult,
                _calcResult.CalcResultSummary.SaOperatingCostsWoTitleSection3,
                _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C,
                Countries.Scotland
            );

            // Assert
            Assert.AreEqual(0.16m, Math.Round(result, 2));
        }

        [TestMethod]
        public void CanCallGetSaSetupCostsWalesOverallTotalWithBadDebtProvision()
        {
            ThreeSaCostsProducer.GetProducerSetUpCostsSection3(_calcResult!, _calcResult!.CalcResultSummary);

            // Act
            var result = ThreeSaCostsProducer.GetCountryTotalWithBadDebtProvision(
                _calcResult,
                _calcResult.CalcResultSummary.SaOperatingCostsWoTitleSection3,
                _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C,
                Countries.Wales
            );

            // Assert
            Assert.AreEqual(0.32m, Math.Round(result, 2));
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
                _dbContext?.Material.Add(new Material
                {
                    Name = materialKv.Value,
                    Code = materialKv.Key,
                    Description = "Some",
                });
            }

            _dbContext?.SaveChanges();
        }

        private void CreateProducerDetail()
        {
            var producerNames = new[]
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
                _dbContext?.ProducerDetail.Add(new ProducerDetail
                {
                    ProducerId = producerId++,
                    SubsidiaryId = $"{producerId}-Sub",
                    ProducerName = producerName,
                    CalculatorRunId = 1,
                });
            }

            _dbContext?.SaveChanges();

            for (int producerDetailId = 1; producerDetailId <= 10; producerDetailId++)
            {
                for (int materialId = 1; materialId < 9; materialId++)
                {
                    foreach (var subPeriod in new[] { "2025-H1", "2025-H2"}) {
                        _dbContext?.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                        {
                            MaterialId = materialId,
                            ProducerDetailId = producerDetailId,
                            PackagingType = "HH",
                            SubmissionPeriod = subPeriod,
                            PackagingTonnage = materialId * 50,
                        });
                        _dbContext?.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                        {
                            MaterialId = materialId,
                            ProducerDetailId = producerDetailId,
                            PackagingType = "CW",
                            SubmissionPeriod = subPeriod,
                            PackagingTonnage = materialId * 25,
                        });
                    }
                }
            }

            _dbContext?.SaveChanges();
        }
    }
}
