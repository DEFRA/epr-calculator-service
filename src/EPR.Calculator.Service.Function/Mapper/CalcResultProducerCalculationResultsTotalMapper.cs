using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CalcResultProducerCalculationResultsTotalMapper : ICalcResultProducerCalculationResultsTotalMapper
    {
        public CalcResultProducerCalculationResultsTotal? Map(
            CalcResultSummary calcResultSummary)
        {
            // specified in user story as remaining null
            return null;
        }
    }
}
