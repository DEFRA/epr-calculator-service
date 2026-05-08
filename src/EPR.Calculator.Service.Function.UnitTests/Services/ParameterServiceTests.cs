using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    /// <summary>
    /// Unit tests for the <see cref="ParameterService"/> class.
    /// </summary>
    [TestClass]
    public class ParameterServiceTests
    {
        private IFixture _fixture = null!;
        private ApplicationDBContext _dbContext = null!;
        private ParameterService _sut = null!;

        [TestInitialize]
        public void Init()
        {
            _fixture = TestFixtures.New();
            _dbContext = _fixture.Freeze<ApplicationDBContext>();

            _sut = _fixture.Create<ParameterService>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Dispose();
        }

        [TestMethod]
        public async Task ShouldReturnDefaultParameters()
        {
            // Arrange
            var run = new CalculatorRun
            {
                Id = 1,
                Name = "Some name",
                DefaultParameterSettingMasterId = 100
            };

            var master = new DefaultParameterSettingMaster
            {
                Id = 100
            };

            var detail = new DefaultParameterSettingDetail
            {
                DefaultParameterSettingMasterId = 100,
                ParameterUniqueReferenceId = "PARAM1",
                ParameterValue = 123m
            };

            var template = new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "PARAM1",
                ParameterType = "Some Type",
                ParameterCategory = "Some Category"
            };

            _dbContext.CalculatorRuns.Add(run);
            _dbContext.DefaultParameterSettings.Add(master);
            _dbContext.DefaultParameterSettingDetail.Add(detail);
            _dbContext.DefaultParameterTemplateMasterList.Add(template);

            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.GetDefaultParameters(1);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(123m, result["PARAM1"]);
        }
    }
}
