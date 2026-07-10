using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class CalcResultWriterTests
    {
        private IFixture _fixture = null!;
        private SqliteConnection _connection = null!;
        private ApplicationDBContext _dbContext = null!;
        private CalcResultWriter _sut = null!;

        [TestInitialize]
        public void Init()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseSqlite(_connection)
                .Options;

            _dbContext = new ApplicationDBContext(options);
            _dbContext.Database.EnsureCreated();

            _fixture = TestFixtures.New();
            _fixture.Inject(_dbContext);
            _sut = _fixture.Create<CalcResultWriter>();
        }

        [TestCleanup]
        public void TearDown()
        {
            _dbContext.Dispose();
            _connection.Dispose();
        }

        [TestMethod]
        public async Task StoreProjectedH1Data_WorksAsExpected()
        {
            var projectedProducers = ImmutableList.Create(
                new CalcResultH1ProjectedProducer
                {
                    ProducerId = 1,
                    SubsidiaryId = null,
                    Level = "1",
                    SubmissionPeriodCode = "2025-H1",
                    IsSubtotal = true,
                    H1ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage>
                    {
                        [MaterialCodes.Aluminium] = MkH1MaterialTonnage(),
                        [MaterialCodes.Glass] = MkH1MaterialTonnage(isGlass: true),
                        [MaterialCodes.PaperOrCard] = MkH1MaterialTonnage(),
                        [MaterialCodes.Steel] = MkH1MaterialTonnage()
                    }
                },
                new CalcResultH1ProjectedProducer
                {
                    ProducerId = 1,
                    SubsidiaryId = null,
                    Level = "2",
                    SubmissionPeriodCode = "2025-H1",
                    IsSubtotal = false,
                    H1ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage>
                    {
                        [MaterialCodes.Aluminium] = MkH1MaterialTonnage(),
                        [MaterialCodes.Glass] = MkH1MaterialTonnage(isGlass: true),
                        [MaterialCodes.PaperOrCard] = MkH1MaterialTonnage()
                    }
                },
                new CalcResultH1ProjectedProducer
                {
                    ProducerId = 1,
                    SubsidiaryId = "A",
                    Level = "2",
                    SubmissionPeriodCode = "2025-H1",
                    IsSubtotal = false,
                    H1ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage>
                    {
                        [MaterialCodes.Aluminium] = MkH1MaterialTonnage(),
                        [MaterialCodes.Steel] = MkH1MaterialTonnage()
                    }
                }
            );
            await _sut.StoreProjectedH1Data(1, projectedProducers, CancellationToken.None);

            var storedH1 = await _dbContext.TransformProjectedH1.ToImmutableListAsync();
            storedH1.Count.ShouldBe(9);
            storedH1.Where(p => p.ProducerId == 1 && p.SubsidiaryId == null && p.Level == "1").ToList().Count.ShouldBe(4);
            storedH1.Where(p => p.ProducerId == 1 && p.SubsidiaryId == null && p.Level == "2").ToList().Count.ShouldBe(3);
            storedH1.Where(p => p.ProducerId == 1 && p.SubsidiaryId == "A" && p.Level == "2").ToList().Count.ShouldBe(2);
        }

        [TestMethod]
        public async Task StoreProjectedH2Data_WorksAsExpected()
        {
            var projectedProducers = ImmutableList.Create(
                new CalcResultH2ProjectedProducer
                {
                    ProducerId = 1,
                    SubsidiaryId = null,
                    Level = "1",
                    SubmissionPeriodCode = "2025-H2",
                    IsSubtotal = true,
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        [MaterialCodes.Aluminium] = MkH2MaterialTonnage(),
                        [MaterialCodes.Glass] = MkH2MaterialTonnage(isGlass: true),
                        [MaterialCodes.PaperOrCard] = MkH2MaterialTonnage(),
                        [MaterialCodes.Steel] = MkH2MaterialTonnage()
                    }
                },
                new CalcResultH2ProjectedProducer
                {
                    ProducerId = 1,
                    SubsidiaryId = null,
                    Level = "2",
                    SubmissionPeriodCode = "2025-H2",
                    IsSubtotal = false,
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        [MaterialCodes.Aluminium] = MkH2MaterialTonnage(),
                        [MaterialCodes.Glass] = MkH2MaterialTonnage(isGlass: true),
                        [MaterialCodes.PaperOrCard] = MkH2MaterialTonnage()
                    }
                },
                new CalcResultH2ProjectedProducer
                {
                    ProducerId = 1,
                    SubsidiaryId = "A",
                    Level = "2",
                    SubmissionPeriodCode = "2025-H2",
                    IsSubtotal = false,
                    H2ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                    {
                        [MaterialCodes.Aluminium] = MkH2MaterialTonnage(),
                        [MaterialCodes.Steel] = MkH2MaterialTonnage()
                    }
                }
            );
            await _sut.StoreProjectedH2Data(1, projectedProducers, CancellationToken.None);

            var storedH2 = await _dbContext.TransformProjectedH2.ToImmutableListAsync();
            storedH2.Count.ShouldBe(9);
            storedH2.Where(p => p.ProducerId == 1 && p.SubsidiaryId == null && p.Level == "1").ToList().Count.ShouldBe(4);
            storedH2.Where(p => p.ProducerId == 1 && p.SubsidiaryId == null && p.Level == "2").ToList().Count.ShouldBe(3);
            storedH2.Where(p => p.ProducerId == 1 && p.SubsidiaryId == "A" && p.Level == "2").ToList().Count.ShouldBe(2);
        }

        [TestMethod]
        public async Task StoreScaledData_WorksAsExpected()
        {
            var scaled = ImmutableList.Create(
                new CalcResultScaledupProducer
                {
                    ProducerId = 101001,
                    SubsidiaryId = null,
                    ProducerName = "Allied Packaging",
                    Level = "1",
                    SubmissionPeriodCode = "2024-P2",
                    DaysInSubmissionPeriod = 91,
                    DaysInWholePeriod = 91,
                    ScaleupFactor = 2,
                    PomData = new List<ScaledupPomEntry>
                    {
                        new ScaledupPomEntry(1, PackagingTypes.Household, 1000, 2000),
                        new ScaledupPomEntry(1, PackagingTypes.PublicBin, 100, 200),
                        new ScaledupPomEntry(1, PackagingTypes.ConsumerWaste, 500, 1000),
                        new ScaledupPomEntry(2, PackagingTypes.Household, 1000, 2000),
                        new ScaledupPomEntry(2, PackagingTypes.PublicBin, 100, 200),
                        new ScaledupPomEntry(2, PackagingTypes.HouseholdDrinksContainers, 120, 240),
                        new ScaledupPomEntry(2, PackagingTypes.ConsumerWaste, 500, 1000),
                    },
                }
            );
            await _sut.StoreScaledData(1, scaled, CancellationToken.None);

            var storedScaled = await _dbContext.TransformScaled.ToImmutableListAsync();
            storedScaled.Count.ShouldBe(7);
            storedScaled.Any(p => p.MaterialId == 1 && p.PackagingType == PackagingTypes.Household && p.Tonnage == 1000 & p.ScaledTonnage == 2000);
            storedScaled.Any(p => p.MaterialId == 1 && p.PackagingType == PackagingTypes.ConsumerWaste && p.Tonnage == 500 & p.ScaledTonnage == 1000);
            storedScaled.Any(p => p.MaterialId == 2 && p.PackagingType == PackagingTypes.HouseholdDrinksContainers && p.Tonnage == 120 & p.ScaledTonnage == 240);
        }

        [TestMethod]
        public async Task StorePartialData_WorksAsExpected()
        {
            var partial = ImmutableList.Create(
                new CalcResultPartialObligation
                {
                    ProducerId = 101001,
                    SubsidiaryId = null,
                    ProducerName = "Allied Packaging",
                    Level = "1",
                    SubmissionYear = 2024,
                    DaysInSubmissionYear = 365,
                    DaysObligated = 91,
                    ObligatedFactor = 0.3m,
                    JoiningDate = "15/07/2024",
                    PartialObligationTonnageByMaterial = new Dictionary<string, CalcResultPartialObligationTonnage>()
                    {
                        [MaterialCodes.Aluminium] = MkPartialMaterialTonnage(isModulated: true),
                        [MaterialCodes.Steel] = MkPartialMaterialTonnage(isModulated: true),
                        [MaterialCodes.Glass] = MkPartialMaterialTonnage(isModulated: true, isGlass: true),
                    }
                }
            );
            await _sut.StorePartialData(1, partial, CancellationToken.None);

            var storedPartial = await _dbContext.TransformPartial.ToImmutableListAsync();
            storedPartial.Count.ShouldBe(3);
            storedPartial.Any(p => p.ProducerId == 101001 && p.SubsidiaryId == null && p.MaterialCode == MaterialCodes.Aluminium);
            storedPartial.Any(p => p.ProducerId == 101001 && p.SubsidiaryId == null && p.MaterialCode == MaterialCodes.Steel);
            storedPartial.Any(p => p.ProducerId == 101001 && p.SubsidiaryId == null && p.MaterialCode == MaterialCodes.Glass);
        }

        [TestMethod]
        public async Task StoreProducerMaterialPackaging_WorksAsExpected()
        {
            TestDataHelper.SeedDatabaseForInitialRun(_dbContext);

            ProducerReportedMaterial mkProducerReportedMaterial(int producerDetailId, string submissionPeriod, string material, string packagingType, decimal total, decimal? r, decimal? a)
            {
                return new ProducerReportedMaterial
                {
                    ProducerDetailId = producerDetailId,
                    PackagingType = packagingType,
                    MaterialId = material switch
                    {
                        "ST" => 1,
                        "AL" => 2,
                        "PL" => 3,
                        _ => throw new ArgumentException($"Unknown material code: {material}")
                    },
                    PackagingTonnage = total,
                    PackagingTonnageRed = r,
                    PackagingTonnageAmber = a,
                    PackagingTonnageGreen = 0m,
                    PackagingTonnageRedMedical = null,
                    PackagingTonnageAmberMedical = null,
                    PackagingTonnageGreenMedical = null,
                    SubmissionPeriod = submissionPeriod
                };
            }

            var producer1 = new ProducerDetail{
                Id = 1,
                ProducerId = 1,
                SubsidiaryId = null
            };
            producer1.ProducerReportedMaterials.Add(mkProducerReportedMaterial(producer1.Id, submissionPeriod: "2025-H1", material: "ST", packagingType: "PB", total:   7, r:   2, a: 5 ));
            producer1.ProducerReportedMaterials.Add(mkProducerReportedMaterial(producer1.Id, submissionPeriod: "2025-H2", material: "PL", packagingType: "HH", total:  12, r:   0, a: 11));
            producer1.ProducerReportedMaterials.Add(mkProducerReportedMaterial(producer1.Id, submissionPeriod: "2025-H1", material: "ST", packagingType: "HH", total: 201, r: 201, a: 0 ));
            var producer2 = new ProducerDetail{
                Id = 2,
                ProducerId = 1,
                SubsidiaryId = "A"
            };
            producer2.ProducerReportedMaterials.Add(mkProducerReportedMaterial(producer2.Id, submissionPeriod: "2025-H2", material: "ST", packagingType: "PB", total:   5, r:   1, a: 4 ));
            producer2.ProducerReportedMaterials.Add(mkProducerReportedMaterial(producer2.Id, submissionPeriod: "2025-H2", material: "PL", packagingType: "HH", total:  10, r:   0, a: 10));
            producer2.ProducerReportedMaterials.Add(mkProducerReportedMaterial(producer2.Id, submissionPeriod: "2025-H2", material: "ST", packagingType: "HH", total: 200, r: 200, a: 0 ));
            var producers = new List<L1Producer>
            {
                new L1Producer(1, [producer1, producer2])
            };

            await _sut.StoreProducerMaterialPackaging(producers, CancellationToken.None);

            var stored = await _dbContext.ProducerMaterialPackaging.ToImmutableListAsync();
            stored.Count.ShouldBe(6);
        }

        [TestMethod]
        public async Task StoreProducerFees_WorksAsExpected()
        {
            var producerFees = TestDataHelper.GetProducerFees();

            await _sut.StoreProducerFees(1, producerFees, CancellationToken.None);

            var stored = await _dbContext.ProducerDisposalFee.ToImmutableListAsync();
            stored.Count.ShouldBe(1);
            stored.First().ShouldBeEquivalentTo(producerFees);
        }

        [TestMethod]
        public async Task StoreSmcw_WorksAsExpected()
        {
            var smcw = MkSelfManagedConsumerWaste(1);

            await _sut.StoreSmcw(1, smcw, CancellationToken.None);

            var stored = await _dbContext.SelfManagedConsumerWaste.SingleAsync();
            stored.ShouldBeEquivalentTo(smcw);
        }

        [TestMethod]
        public async Task StoreModulationResult_WorksAsExpected()
        {
            var modulation = MkModulationResult(1);

            await _sut.StoreModulationResult(1, modulation, CancellationToken.None);

            var stored = await _dbContext.ModulationResult.SingleAsync();
            stored.ShouldBeEquivalentTo(modulation);
        }

        private SelfManagedConsumerWaste MkSelfManagedConsumerWaste(int runId)
        {
            var smcwData = new SelfManagedConsumerWasteData
            {
                SmcwTonnage = 100,
                ActionedSmcwTonnage = new RamTonnageGroup { Total = 80, Red = 20, Amber = 30, Green = 30 },
                ResidualSmcwTonnage = 20,
                NetTonnage = new RamTonnageGroup { Total = 80, Red = 20, Amber = 30, Green = 30 }
            };

            return new SelfManagedConsumerWaste
            {
                CalculatorRunId = runId,
                ProducerTotals = new List<ProducerSelfManagedConsumerWaste>
                {
                    new()
                    {
                        ProducerId = 1,
                        SubsidiaryId = "A",
                        Level = 1,
                        SmcwByMaterial = new Dictionary<string, MaterialSelfManagedConsumerWasteData>
                        {
                            [MaterialCodes.Aluminium] = new MaterialSelfManagedConsumerWasteData
                            {
                                MaterialCode = MaterialCodes.Aluminium,
                                Smcw = smcwData
                            }
                        }
                    }
                },
                TotalByMaterial = new Dictionary<string, MaterialSelfManagedConsumerWasteData>
                {
                    [MaterialCodes.Aluminium] = new MaterialSelfManagedConsumerWasteData
                    {
                        MaterialCode = MaterialCodes.Aluminium,
                        Smcw = smcwData
                    }
                }
            };
        }

        private ModulationResult MkModulationResult(int runId)
        {
            var material = new MaterialDetail { Id = 1, Code = MaterialCodes.Aluminium, Name = "Aluminium" };

            return new ModulationResult
            {
                CalculatorRunId = runId,
                GreenFactor = 1.5m,
                RedFactor = 2.5m,
                ModulationByMaterial = new Dictionary<MaterialDetail, MaterialModulation>
                {
                    [material] = new MaterialModulation
                    {
                        MaterialDetail = material,
                        ModulationDetail = new ModulationDetail
                        {
                            RedMaterialDisposalCost = 10,
                            AmberMaterialDisposalCost = 20,
                            GreenMaterialDisposalCost = 30,
                            RedMaterialTonnages = 100,
                            AmberMaterialTonnages = 200,
                            GreenMaterialTonnages = 300,
                            TotalRedMaterialAtAmberDisposalCost = 400,
                            TotalGreenMaterialAtAmberDisposalCost = 500
                        }
                    }
                }
            };
        }

        private CalcResultH2ProjectedProducerMaterialTonnage MkH2MaterialTonnage(bool isGlass = false)
        {
            return new CalcResultH2ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage = 100,
                HouseholdRAMTonnage = new RamTonnage
                {
                    Red = 50,
                    Amber = 0,
                    Green = 0,
                    RedMedical = 0,
                    AmberMedical = 0,
                    GreenMedical = 0
                },
                PublicBinTonnage = 200,
                PublicBinRAMTonnage = new RamTonnage
                {
                    Red = 100,
                    Amber = 0,
                    Green = 0,
                    RedMedical = 0,
                    AmberMedical = 0,
                    GreenMedical = 0
                },
                HouseholdDrinksContainerTonnage = isGlass ? 300 : 0,
                HouseholdDrinksContainerRAMTonnage = isGlass ? new RamTonnage
                {
                    Red = 150,
                    Amber = 0,
                    Green = 0,
                    RedMedical = 0,
                    AmberMedical = 0,
                    GreenMedical = 0
                } : null,
                HouseholdTonnageWithoutRAM = 50,
                PublicBinTonnageWithoutRAM = 100,
                HouseholdDrinksContainerTonnageWithoutRAM = isGlass ? 150 : null,
                ProjectedHouseholdTonnage = 50,
                ProjectedHouseholdRAMTonnage = new RamTonnage
                {
                    Red = 100,
                    Amber = 0,
                    Green = 0,
                    RedMedical = 0,
                    AmberMedical = 0,
                    GreenMedical = 0
                },
                ProjectedPublicBinTonnage = 200,
                ProjectedPublicBinRAMTonnage = new RamTonnage
                {
                    Red = 200,
                    Amber = 0,
                    Green = 0,
                    RedMedical = 0,
                    AmberMedical = 0,
                    GreenMedical = 0
                },
                ProjectedHouseholdDrinksContainerTonnage = isGlass ? 300 : null,
                ProjectedHouseholdDrinksContainerRAMTonnage = isGlass ? new RamTonnage
                {
                    Red = 300,
                    Amber = 0,
                    Green = 0,
                    RedMedical = 0,
                    AmberMedical = 0,
                    GreenMedical = 0
                } : null
            };
        }

        private CalcResultH1ProjectedProducerMaterialTonnage MkH1MaterialTonnage(bool isGlass = false)
        {
            return new CalcResultH1ProjectedProducerMaterialTonnage
            {
                HouseholdTonnage = 100,
                HouseholdRAMTonnage = new RamTonnage
                {
                    Red = 50,
                    Amber = 0,
                    Green = 0,
                    RedMedical = 0,
                    AmberMedical = 0,
                    GreenMedical = 0
                },
                PublicBinTonnage = 200,
                PublicBinRAMTonnage = new RamTonnage
                {
                    Red = 100,
                    Amber = 0,
                    Green = 0,
                    RedMedical = 0,
                    AmberMedical = 0,
                    GreenMedical = 0
                },
                HouseholdDrinksContainerTonnage = isGlass ? 300 : 0,
                HouseholdDrinksContainerRAMTonnage = isGlass ? new RamTonnage
                {
                    Red = 150,
                    Amber = 0,
                    Green = 0,
                    RedMedical = 0,
                    AmberMedical = 0,
                    GreenMedical = 0
                } : null,
                HouseholdTonnageWithoutRAM = 50,
                PublicBinTonnageWithoutRAM = 100,
                HouseholdDrinksContainerTonnageWithoutRAM = isGlass ? 150 : null,
                ProjectedHouseholdTonnage = 50,
                ProjectedHouseholdRAMTonnage = new RamTonnage
                {
                    Red = 100,
                    Amber = 0,
                    Green = 0,
                    RedMedical = 0,
                    AmberMedical = 0,
                    GreenMedical = 0
                },
                ProjectedPublicBinTonnage = 200,
                ProjectedPublicBinRAMTonnage = new RamTonnage
                {
                    Red = 200,
                    Amber = 0,
                    Green = 0,
                    RedMedical = 0,
                    AmberMedical = 0,
                    GreenMedical = 0
                },
                ProjectedHouseholdDrinksContainerTonnage = isGlass ? 300 : null,
                ProjectedHouseholdDrinksContainerRAMTonnage = isGlass ? new RamTonnage
                {
                    Red = 300,
                    Amber = 0,
                    Green = 0,
                    RedMedical = 0,
                    AmberMedical = 0,
                    GreenMedical = 0
                } : null,
                H2RamProportions = new RAMProportions
                {
                    Red = 100,
                    Amber = 0,
                    Green = 0,
                    RedMedical = 0,
                    AmberMedical = 0,
                    GreenMedical = 0
                }
            };
        }

        private CalcResultPartialObligationTonnage MkPartialMaterialTonnage(bool isModulated = true, bool isGlass = false, decimal obligatedFactor = 0.5m)
        {
            return new CalcResultPartialObligationTonnage
            {
                ObligatedFactor = obligatedFactor,
                HouseholdTonnage = 100,
                HouseholdRAMTonnage = isModulated ? new RamTonnage
                {
                    Red = 50,
                    Amber = 0,
                    Green = 0,
                    RedMedical = 0,
                    AmberMedical = 0,
                    GreenMedical = 0
                } : null,
                PublicBinTonnage = 200,
                PublicBinRAMTonnage = isModulated ? new RamTonnage
                {
                    Red = 100,
                    Amber = 0,
                    Green = 0,
                    RedMedical = 0,
                    AmberMedical = 0,
                    GreenMedical = 0
                } : null,
                HouseholdDrinksContainersTonnage = isGlass ? 300 : 0,
                HouseholdDrinksContainersRAMTonnage = isModulated && isGlass ? new RamTonnage
                {
                    Red = 150,
                    Amber = 0,
                    Green = 0,
                    RedMedical = 0,
                    AmberMedical = 0,
                    GreenMedical = 0
                } : null,
                SelfManagedConsumerWasteTonnage = 50
            };
        }
    }
}

