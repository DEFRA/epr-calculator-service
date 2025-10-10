using EPR.Calculator.API.Data.DataModels;
using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IOrgAndPomDataMYCScenarioService
    {
        IEnumerable<ProducerReportedMaterial> GetProducerReportedMaterials();
    }
}
