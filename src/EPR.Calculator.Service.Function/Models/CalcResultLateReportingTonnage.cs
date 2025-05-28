namespace EPR.Calculator.Service.Function.Models
{
    using System;
    using System.Collections.Generic;

    public record CalcResultLateReportingTonnage
    {
        public string Name { get; init; } = string.Empty;

        public string MaterialHeading { get; init; } = string.Empty;

        public string TonnageHeading { get; init; } = string.Empty;

        required public IEnumerable<CalcResultLateReportingTonnageDetail> CalcResultLateReportingTonnageDetails { get; init; }
            = Array.Empty<CalcResultLateReportingTonnageDetail>();
    }
}