using System.Collections.Generic;
using System.Threading.Tasks;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.ErrorReport
{
    public interface ICalcResultErrorReportBuilder
    {
        public IEnumerable<CalcResultErrorReport> ConstructAsync(CalcResultsRequestDto resultsRequestDto);
    }
}