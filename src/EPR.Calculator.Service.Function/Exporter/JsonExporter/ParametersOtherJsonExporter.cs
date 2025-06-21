using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter
{
    public class ParametersOtherJsonExporter : IParametersOtherJsonExporter
    {
        private readonly IParametersOtherMapper _mapper;

        public ParametersOtherJsonExporter(IParametersOtherMapper mapper)
        {
            _mapper = mapper;
        }

        public string Export(CalcResultParameterOtherCost calcResultParametersOther)
        {
            var result = _mapper.Map(calcResultParametersOther);
            return JsonConvert.SerializeObject(result);
        }
    }
}