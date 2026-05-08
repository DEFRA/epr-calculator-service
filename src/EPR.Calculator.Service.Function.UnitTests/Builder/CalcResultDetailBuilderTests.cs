using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.Detail;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Data;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    [TestClass]
    public class CalcResultDetailBuilderTests
    {
        private ApplicationDBContext _dbContext = null!;
        private CalcResultDetailBuilder _sut = null!;
        private RunContext _runContext = DummyData.RunContexts.CalculatorRun2025;
        private IFixture _fixture = null!;

        [TestInitialize]
        public void Init()
        {
            _fixture = TestFixtures.New();
            _dbContext = _fixture.Freeze<ApplicationDBContext>();
            SeedDatabase();

            _sut = new CalcResultDetailBuilder(_dbContext);
        }

        [TestCleanup]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        private void SeedDatabase()
        {
            var calculatorRun = new CalculatorRun
            {
                Id = _runContext.RunId,
                Name = _runContext.RunName,
                RelativeYear = _runContext.RelativeYear,
                CreatedBy = _runContext.User,
                CreatedAt = new DateTime(2023, 1, 1),
                CalculatorRunOrganisationDataMaster = new CalculatorRunOrganisationDataMaster { CreatedBy = "", RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2023, 1, 1), CreatedAt = new DateTime(2023, 1, 1) },
                CalculatorRunPomDataMaster = new CalculatorRunPomDataMaster { CreatedBy = "", RelativeYear = new RelativeYear(2024), EffectiveFrom = new DateTime(2023, 1, 1), CreatedAt = new DateTime(2023, 1, 1) },
                LapcapDataMaster = new LapcapDataMaster
                {
                    LapcapFileName = "LapcapFile.csv",
                    CreatedAt = new DateTime(2023, 1, 1),
                    CreatedBy = "TestUser",
                    RelativeYear = new RelativeYear(2024),
                },
                DefaultParameterSettingMaster = new DefaultParameterSettingMaster
                {
                    ParameterFileName = "Parameters.csv",
                    CreatedAt = new DateTime(2023, 1, 1),
                    CreatedBy = "TestUser",
                    RelativeYear = new RelativeYear(2024),
                },
            };

            _dbContext.CalculatorRuns.Add(calculatorRun);
            _dbContext.SaveChanges();
        }

        [TestMethod]
        public async Task Construct_AllPropertiesPresent_ReturnsCorrectData()
        {
            var runContext = DummyData.RunContexts.CalculatorRun2025;
            var result = await _sut.ConstructAsync(DummyData.RunContexts.CalculatorRun2025);
            Assert.AreEqual(runContext.RunId, result.RunId);
            Assert.AreEqual(runContext.RunName, result.RunName);
            Assert.AreEqual(runContext.User, result.RunBy);
            Assert.AreEqual(new DateTime(2023, 1, 1), result.RunDate);
            Assert.AreEqual(runContext.RelativeYear, result.RelativeYear);
            Assert.AreEqual("01/01/2023 00:00", result.RpdFileORG);
            Assert.AreEqual("01/01/2023 00:00", result.RpdFilePOM);
            Assert.AreEqual("LapcapFile.csv,01/01/2023 00:00,TestUser", result.LapcapFile);
            Assert.AreEqual("Parameters.csv,01/01/2023 00:00,TestUser", result.ParametersFile);
        }

        [TestMethod]
        public async Task Construct_MissingOptionalProperties_ReturnsPartialData()
        {
            _dbContext.CalculatorRuns.RemoveRange(_dbContext.CalculatorRuns);
            await _dbContext.SaveChangesAsync();

            var calculatorRun = new CalculatorRun
            {
                Id = 2,
                Name = "RunWithMissingProps",
                CreatedBy = "TestUser2",
                CreatedAt = new DateTime(2023, 2, 1),
                RelativeYear = new RelativeYear(2025),
            };

            _dbContext.CalculatorRuns.Add(calculatorRun);
            await _dbContext.SaveChangesAsync();

            var runContext = DummyData.RunContexts.CalculatorRun2025 with { RunId = 2 };
            var result = await _sut.ConstructAsync(runContext);

            Assert.AreEqual(2, result.RunId);
            Assert.AreEqual("RunWithMissingProps", result.RunName);
            Assert.AreEqual("TestUser2", result.RunBy);
            Assert.AreEqual(new DateTime(2023, 2, 1), result.RunDate);
            Assert.AreEqual(2025, result.RelativeYear.Value);
            Assert.IsTrue(string.IsNullOrEmpty(result.RpdFileORG));
            Assert.IsTrue(string.IsNullOrEmpty(result.RpdFilePOM));
            Assert.IsTrue(string.IsNullOrEmpty(result.LapcapFile));
            Assert.IsTrue(string.IsNullOrEmpty(result.ParametersFile));
        }
    }
}
