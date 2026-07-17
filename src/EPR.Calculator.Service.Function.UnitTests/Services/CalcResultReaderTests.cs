using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class CalcResultReaderTests
    {
        private IFixture _fixture = null!;
        private SqliteConnection _connection = null!;
        private ApplicationDBContext _dbContext = null!;
        private CalcResultReader _sut = null!;

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
            _sut = _fixture.Create<CalcResultReader>();
        }

        [TestCleanup]
        public void TearDown()
        {
            _dbContext.Dispose();
            _connection.Dispose();
        }

        [TestMethod]
        public async Task ReadH1ProjectedData_WorksAsExpected()
        {
            _dbContext.AddRange(new List<TransformProjectedH1>
            {
                MkTransformProjectedH1(1, 1, null, MaterialCodes.Aluminium, "1"),
                MkTransformProjectedH1(1, 1, null, MaterialCodes.Glass, "1", isGlass: true),
                MkTransformProjectedH1(1, 1, null, MaterialCodes.PaperOrCard, "1"),
                MkTransformProjectedH1(1, 1, null, MaterialCodes.Aluminium, "2"),
                MkTransformProjectedH1(1, 1, null, MaterialCodes.Glass, "2", isGlass: true),
                MkTransformProjectedH1(1, 1, null, MaterialCodes.PaperOrCard, "2"),
                MkTransformProjectedH1(1, 1, "A", MaterialCodes.Aluminium, "2"),
                MkTransformProjectedH1(1, 1, "A", MaterialCodes.Glass, "2", isGlass: true),
                MkTransformProjectedH1(1, 1, "A", MaterialCodes.PaperOrCard, "2"),
                MkTransformProjectedH1(2, 1, "A", MaterialCodes.Aluminium, "2"),
                MkTransformProjectedH1(2, 1, "A", MaterialCodes.Glass, "2", isGlass: true),
                MkTransformProjectedH1(2, 1, "A", MaterialCodes.PaperOrCard, "2")
             });
            await _dbContext.SaveChangesAsync();
            
            var result = await _sut.ReadH1ProjectedData(1, CancellationToken.None);
            result.Count.ShouldBe(3);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == null && p.Level == "1").H1ProjectedTonnageByMaterial.Count.ShouldBe(3);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == null && p.Level == "2").H1ProjectedTonnageByMaterial.Count.ShouldBe(3);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == "A" && p.Level == "2").H1ProjectedTonnageByMaterial.Count.ShouldBe(3);
        }

        [TestMethod]
        public async Task ReadH2ProjectedData_WorksAsExpected()
        {
            _dbContext.AddRange(new List<TransformProjectedH2>
            {
                MkTransformProjectedH2(1, 1, null, MaterialCodes.Aluminium, "1"),
                MkTransformProjectedH2(1, 1, null, MaterialCodes.Glass, "1", isGlass: true),
                MkTransformProjectedH2(1, 1, null, MaterialCodes.PaperOrCard, "1"),
                MkTransformProjectedH2(1, 1, null, MaterialCodes.Aluminium, "2"),
                MkTransformProjectedH2(1, 1, null, MaterialCodes.Glass, "2", isGlass: true),
                MkTransformProjectedH2(1, 1, null, MaterialCodes.PaperOrCard, "2"),
                MkTransformProjectedH2(1, 1, "A", MaterialCodes.Aluminium, "2"),
                MkTransformProjectedH2(1, 1, "A", MaterialCodes.Glass, "2", isGlass: true),
                MkTransformProjectedH2(1, 1, "A", MaterialCodes.PaperOrCard, "2"),
                MkTransformProjectedH2(2, 1, "A", MaterialCodes.Aluminium, "2"),
                MkTransformProjectedH2(2, 1, "A", MaterialCodes.Glass, "2", isGlass: true),
                MkTransformProjectedH2(2, 1, "A", MaterialCodes.PaperOrCard, "2")
             });
            await _dbContext.SaveChangesAsync();
            
            var result = await _sut.ReadH2ProjectedData(1, CancellationToken.None);
            result.Count.ShouldBe(3);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == null && p.Level == "1").H2ProjectedTonnageByMaterial.Count.ShouldBe(3);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == null && p.Level == "2").H2ProjectedTonnageByMaterial.Count.ShouldBe(3);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == "A" && p.Level == "2").H2ProjectedTonnageByMaterial.Count.ShouldBe(3);
        }

        [TestMethod]
        public async Task ReadScaledData_WorksAsExpected()
        {
            _dbContext.AddRange(new List<TransformScaled>
            {   
                MkTransformScaled(1, 1, null, 1, "HH"),
                MkTransformScaled(1, 1, null, 1, "PB"),
                MkTransformScaled(1, 1, "A", 2, "HH"),
                MkTransformScaled(1, 1, "A", 2, "PB"),
                MkTransformScaled(1, 2, "B", 3, "CW"),
                MkTransformScaled(1, 2, "B", 4, "HDC"),
                MkTransformScaled(2, 2, "B", 3, "CW"),
                MkTransformScaled(2, 2, "B", 4, "HDC"),
            });
            await _dbContext.SaveChangesAsync();
            
            var result = await _sut.ReadScaledData(1, CancellationToken.None);
            result.Count.ShouldBe(3);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == null).PomData.Count.ShouldBe(2);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == "A").PomData.Count.ShouldBe(2);
            result.First(p => p.ProducerId == 2 && p.SubsidiaryId == "B").PomData.Count.ShouldBe(2);
        }

        [TestMethod]
        public async Task ReadPartialData_WorksAsExpected()
        {
            _dbContext.AddRange(new List<TransformPartial>
            {   
                MkTransformPartial(1, 1, null, MaterialCodes.Aluminium, "1", isModulated: true),
                MkTransformPartial(1, 1, null, MaterialCodes.Glass, "1", isModulated: true, isGlass: true),
                MkTransformPartial(1, 1, "A", MaterialCodes.Steel, "2", isModulated: true),
                MkTransformPartial(1, 1, "A", MaterialCodes.OtherMaterials, "2", isModulated: true),
                MkTransformPartial(1, 2, "B", MaterialCodes.Aluminium, "2", isModulated: true),
                MkTransformPartial(1, 2, "B", MaterialCodes.Glass, "2", isModulated: true, isGlass: true),
                MkTransformPartial(2, 1, null, MaterialCodes.Aluminium, "1", isModulated: true),
                MkTransformPartial(2, 1, null, MaterialCodes.Glass, "1", isModulated: true, isGlass: true),
            });
            await _dbContext.SaveChangesAsync();
            
            var result = await _sut.ReadPartialData(1, CancellationToken.None);
            result.Count.ShouldBe(3);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == null).PartialObligationTonnageByMaterial.Count.ShouldBe(2);
            result.First(p => p.ProducerId == 1 && p.SubsidiaryId == "A").PartialObligationTonnageByMaterial.Count.ShouldBe(2);
            result.First(p => p.ProducerId == 2 && p.SubsidiaryId == "B").PartialObligationTonnageByMaterial.Count.ShouldBe(2);
        }

        [TestMethod]
        public async Task ReadProducerFees_WorksAsExpected()
        {
            var producerFees = TestDataHelper.GetProducerFees();
            _dbContext.Add(producerFees);
            await _dbContext.SaveChangesAsync();

            var result = await _sut.ReadProducerFees(0, CancellationToken.None);

            result.CalculatorRunId.ShouldBeEquivalentTo(producerFees.CalculatorRunId);
            result.Details.ShouldBeEquivalentTo(producerFees.Details);
            result.Total.ShouldBeEquivalentTo(producerFees.Total);
        }

        [TestMethod]
        public async Task ReadSmcw_WorksAsExpected()
        {
            var smcw = MkSelfManagedConsumerWaste(1);
            _dbContext.Add(smcw);
            await _dbContext.SaveChangesAsync();

            var result = await _sut.ReadSmcw(1, CancellationToken.None);

            result.CalculatorRunId.ShouldBe(1);
            result.ProducerTotals.Count.ShouldBe(1);
            result.ProducerTotals.First().ProducerId.ShouldBe(1);
            result.ProducerTotals.First().SubsidiaryId.ShouldBe("A");
            result.ProducerTotals.First().Level.ShouldBe(1);
            result.ProducerTotals.First().SmcwByMaterial[MaterialCodes.Aluminium].SmcwTonnage.ShouldBe(100);
            result.TotalByMaterial[MaterialCodes.Aluminium].SmcwTonnage.ShouldBe(100);
        }

        [TestMethod]
        public async Task ReadModulationResult_WorksAsExpected()
        {
            var modulation = MkModulationResult(1);
            _dbContext.Add(modulation);
            await _dbContext.SaveChangesAsync();

            var result = await _sut.ReadModulationResult(1, CancellationToken.None);

            result.CalculatorRunId.ShouldBe(1);
            result.GreenFactor.ShouldBe(1.5m);
            result.RedFactor.ShouldBe(2.5m);
            result.ModulationByMaterial.Count.ShouldBe(1);
            var material = new MaterialDetail { Id = 1, Code = MaterialCodes.Aluminium, Name = "Aluminium" };
            result.ModulationByMaterial[material].RedMaterialDisposalCost.ShouldBe(10);
        }

        private ModulationResult MkModulationResult(int runId)
        {
            var material = new MaterialDetail { Id = 1, Code = MaterialCodes.Aluminium, Name = "Aluminium" };

            return new ModulationResult
            {
                CalculatorRunId = runId,
                GreenFactor = 1.5m,
                RedFactor = 2.5m,
                ModulationByMaterial = new Dictionary<MaterialDetail, ModulationDetail>
                {
                    [material] = new ModulationDetail
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
            };
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
                        SmcwByMaterial = new Dictionary<string, SelfManagedConsumerWasteData>
                        {
                            [MaterialCodes.Aluminium] = smcwData
                        }
                    }
                },
                TotalByMaterial = new Dictionary<string, SelfManagedConsumerWasteData>
                {
                    [MaterialCodes.Aluminium] = smcwData
                }
            };
        }

        private TransformProjectedH1 MkTransformProjectedH1(int runId, int producerId, string? subsidiaryId, string materialCode, string level, bool isGlass = false) {
            return new TransformProjectedH1
            {
                CalculatorRunId = runId,
                ProducerId = producerId,
                SubsidiaryId = subsidiaryId,
                Level = level,
                SubmissionPeriodCode = "2025-H1",
                MaterialCode = materialCode,
                HouseholdTonnage = 100,
                HouseholdTonnageRed = 50,
                HouseholdTonnageAmber = 0,
                HouseholdTonnageGreen = 0,
                HouseholdTonnageRedMedical = 0,
                HouseholdTonnageAmberMedical = 0,
                HouseholdTonnageGreenMedical = 0,
                PublicBinTonnage = 200,
                PublicBinTonnageRed = 100,
                PublicBinTonnageAmber = 0,
                PublicBinTonnageGreen = 0,
                PublicBinTonnageRedMedical = 0,
                PublicBinTonnageAmberMedical = 0,
                PublicBinTonnageGreenMedical = 0,
                HDCTonnage = isGlass ? 300 : null,
                HDCTonnageRed = isGlass ? 150 : null,
                HDCTonnageAmber = isGlass ? 0 : null,
                HDCTonnageGreen = isGlass ? 0 : null,
                HDCTonnageRedMedical = isGlass ? 0 : null,
                HDCTonnageAmberMedical = isGlass ? 0 : null,
                HDCTonnageGreenMedical = isGlass ? 0 : null,
                ProjectedHouseholdTonnage = 100,
                ProjectedHouseholdTonnageRed = 50,
                ProjectedHouseholdTonnageAmber = 0,
                ProjectedHouseholdTonnageGreen = 0,
                ProjectedHouseholdTonnageRedMedical = 0,
                ProjectedHouseholdTonnageAmberMedical = 0,
                ProjectedHouseholdTonnageGreenMedical = 0,
                ProjectedPublicBinTonnage = 200,
                ProjectedPublicBinTonnageRed = 100,
                ProjectedPublicBinTonnageAmber = 0,
                ProjectedPublicBinTonnageGreen = 0,
                ProjectedPublicBinTonnageRedMedical = 0,
                ProjectedPublicBinTonnageAmberMedical = 0,
                ProjectedPublicBinTonnageGreenMedical = 0,
                ProjectedHDCTonnage = isGlass ? 300 : null,
                ProjectedHDCTonnageRed = isGlass ? 150 : null,
                ProjectedHDCTonnageAmber = isGlass ? 0 : null,
                ProjectedHDCTonnageGreen = isGlass ? 0 : null,
                ProjectedHDCTonnageRedMedical = isGlass ? 0 : null,
                ProjectedHDCTonnageAmberMedical = isGlass ? 0 : null,
                ProjectedHDCTonnageGreenMedical = isGlass ? 0 : null,
                H2RamProportionsRed = 100,
                H2RamProportionsAmber = 0,
                H2RamProportionsGreen = 0,
                H2RamProportionsRedMedical = 0,
                H2RamProportionsAmberMedical = 0,
                H2RamProportionsGreenMedical = 0
            };
        }

        private TransformProjectedH2 MkTransformProjectedH2(int runId, int producerId, string? subsidiaryId, string materialCode, string level, bool isGlass = false) {
            return new TransformProjectedH2
            {
                CalculatorRunId = runId,
                ProducerId = producerId,
                SubsidiaryId = subsidiaryId,
                Level = level,
                SubmissionPeriodCode = "2025-H2",
                MaterialCode = materialCode,
                HouseholdTonnage = 100,
                HouseholdTonnageRed = 50,
                HouseholdTonnageAmber = 0,
                HouseholdTonnageGreen = 0,
                HouseholdTonnageRedMedical = 0,
                HouseholdTonnageAmberMedical = 0,
                HouseholdTonnageGreenMedical = 0,
                PublicBinTonnage = 200,
                PublicBinTonnageRed = 100,
                PublicBinTonnageAmber = 0,
                PublicBinTonnageGreen = 0,
                PublicBinTonnageRedMedical = 0,
                PublicBinTonnageAmberMedical = 0,
                PublicBinTonnageGreenMedical = 0,
                HDCTonnage = isGlass ? 300 : null,
                HDCTonnageRed = isGlass ? 150 : null,
                HDCTonnageAmber = isGlass ? 0 : null,
                HDCTonnageGreen = isGlass ? 0 : null,
                HDCTonnageRedMedical = isGlass ? 0 : null,
                HDCTonnageAmberMedical = isGlass ? 0 : null,
                HDCTonnageGreenMedical = isGlass ? 0 : null,
                ProjectedHouseholdTonnage = 100,
                ProjectedHouseholdTonnageRed = 50,
                ProjectedHouseholdTonnageAmber = 0,
                ProjectedHouseholdTonnageGreen = 0,
                ProjectedHouseholdTonnageRedMedical = 0,
                ProjectedHouseholdTonnageAmberMedical = 0,
                ProjectedHouseholdTonnageGreenMedical = 0,
                ProjectedPublicBinTonnage = 200,
                ProjectedPublicBinTonnageRed = 100,
                ProjectedPublicBinTonnageAmber = 0,
                ProjectedPublicBinTonnageGreen = 0,
                ProjectedPublicBinTonnageRedMedical = 0,
                ProjectedPublicBinTonnageAmberMedical = 0,
                ProjectedPublicBinTonnageGreenMedical = 0,
                ProjectedHDCTonnage = isGlass ? 300 : null,
                ProjectedHDCTonnageRed = isGlass ? 150 : null,
                ProjectedHDCTonnageAmber = isGlass ? 0 : null,
                ProjectedHDCTonnageGreen = isGlass ? 0 : null,
                ProjectedHDCTonnageRedMedical = isGlass ? 0 : null,
                ProjectedHDCTonnageAmberMedical = isGlass ? 0 : null,
                ProjectedHDCTonnageGreenMedical = isGlass ? 0 : null
            };
        }

        private TransformScaled MkTransformScaled(int runId, int producerId, string? subId, int materialId, string packagingType)
        {
            return new TransformScaled()
            {
                CalculatorRunId = runId,
                ProducerId = producerId, 
                SubsidiaryId = subId,
                ProducerName = "Producer",
                TradingName = "Trading",
                SubmissionPeriodCode = "2024-P2",
                Level = "1",
                IsSubTotal = false,
                DaysInSubmissionPeriod = 180,
                DaysInWholePeriod = 365,
                ScaleupFactor = 2,
                MaterialId = materialId,
                PackagingType = packagingType,
                Tonnage = 10,
                ScaledTonnage = 20
            };
        }

        private TransformPartial MkTransformPartial(int runId, int producerId, string? subsidiaryId, string materialCode, string level, bool isModulated = true, bool isGlass = false) {
            return new TransformPartial
            {
                CalculatorRunId = runId,
                ProducerId = producerId,
                SubsidiaryId = subsidiaryId,
                ProducerName = "Producer",
                TradingName = "Trading",
                Level = level,
                SubmissionYear = 2025,
                DaysInSubmissionYear = 365,
                JoiningDate = "15/07/2025",
                DaysObligated = 180,
                ObligatedFactor = 0.5m,
                MaterialCode = materialCode,
                HouseholdTonnage = 100,
                HouseholdTonnageRed = isModulated ? 50 : null,
                HouseholdTonnageAmber = isModulated ? 0 : null,
                HouseholdTonnageGreen = isModulated ? 0 : null,
                HouseholdTonnageRedMedical = isModulated ? 0 : null,
                HouseholdTonnageAmberMedical = isModulated ? 0 : null,
                HouseholdTonnageGreenMedical = isModulated ? 0 : null,
                PublicBinTonnage = 200,
                PublicBinTonnageRed = isModulated ? 100 : null,
                PublicBinTonnageAmber = isModulated ? 0 : null,
                PublicBinTonnageGreen = isModulated ? 0 : null,
                PublicBinTonnageRedMedical = isModulated ? 0 : null,
                PublicBinTonnageAmberMedical = isModulated ? 0 : null,
                PublicBinTonnageGreenMedical = isModulated ? 0 : null,
                HDCTonnage = isGlass ? 300 : null,
                HDCTonnageRed = isModulated && isGlass ? 150 : null,
                HDCTonnageAmber = isModulated && isGlass ? 0 : null,
                HDCTonnageGreen = isModulated && isGlass ? 0 : null,
                HDCTonnageRedMedical = isModulated && isGlass ? 0 : null,
                HDCTonnageAmberMedical = isModulated && isGlass ? 0 : null,
                HDCTonnageGreenMedical = isModulated && isGlass ? 0 : null,
                SMCWTonnage = 50
            };
        }
    }
}

