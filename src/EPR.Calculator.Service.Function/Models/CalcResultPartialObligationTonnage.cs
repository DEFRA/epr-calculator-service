using EPR.Calculator.Service.Function.Utils;

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
            return partialRam != null ? partialRam.TotalRamTonnage() : MathUtils.RoundAwayFromZero(HouseholdTonnage * ObligatedFactor, 3);
        }
        public RAMTonnage? PartialHouseholdRAMTonnage()
        {
            return HouseholdRAMTonnage != null ? ToPartialRam(HouseholdRAMTonnage, ObligatedFactor) : null;
        }
        public decimal PartialPublicBinTonnage()
        {
            var partialRam = PartialPublicBinRAMTonnage();
            return partialRam != null ? partialRam.TotalRamTonnage() : MathUtils.RoundAwayFromZero(PublicBinTonnage * ObligatedFactor, 3);
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
            return HouseholdDrinksContainersTonnage != null ? MathUtils.RoundAwayFromZero(HouseholdDrinksContainersTonnage.Value * ObligatedFactor, 3) : null;
        }
        public RAMTonnage? PartialHouseholdDrinksContainersRAMTonnage()
        {
            return HouseholdDrinksContainersRAMTonnage != null ? ToPartialRam(HouseholdDrinksContainersRAMTonnage, ObligatedFactor) : null;
        }
        public decimal PartialSelfManagedConsumerWasteTonnage()
        {
            return MathUtils.RoundAwayFromZero(SelfManagedConsumerWasteTonnage * ObligatedFactor, 3);
        }
        public decimal TotalTonnage()
        {
            return HouseholdTonnage + PublicBinTonnage + (HouseholdDrinksContainersTonnage ?? 0);
        }
        public decimal PartialTotalTonnage()
        {
            return PartialHouseholdTonnage() + PartialPublicBinTonnage() + (PartialHouseholdDrinksContainersTonnage() ?? 0);
        }

         private RAMTonnage ToPartialRam(RAMTonnage ram, decimal partialAmount) {
            return new RAMTonnage {
                RedTonnage          = MathUtils.RoundAwayFromZero(ram.RedTonnage * partialAmount, 3),
                AmberTonnage        = MathUtils.RoundAwayFromZero(ram.AmberTonnage * partialAmount, 3),
                GreenTonnage        = MathUtils.RoundAwayFromZero(ram.GreenTonnage * partialAmount, 3),
                RedMedicalTonnage   = MathUtils.RoundAwayFromZero(ram.RedMedicalTonnage * partialAmount, 3),
                AmberMedicalTonnage = MathUtils.RoundAwayFromZero(ram.AmberMedicalTonnage * partialAmount, 3),
                GreenMedicalTonnage = MathUtils.RoundAwayFromZero(ram.GreenMedicalTonnage * partialAmount, 3),
            };
        }
    }
}

