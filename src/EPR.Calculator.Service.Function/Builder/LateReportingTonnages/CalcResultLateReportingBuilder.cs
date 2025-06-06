namespace EPR.Calculator.Service.Function.Builder.LateReportingTonnages
{
    using System.Linq;
    using System.Threading.Tasks;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.EntityFrameworkCore;

    public class CalcResultLateReportingBuilder : ICalcResultLateReportingBuilder
    {
        public const string LateReportingHeader = "Parameters - Late Reporting Tonnages";
        public const string Total = "Total";
        public const string MaterialHeading = "Material";
        public const string TonnageHeading = "Late Reporting Tonnage";
        private readonly ApplicationDBContext context;

        public CalcResultLateReportingBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<CalcResultLateReportingTonnage> Construct(CalcResultsRequestDto resultsRequestDto)
        {
            var result = await (from run in this.context.CalculatorRuns
                                join master in this.context.DefaultParameterSettings
                                on run.DefaultParameterSettingMasterId equals master.Id
                                join detail in this.context.DefaultParameterSettingDetail on master.Id equals detail.DefaultParameterSettingMasterId
                                join template in this.context.DefaultParameterTemplateMasterList on detail.ParameterUniqueReferenceId equals template.ParameterUniqueReferenceId
                                where run.Id == resultsRequestDto.RunId && template.ParameterType == TonnageHeading
                                select new CalcResultLateReportingTonnageDetail
                                {
                                    Name = template.ParameterCategory,
                                    TotalLateReportingTonnage = detail.ParameterValue,
                                }).ToListAsync();

            result.Add(new CalcResultLateReportingTonnageDetail
            {
                Name = Total,
                TotalLateReportingTonnage = result.Sum(r => r.TotalLateReportingTonnage),
            });

            return new CalcResultLateReportingTonnage
            {
                Name = LateReportingHeader,
                MaterialHeading = MaterialHeading,
                TonnageHeading = TonnageHeading,
                CalcResultLateReportingTonnageDetails = result,
            };
        }
    }
}