using System.Collections.Immutable;
using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IMaterialService
    {
        public Task<ImmutableArray<MaterialDto>> GetMaterials(CancellationToken cancellationToken = default);
        public Task<ImmutableDictionary<string, int>> GetMaterialIdsByType(CancellationToken cancellationToken = default);
    }

    public class MaterialService(
        ApplicationDBContext dbContext)
        : IMaterialService
    {
        public async Task<ImmutableArray<MaterialDto>> GetMaterials(CancellationToken cancellationToken = default)
        {
            return await dbContext.Material
                .Select(material => new MaterialDto
                {
                    Id = material.Id,
                    Code = material.Code,
                    Name = material.Name
                })
                .ToImmutableArrayAsync(cancellationToken);
        }

        public async Task<ImmutableDictionary<string, int>> GetMaterialIdsByType(CancellationToken cancellationToken = default)
        {
            return await dbContext.Material
                .Select(m => new { m.Name, m.Id })
                .ToImmutableDictionaryAsync(m => m.Name, m => m.Id, cancellationToken);
        }
    }
}