using System.Linq;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class LateReportingTonnageMapper : ILateReportingTonnageMapper
    {
        public const string LateReportingJsonHeader = "Late Reporting Tonnage";

        CalcResultLateReportingTonnageJson ILateReportingTonnageMapper.Map(CalcResultLateReportingTonnage? calcResultLateReportingTonnage)
        {
            if (calcResultLateReportingTonnage is null) return new CalcResultLateReportingTonnageJson();
            return new CalcResultLateReportingTonnageJson
            {
                Name = LateReportingJsonHeader,
                calcResultLateReportingTonnageDetails = calcResultLateReportingTonnage.CalcResultLateReportingTonnageDetails
                .Where(n=>n.Name.ToLower() != "total")
                .Select(t => new CalcResultLateReportingTonnageDetailsJson { MaterialName = t.Name, TotalLateReportingTonnage = t.TotalLateReportingTonnage }).ToList(),
                
                CalcResultLateReportingTonnageTotal = calcResultLateReportingTonnage.CalcResultLateReportingTonnageDetails
                .Where(n => n.Name.ToLower() != "total")
                .Sum(t => t.TotalLateReportingTonnage)
            };
        }
    }
}
