using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.LaDisposalCostData
{
    public class CalcResultLaDisposalCostDataExporter : ICalcResultLaDisposalCostDataExporter
    {
        private readonly ICalcResultLaDisposalCostDataMapper mapper;

        public CalcResultLaDisposalCostDataExporter(ICalcResultLaDisposalCostDataMapper mapper)
        {
            this.mapper = mapper;
        }

        public CalcResultLaDisposalCostDataJson Export(IEnumerable<CalcResultLaDisposalCostDataDetail> laDisposalCostData)
        {
            return this.mapper.Map(laDisposalCostData);
        }
    }
}
