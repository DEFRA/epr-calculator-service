﻿namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultLaDisposalCostDataDetail
    {
        public required string Name { get; set; }
        public string? Material { get; set; }
        public required string England { get; set; }
        public required string Wales { get; set; }
        public required string Scotland { get; set; }
        public required string NorthernIreland { get; set; }
        public required string Total { get; set; }

        public required string ProducerReportedHouseholdPackagingWasteTonnage { get; set; }

        public string LateReportingTonnage { get; set; } = string.Empty;

        public string ProducerReportedHouseholdTonnagePlusLateReportingTonnage { get; set; } = string.Empty;

        public string? DisposalCostPricePerTonne { get; set; }

        public int OrderId { get; set; }
    }
}
