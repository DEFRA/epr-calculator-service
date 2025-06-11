using System.Linq;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class LateReportingTonnageMapper : ILateReportingTonnageMapper
    {
        CalcResultLateReportingTonnageJson ILateReportingTonnageMapper.Map(CalcResultLateReportingTonnage calcResultLateReportingTonnage)
        {
            if (calcResultLateReportingTonnage is null) return new CalcResultLateReportingTonnageJson();
            return new CalcResultLateReportingTonnageJson
            {
                Name = calcResultLateReportingTonnage.Name,
                calcResultLateReportingTonnageDetails = calcResultLateReportingTonnage.CalcResultLateReportingTonnageDetails.Select(t => new CalcResultLateReportingTonnageDetailsJson { MaterialName = t.Name, TotalLateReportingTonnage = t.TotalLateReportingTonnage }).ToList(),
                CalcResultLateReportingTonnageTotal = calcResultLateReportingTonnage.CalcResultLateReportingTonnageDetails.Sum(t => t.TotalLateReportingTonnage)
            };
        }
    }
}
