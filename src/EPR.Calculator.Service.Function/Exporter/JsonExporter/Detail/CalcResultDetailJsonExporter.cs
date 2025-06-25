using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.Detail
{
    public class CalcResultDetailJsonExporter : ICalcResultDetailJsonExporter
    {
        public CalcResultDetailJson Export(CalcResultDetail calcResultDetail)
        {
            return CalcResultDetailJsonMapper.Map(calcResultDetail);
        }
    }
}