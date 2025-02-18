using EPR.Calculator.Service.Function.Data.DataModels;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultProducerAndReportMaterialDetail
    {
        public required ProducerDetail ProducerDetail { get; set; }
        public required ProducerReportedMaterial ProducerReportedMaterial { get; set; }
    }
}
