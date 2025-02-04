﻿using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultLateReportingTonnage
    {
        public string Name { get; set; } = string.Empty;

        public string MaterialHeading { get; set; } = string.Empty;

        public string TonnageHeading { get; set; } = string.Empty;

        public required IEnumerable<CalcResultLateReportingTonnageDetail> CalcResultLateReportingTonnageDetails { get; set; }
            = [];
    }
}