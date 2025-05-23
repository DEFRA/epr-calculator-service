﻿namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultSummaryProducerCommsFeesCostByMaterial
    {
        public decimal HouseholdPackagingWasteTonnage { get; set; }

        public decimal ReportedPublicBinTonnage { get; set; }

        public decimal TotalReportedTonnage { get; set; }

        public decimal HouseholdDrinksContainers { get; set; }

        public decimal PriceperTonne { get; set; }

        public decimal ProducerTotalCostWithoutBadDebtProvision { get; set; }

        public decimal BadDebtProvision { get; set; }

        public decimal ProducerTotalCostwithBadDebtProvision { get; set; }

        public decimal EnglandWithBadDebtProvision { get; set; }

        public decimal WalesWithBadDebtProvision { get; set; }

        public decimal ScotlandWithBadDebtProvision { get; set; }

        public decimal NorthernIrelandWithBadDebtProvision { get; set; }

    }
}
