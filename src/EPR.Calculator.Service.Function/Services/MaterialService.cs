using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IMaterialService
    {
        public Task<ImmutableList<MaterialDetail>> GetMaterials(CancellationToken cancellationToken = default);
        public Task<ImmutableDictionary<string, MaterialDetail>> GetMaterialsByCode(CancellationToken cancellationToken = default);
    }

    public class MaterialService(
        ApplicationDBContext dbContext)
        : IMaterialService
    {
        public async Task<ImmutableList<MaterialDetail>> GetMaterials(CancellationToken cancellationToken = default)
        {
            return await dbContext.Material
                .Select(material => new MaterialDetail
                {
                    Id = material.Id,
                    Code = material.Code,
                    Name = material.Name
                })
                .ToImmutableListAsync(cancellationToken);
        }

        public async Task<ImmutableDictionary<string, MaterialDetail>> GetMaterialsByCode(CancellationToken cancellationToken = default)
        {
            return await dbContext.Material
                .Select(material => new MaterialDetail
                {
                    Id = material.Id,
                    Code = material.Code,
                    Name = material.Name
                })
                .ToImmutableDictionaryAsync(m => m.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);
        }
    }
}
