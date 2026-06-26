using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultPartialObligationTonnage
    {
        public required decimal ObligatedFactor { get; set; }
        public required decimal HouseholdTonnage { get; set; }
        public RAMTonnage? HouseholdRAMTonnage { get; set; }
        public required decimal PublicBinTonnage { get; set; }
        public RAMTonnage? PublicBinRAMTonnage { get; set; }
        public decimal? HouseholdDrinksContainersTonnage { get; set; }
        public RAMTonnage? HouseholdDrinksContainersRAMTonnage { get; set; }
        public required decimal SelfManagedConsumerWasteTonnage { get; set; }
        public decimal PartialHouseholdTonnage()
        {
            var partialRam = PartialHouseholdRAMTonnage();
            return partialRam != null ? partialRam.TotalRamTonnage() : Math.Round(HouseholdTonnage * ObligatedFactor, 3);
        }
        public RAMTonnage? PartialHouseholdRAMTonnage()
        {
            return HouseholdRAMTonnage != null ? ToPartialRam(HouseholdRAMTonnage, ObligatedFactor) : null;
        }
        public decimal PartialPublicBinTonnage()
        {
            var partialRam = PartialPublicBinRAMTonnage();
            return partialRam != null ? partialRam.TotalRamTonnage() : Math.Round(PublicBinTonnage * ObligatedFactor, 3);
        }
        public RAMTonnage? PartialPublicBinRAMTonnage()
        {
            return PublicBinRAMTonnage != null ? ToPartialRam(PublicBinRAMTonnage, ObligatedFactor) : null;
        }
        public decimal? PartialHouseholdDrinksContainersTonnage()
        {
            var partialRam = PartialHouseholdDrinksContainersRAMTonnage();
            if (partialRam != null)
            {
                return partialRam.TotalRamTonnage();
            }
            return HouseholdDrinksContainersTonnage != null ? Math.Round(HouseholdDrinksContainersTonnage.Value * ObligatedFactor, 3) : null;
        }
        public RAMTonnage? PartialHouseholdDrinksContainersRAMTonnage()
        {
            return HouseholdDrinksContainersRAMTonnage != null ? ToPartialRam(HouseholdDrinksContainersRAMTonnage, ObligatedFactor) : null;
        }
        public decimal PartialSelfManagedConsumerWasteTonnage()
        {
            return Math.Round(SelfManagedConsumerWasteTonnage * ObligatedFactor, 3);
        }
        public decimal TotalTonnage()
        {
            return HouseholdTonnage + PublicBinTonnage + (HouseholdDrinksContainersTonnage ?? 0);
        }
        public decimal PartialTotalTonnage()
        {
            return PartialHouseholdTonnage() + PartialPublicBinTonnage() + (PartialHouseholdDrinksContainersTonnage() ?? 0);
        }

        private RAMTonnage ToPartialRam(RAMTonnage ram, decimal partialAmount)
        {
            return new RAMTonnage
            {
                Red        = Math.Round(ram.Red * partialAmount, 3),
                Amber      = Math.Round(ram.Amber * partialAmount, 3),
                Green      = Math.Round(ram.Green * partialAmount, 3),
                RedMedical = Math.Round(ram.RedMedical * partialAmount, 3),
                AmberMedical = Math.Round(ram.AmberMedical * partialAmount, 3),
                GreenMedical = Math.Round(ram.GreenMedical * partialAmount, 3)
            };
        }
    }
}

