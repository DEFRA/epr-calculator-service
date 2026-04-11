using System.Collections.Immutable;
using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IMaterialService
    {
        public Task<List<MaterialDetail>> GetMaterials();
        public Task<ImmutableDictionary<string, int>> GetMaterialIdsByType();
    }

    public class MaterialService(
        ApplicationDBContext dbContext)
        : IMaterialService
    {
        public async Task<List<MaterialDetail>> GetMaterials()
        {
            var materials = await dbContext.Material.ToListAsync();
            return MaterialMapper.Map(materials);
        }

        public async Task<ImmutableDictionary<string, int>> GetMaterialIdsByType()
        {
            var materials = await dbContext.Material.AsNoTracking().ToListAsync();
            return materials.ToImmutableDictionary(m => m.Name, m => m.Id);
        }
    }
}