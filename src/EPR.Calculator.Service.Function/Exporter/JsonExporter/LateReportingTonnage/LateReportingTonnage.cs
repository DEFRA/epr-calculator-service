using EPR.Calculator.Service.Function.Exporter.JsonExporter.LateReportingTonnage;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.LateReportingTonnage
{
    public class LateReportingTonnage : ILateReportingTonnage
    {
        private readonly ILateReportingTonnageMapper _mapper;
        public LateReportingTonnage(ILateReportingTonnageMapper lateReportingMapper)
        {
            _mapper = lateReportingMapper;
        }


        public CalcResultLateReportingTonnageJson Export(CalcResultLateReportingTonnage? calcResultLateReportingData)
        {
            return _mapper.Map(calcResultLateReportingData);
        }
    }
}
