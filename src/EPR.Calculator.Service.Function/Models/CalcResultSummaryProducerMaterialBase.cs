
namespace EPR.Calculator.Service.Function.Models
{
    public abstract class CalcResultSummaryProducerMaterialBase
    {
        public decimal HouseholdPackagingWasteTonnage { get; set; }
        public decimal HouseholdPackagingWasteTonnageRed { get; set; }
        public decimal HouseholdPackagingWasteTonnageAmber { get; set; }
        public decimal HouseholdPackagingWasteTonnageGreen { get; set; }
        public decimal HouseholdPackagingWasteTonnageRedMedical { get; set; }
        public decimal HouseholdPackagingWasteTonnageAmberMedical { get; set; }
        public decimal HouseholdPackagingWasteTonnageGreenMedical { get; set; }

        public decimal PublicBinTonnage { get; set; }
        public decimal PublicBinTonnageRed { get; set; }
        public decimal PublicBinTonnageAmber { get; set; }
        public decimal PublicBinTonnageGreen { get; set; }
        public decimal PublicBinTonnageRedMedical { get; set; }
        public decimal PublicBinTonnageAmberMedical { get; set; }
        public decimal PublicBinTonnageGreenMedical { get; set; }

        public decimal HouseholdDrinksContainersTonnage { get; set; }
        public decimal HouseholdDrinksContainersTonnageRed { get; set; }
        public decimal HouseholdDrinksContainersTonnageAmber { get; set; }
        public decimal HouseholdDrinksContainersTonnageGreen { get; set; }
        public decimal HouseholdDrinksContainersTonnageRedMedical { get; set; }
        public decimal HouseholdDrinksContainersTonnageAmberMedical { get; set; }
        public decimal HouseholdDrinksContainersTonnageGreenMedical { get; set; }

        public decimal TotalReportedTonnage { get; set; }

        public decimal BadDebtProvision { get; set; }

        public decimal EnglandWithBadDebtProvision { get; set; }
        public decimal WalesWithBadDebtProvision { get; set; }
        public decimal ScotlandWithBadDebtProvision { get; set; }
        public decimal NorthernIrelandWithBadDebtProvision { get; set; }
    }
}