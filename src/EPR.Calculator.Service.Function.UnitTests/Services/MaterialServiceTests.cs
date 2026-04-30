using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    /// <summary>
    /// Unit tests for the <see cref="MaterialService"/> class.
    /// </summary>
    [TestClass]
    public class MaterialServiceTests
    {
        private ApplicationDBContext _dbContext = null!;
        private MaterialService _materialService = null!;

        [TestInitialize]
        public async Task Init()
        {
            var dbMaterials = TestDataHelper.Materials.Select(m => new Material
            {
                Id = m.Id,
                Code = m.Code,
                Name = m.Name
            });

            _dbContext = TestFixtures.New().Create<ApplicationDBContext>();
            await _dbContext.Material.AddRangeAsync(dbMaterials);
            await _dbContext.SaveChangesAsync();

            _materialService = new MaterialService(_dbContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Dispose();
        }

        [TestMethod]
        public async Task Should_return_materials()
        {
            // Act
            var result = await _materialService.GetMaterials();

            // Assert
            result.Length.ShouldBe(TestDataHelper.Materials.Length);
        }

        [TestMethod]
        public async Task Should_return_materials_by_type()
        {
            // Act
            var result = await _materialService.GetMaterialIdsByType();

            // Assert
            result.Count.ShouldBe(TestDataHelper.Materials.Length);
        }
    }
}