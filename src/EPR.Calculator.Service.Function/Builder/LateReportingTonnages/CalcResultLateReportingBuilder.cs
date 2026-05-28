using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.LateReportingTonnages
{
    public interface ICalcResultLateReportingBuilder
    {
        Task<CalcResultLateReportingTonnage> ConstructAsync(RunContext runContext, IImmutableList<MaterialDetail> materials);
    }

    public class CalcResultLateReportingBuilder(ApplicationDBContext dbContext)
        : ICalcResultLateReportingBuilder
    {
        public const string LateReportingHeader = "Parameters - Late Reporting Tonnages";
        public const string Total = "Total";
        public const string MaterialHeading = "Material";
        public const string TonnageHeading = "Late Reporting Tonnage";
        public const string RedTonnageHeading = "Red + Red Medical Late Reporting Tonnage";
        public const string AmberTonnageHeading = "Amber + Amber Medical Late Reporting Tonnage";
        public const string GreenTonnageHeading = "Green + Green Medical Late Reporting Tonnage";

        public async Task<CalcResultLateReportingTonnage> ConstructAsync(RunContext runContext, IImmutableList<MaterialDetail> materials)
        {
            var result = await (
                from run in dbContext.CalculatorRuns
                join master in dbContext.DefaultParameterSettings on run.DefaultParameterSettingMasterId equals master.Id
                join detail in dbContext.DefaultParameterSettingDetail on master.Id equals detail.DefaultParameterSettingMasterId
                join template in dbContext.DefaultParameterTemplateMasterList on detail.ParameterUniqueReferenceId equals template.ParameterUniqueReferenceId
                where run.Id == runContext.RunId && template.ParameterType == "Late Reporting Tonnage"
                select new { template.ParameterCategory, detail.ParameterValue }
            ).ToListAsync();

            var materialByName = materials.ToDictionary(m => m.Name, m => m.Code);

            var tonnageDetails = result
                .GroupBy(x => RemoveSuffix(x.ParameterCategory))
                .Select(g =>
                {
                    var red   = GetParameterValueBySuffix(g, "-R");
                    var amber = GetParameterValueBySuffix(g, "-A");
                    var green = GetParameterValueBySuffix(g, "-G");

                    var materialCode = materialByName[g.Key];
                    return (materialCode, new CalcResultLateReportingTonnageDetail
                    {
                        Red   = red,
                        Amber = amber,
                        Green = green,
                        Total = red + amber + green
                    });
                }
            ).ToDictionary();

            return new CalcResultLateReportingTonnage
            {
                ByMaterial = tonnageDetails
            };
        }

        private static string RemoveSuffix(string value)
        {
            if (value.EndsWith("-R") || value.EndsWith("-G") || value.EndsWith("-A"))
                return value.Substring(0, value.Length - 2);
            return value;
        }

        private static decimal GetParameterValueBySuffix(IGrouping<string, dynamic> group, string suffix)
        {
            return group.First(x => x.ParameterCategory.EndsWith(suffix)).ParameterValue;
        }
    }
}
