using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter
{
    public class CommsCostJsonExporter : ICommsCostJsonExporter
    {
        private readonly ICommsCostMapper mapper;

        public CommsCostJsonExporter(ICommsCostMapper mapper)
        {
            this.mapper = mapper;
        }

        public CalcResultCommsCostJson Export(CalcResultCommsCost communicationCost)
        {
            return this.mapper.Map(communicationCost);
        }       
    }
}
