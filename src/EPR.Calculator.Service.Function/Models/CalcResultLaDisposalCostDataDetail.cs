namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultLaDisposalCostDataDetail
    {
        public required decimal EnglandCost { get; set; }

        public required decimal WalesCost { get; set; }

        public required decimal ScotlandCost { get; set; }

        public required decimal NorthernIrelandCost { get; set; }

        private decimal? totalCost;
        public decimal TotalCost =>
            totalCost ??=
                EnglandCost + WalesCost + ScotlandCost + NorthernIrelandCost;

        public required decimal HouseholdPackagingWasteTonnage { get; set; }

        public required decimal PublicBinTonnage { get; set; }

        public decimal? HouseholdDrinkContainersTonnage { get; set; }

        public decimal? LateReportingTonnage { get; set; }

        public decimal? ActionedSelfManagedConsumerWasteTonnage { get; set; }

        // This is not derived since the rule changed for modulation
        public required decimal TotalTonnage { get; set; }

        // This is not derived since the rule changed for modulation
        public decimal? DisposalCostPricePerTonne { get; set; }
    }
}
