using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.OnePlusFourApportionment
{
    public class OnePlusFourApportionmentJsonExporter : IOnePlusFourApportionmentJsonExporter
    {
        private IOnePlusFourApportionmentMapper onePlusFourApportionmentMapper { get; set; }

        public OnePlusFourApportionmentJsonExporter(IOnePlusFourApportionmentMapper mapper)
        {
            onePlusFourApportionmentMapper = mapper;
        }

        public CalcResultOnePlusFourApportionmentJson Export(CalcResultOnePlusFourApportionment calcResult1Plus4Apportionment)
        {
            if (calcResult1Plus4Apportionment.CalcResultOnePlusFourApportionmentDetails is null) return new CalcResultOnePlusFourApportionmentJson();
            return onePlusFourApportionmentMapper.Map(calcResult1Plus4Apportionment);
        }
    }
}
