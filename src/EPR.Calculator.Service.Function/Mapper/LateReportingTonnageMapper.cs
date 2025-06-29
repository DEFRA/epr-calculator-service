using System.Linq;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class LateReportingTonnageMapper : ILateReportingTonnageMapper
    {
        public const string LateReportingJsonHeader = "Late Reporting Tonnage";
        public const string Total = "total";

        CalcResultLateReportingTonnageJson ILateReportingTonnageMapper.Map(CalcResultLateReportingTonnage? calcResultLateReportingTonnage)
        {
            if (calcResultLateReportingTonnage is null) return new CalcResultLateReportingTonnageJson();
            return new CalcResultLateReportingTonnageJson
            {
                Name = LateReportingJsonHeader,
                calcResultLateReportingTonnageDetails = calcResultLateReportingTonnage.CalcResultLateReportingTonnageDetails
                .Where(n=>n.Name.Trim().ToLower() != Total)
                .Select(t => new CalcResultLateReportingTonnageDetailsJson { MaterialName = t.Name, TotalLateReportingTonnage = t.TotalLateReportingTonnage }).ToList(),
                
                CalcResultLateReportingTonnageTotal = calcResultLateReportingTonnage.CalcResultLateReportingTonnageDetails
                .Where(n => n.Name.Trim().ToLower() != Total)
                .Sum(t => t.TotalLateReportingTonnage)
            };
        }
    }
}
