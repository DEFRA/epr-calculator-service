using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter
{
    public class ParametersOtherJsonExporter : IParametersOtherJsonExporter
    {
        private readonly IParametersOtherMapper mapper;

        public ParametersOtherJsonExporter(IParametersOtherMapper mapper)
        {
            this.mapper = mapper;
        }

        public CalcResultParametersOtherJson Export(CalcResultParameterOtherCost calcResultParametersOther)
        {
            return this.mapper.Map(calcResultParametersOther);
        }
    }
}