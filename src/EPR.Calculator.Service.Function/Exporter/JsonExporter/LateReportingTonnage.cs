using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter
{
    public class LateReportingTonnage : ILateReportingTonnage
    {
        private ILateReportingTonnageMapper mapper;
        public LateReportingTonnage(ILateReportingTonnageMapper lateReportingMapper)
        {
            this.mapper = lateReportingMapper;
        }


        public string Export(CalcResultLateReportingTonnage? calcResultLateReportingData)
        {
            var details = this.mapper.Map(calcResultLateReportingData);
            return JsonSerializer.Serialize(details);
        }
    }

}
