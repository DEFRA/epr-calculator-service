using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IMaterialService
    {
        public Task<List<MaterialDetail>> GetMaterials();
    }
}
