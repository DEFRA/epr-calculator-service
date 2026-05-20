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

        public decimal LateReportingTonnage { get; set; }

        // This will be null for Pre-Modulation - i.e. isn't part of the calculation
        public decimal? ActionedSelfManagedConsumerWasteTonnage { get; set; }

        private decimal? totalTonnage;
        public decimal TotalTonnage =>
            totalTonnage ??=
                LateReportingTonnage
                    + HouseholdPackagingWasteTonnage
                    + PublicBinTonnage
                    + (HouseholdDrinkContainersTonnage ?? 0)
                    - (ActionedSelfManagedConsumerWasteTonnage ?? 0);

        private decimal? disposalCostPricePerTonne;
        public decimal? DisposalCostPricePerTonne =>
            disposalCostPricePerTonne ??=
                TotalTonnage == 0 ? (decimal?)null : Math.Round(TotalCost / TotalTonnage, 4);
    }
}
