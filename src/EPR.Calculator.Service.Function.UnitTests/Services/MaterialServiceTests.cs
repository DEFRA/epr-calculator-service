using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Services;

/// <summary>
///     Unit tests for the <see cref="MaterialService" /> class.
/// </summary>
[TestClass]
public class MaterialServiceTests
{
    private ApplicationDBContext _dbContext = null!;
    private MaterialService _sut = null!;

    [TestInitialize]
    public void Init()
    {
        var fixture = TestFixtures.New();
        _dbContext = fixture.Freeze<ApplicationDBContext>();
        _sut = fixture.Freeze<MaterialService>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Dispose();
    }

    [TestMethod]
    public async Task Should_return_materials()
    {
        // Arrange
        await _dbContext.Material.AddRangeAsync(
            new Material { Id = 1, Code = "BB", Name = "Test Material BB" },
            new Material { Id = 2, Code = "AA", Name = "Test Material AA" }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetMaterials();

        // Assert
        result.Count.ShouldBe(2);
        result.Last().Id.ShouldBe(2);
    }

    [TestMethod]
    public async Task Should_return_material_ids_by_code()
    {
        // Arrange
        await _dbContext.Material.AddRangeAsync(
            new Material { Id = 1, Code = "BB", Name = "Test Material BB" },
            new Material { Id = 2, Code = "AA", Name = "Test Material AA" }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetMaterialsByCode();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContainKey("AA");
        result.ShouldContainKey("BB");
        result["AA"].Id.ShouldBe(2);
        result["BB"].Id.ShouldBe(1);
    }
}
