namespace EPR.Calculator.Service.Function.Builder.Summary
{
    using EPR.Calculator.API.Data.DataModels;

    public class CalcResultsProducerAndReportMaterialDetail
    {
        public required ProducerDetail ProducerDetail { get; set; }

        public required ProducerReportedMaterial ProducerReportedMaterial { get; set; }
    }
}