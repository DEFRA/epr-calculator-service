namespace EPR.Calculator.Service.Function.Models
{
    using EPR.Calculator.Service.Function.Data.DataModels;

    public class CalcResultProducerAndReportMaterialDetail
    {
        required public ProducerDetail ProducerDetail { get; set; }

        required public ProducerReportedMaterial ProducerReportedMaterial { get; set; }
    }
}
