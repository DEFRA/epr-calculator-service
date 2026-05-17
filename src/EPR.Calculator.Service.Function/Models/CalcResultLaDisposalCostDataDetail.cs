namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultLaDisposalCostDataDetail
    {
        public required decimal England { get; set; }

        public required decimal Wales { get; set; }

        public required decimal Scotland { get; set; }

        public required decimal NorthernIreland { get; set; }

        public required decimal Total { get; set; }

        public required decimal ProducerReportedHouseholdPackagingWasteTonnage { get; set; }

        public required decimal ReportedPublicBinTonnage { get; set; }

        public decimal? HouseholdDrinkContainers { get; set; }

        public decimal? LateReportingTonnage { get; set; }

        public decimal? TotalReportedTonnage { get; set; }

        public decimal? ActionedSelfManagedConsumerWasteTonnage { get; set; }

        public decimal? ProducerReportedTotalTonnage { get; set; }

        public decimal? DisposalCostPricePerTonne { get; set; }
    }
}
