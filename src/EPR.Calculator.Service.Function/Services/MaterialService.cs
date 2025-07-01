using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Services
{
    public class MaterialService : IMaterialService
    {
        private readonly ApplicationDBContext context;

        public MaterialService(IDbContextFactory<ApplicationDBContext> context)
        {
            this.context = context.CreateDbContext();
        }

        public async Task<List<MaterialDetail>> GetMaterials()
        {
            var materials = await this.context.Material.ToListAsync();
            return MaterialMapper.Map(materials);
        }
    }
}
