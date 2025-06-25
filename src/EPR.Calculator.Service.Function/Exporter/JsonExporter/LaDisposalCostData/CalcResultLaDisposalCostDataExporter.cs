using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.LaDisposalCostData
{
    public class CalcResultLaDisposalCostDataExporter : ICalcResultLaDisposalCostDataExporter
    {
        private readonly ICalcResultLaDisposalCostDataMapper mapper;

        public CalcResultLaDisposalCostDataExporter(ICalcResultLaDisposalCostDataMapper mapper)
        {
            this.mapper = mapper;
        }

        public string Export(List<CalcResultLaDisposalCostDataDetail> laDisposalCostData)
        {
            var result = this.mapper.Map(laDisposalCostData);
            return JsonConvert.SerializeObject(result);
        }
    }
}
