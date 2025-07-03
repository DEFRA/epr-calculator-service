using EPR.Calculator.Service.Function.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IMaterialService
    {
        public Task<List<MaterialDetail>> GetMaterials();
    }
}
