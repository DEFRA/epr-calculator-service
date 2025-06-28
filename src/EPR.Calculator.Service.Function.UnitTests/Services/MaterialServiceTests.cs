using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    /// <summary>
    /// Unit tests for the <see cref="MaterialService"/> class.
    /// </summary>
    [TestClass]
    public class MaterialServiceTests
    {
        private Mock<IDbContextFactory<ApplicationDBContext>> dbContextFactory;
        private ApplicationDBContext dbContext;
        private MaterialService materialService;

        public MaterialServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            this.dbContextFactory = new Mock<IDbContextFactory<ApplicationDBContext>>();
            this.dbContext = new ApplicationDBContext(options);
            this.dbContextFactory.Setup(factory => factory.CreateDbContext()).Returns(this.dbContext);
            this.materialService = new MaterialService(this.dbContextFactory.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.dbContext?.Dispose();
        }

        [TestMethod]
        public async Task ShouldReturnMaterials()
        {
            // Arrange
            var materials = new List<Material>();
            var materialDetails = TestDataHelper.GetMaterials();

            foreach (var material in materialDetails)
            {
                materials.Add(new Material
                {
                    Name = material.Name,
                    Code = material.Code,
                    Description = material.Description
                });
            }

            this.dbContext.Material.AddRange(materials);
            this.dbContext.SaveChanges();

            // Act
            var result = await this.materialService.GetMaterials();

            // Assert
            Assert.AreEqual(8, result.Count);
        }
    }
}
