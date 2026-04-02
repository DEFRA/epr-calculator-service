namespace EPR.Calculator.Service.Function.Models
{
    public record CalcResultLateReportingTonnage
    {
        public string Name { get; init; } = string.Empty;

        public string MaterialHeading { get; init; } = string.Empty;

        public string TonnageHeading { get; init; } = string.Empty;
        public string RedTonnageHeading { get; init; } = string.Empty;
        public string AmberTonnageHeading { get; init; } = string.Empty;
        public string GreenTonnageHeading { get; init; } = string.Empty;

        required public IEnumerable<CalcResultLateReportingTonnageDetail> CalcResultLateReportingTonnageDetails { get; init; }
            = Array.Empty<CalcResultLateReportingTonnageDetail>();
    }
}