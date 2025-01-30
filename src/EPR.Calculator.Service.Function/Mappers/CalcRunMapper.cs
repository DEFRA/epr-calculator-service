using EPR.Calculator.Service.Function.Data.DataModels;
using EPR.Calculator.Service.Function.Dtos;

namespace EPR.Calculator.Service.Function.Mappers
{
    public static class CalcRunMapper
    {
        public static readonly string FileExtension = "CSV";

        public static CalculatorRunDto Map(CalculatorRun run, CalculatorRunClassification classification)
        {
            return new CalculatorRunDto
            {
                RunId = run.Id,
                CreatedAt = run.CreatedAt,
                FileExtension = FileExtension,
                RunName = run.Name ?? string.Empty,
                UpdatedBy = run.UpdatedAt,
                UpdatedAt = run.UpdatedAt,
                RunClassificationId = run.CalculatorRunClassificationId,
                RunClassificationStatus = classification.Status
            };
        }
    }
}
