using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.LateReportingTonnages
{
    public class CalcResultLateReportingBuilder : ICalcResultLateReportingBuilder
    {
        public const string LateReportingHeader = "Parameters - Late Reporting Tonnages";
        public const string Total = "Total";
        public const string MaterialHeading = "Material";
        public const string TonnageHeading = "Late Reporting Tonnage";
        public const string RedTonnageHeading = "Red + Red Medical Late Reporting Tonnage";
        public const string AmberTonnageHeading = "Amber + Amber Medical Late Reporting Tonnage";
        public const string GreenTonnageHeading = "Green + Green Medical Late Reporting Tonnage";
        private readonly ApplicationDBContext context;

        public CalcResultLateReportingBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<CalcResultLateReportingTonnage> ConstructAsync(CalcResultsRequestDto resultsRequestDto)
        {
            var result = await (from run in context.CalculatorRuns
                                join master in context.DefaultParameterSettings
                                on run.DefaultParameterSettingMasterId equals master.Id
                                join detail in context.DefaultParameterSettingDetail on master.Id equals detail.DefaultParameterSettingMasterId
                                join template in context.DefaultParameterTemplateMasterList on detail.ParameterUniqueReferenceId equals template.ParameterUniqueReferenceId
                                where run.Id == resultsRequestDto.RunId && template.ParameterType == TonnageHeading
                                select new { template.ParameterCategory, detail.ParameterValue}).ToListAsync();

            var tonnageDetails = result
                                .GroupBy(x => RemoveSuffix(x.ParameterCategory))
                                .Select(g =>
                                {
                                    var red = GetParameterValueBySuffix(g, "-R");
                                    var amber = GetParameterValueBySuffix(g, "-A");
                                    var green = GetParameterValueBySuffix(g, "-G");

                                    return new CalcResultLateReportingTonnageDetail
                                    {
                                        Name = g.Key,
                                        RedLateReportingTonnage = red,
                                        AmberLateReportingTonnage = amber,
                                        GreenLateReportingTonnage = green,
                                        TotalLateReportingTonnage = red + amber + green
                                    };
                                }).ToList();

            tonnageDetails.Add(new CalcResultLateReportingTonnageDetail
            {
                Name = Total,
                RedLateReportingTonnage = tonnageDetails.Sum(r => r.RedLateReportingTonnage),
                AmberLateReportingTonnage = tonnageDetails.Sum(r => r.AmberLateReportingTonnage),
                GreenLateReportingTonnage = tonnageDetails.Sum(r => r.GreenLateReportingTonnage),
                TotalLateReportingTonnage = tonnageDetails.Sum(r => r.TotalLateReportingTonnage)
            });

            return new CalcResultLateReportingTonnage
            {
                Name = LateReportingHeader,
                MaterialHeading = MaterialHeading,
                TonnageHeading = TonnageHeading,
                RedTonnageHeading = RedTonnageHeading,
                AmberTonnageHeading = AmberTonnageHeading,
                GreenTonnageHeading = GreenTonnageHeading,
                CalcResultLateReportingTonnageDetails = tonnageDetails,
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