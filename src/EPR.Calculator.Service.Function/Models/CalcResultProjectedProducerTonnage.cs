namespace EPR.Calculator.Service.Function.Models
{
    public class RAMTonnage
    {
        public decimal Tonnage { get; set; }
        public decimal RedTonnage { get; set; }
        public decimal AmberTonnage { get; set; }
        public decimal GreenTonnage { get; set; }
        public decimal RedMedicalTonnage { get; set; }
        public decimal AmberMedicalTonnage { get; set; }
        public decimal GreenMedicalTonnage { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is RAMTonnage other &&
                Tonnage == other.Tonnage &&
                RedTonnage == other.RedTonnage &&
                RedMedicalTonnage == other.RedMedicalTonnage &&
                AmberTonnage == other.AmberTonnage &&
                AmberMedicalTonnage == other.AmberMedicalTonnage &&
                GreenTonnage == other.GreenTonnage &&
                GreenMedicalTonnage == other.GreenMedicalTonnage;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Tonnage, RedTonnage, RedMedicalTonnage,
                                    AmberTonnage, AmberMedicalTonnage,
                                    GreenTonnage, GreenMedicalTonnage);
        }
    }

    public interface CalcResultProjectedProducerMaterialTonnage
    {
        public RAMTonnage HouseholdRAMTonnage { get; set; }
        public RAMTonnage PublicBinRAMTonnage { get; set; }
        public RAMTonnage? HouseholdDrinksContainerRAMTonnage { get; set; }
        public decimal TotalTonnage { get; set; }
    }

    public class CalcResultH2ProjectedProducerMaterialTonnage : CalcResultProjectedProducerMaterialTonnage
    {
        public required RAMTonnage HouseholdRAMTonnage { get; set; }
        public required RAMTonnage PublicBinRAMTonnage { get; set; }
        public RAMTonnage? HouseholdDrinksContainerRAMTonnage { get; set; }
        public decimal HouseholdTonnageDefaultedRed { get; set; }
        public decimal PublicBinTonnageDefaultedRed { get; set; }
        public decimal? HouseholdDrinksContainerDefaultedRed { get; set; }
        public decimal TotalTonnage { get; set; }
    }

    public class CalcResultH1ProjectedProducerMaterialTonnage : CalcResultProjectedProducerMaterialTonnage
    {
        public required RAMTonnage HouseholdRAMTonnage { get; set; }
        public required RAMTonnage PublicBinRAMTonnage { get; set; }
        public RAMTonnage? HouseholdDrinksContainerRAMTonnage { get; set; }
        public decimal HouseholdTonnageWithoutRAM { get; set; }
        public decimal PublicBinTonnageWithoutRAM { get; set; }
        public decimal? HouseholdDrinksContainerTonnageWithoutRAM { get; set; }
        public decimal RedH2Proportion { get; set; }
        public decimal AmberH2Proportion { get; set; }
        public decimal GreenH2Proportion { get; set; }
        public decimal RedMedicalH2Proportion { get; set; }
        public decimal AmberMedicalH2Proportion { get; set; }
        public decimal GreenMedicalH2Proportion { get; set; }
        public decimal TotalTonnage { get; set; }
        public decimal H2TotalTonnage { get; set; }
        public required RAMTonnage ProjectedHouseholdRAMTonnage { get; set; }
        public required RAMTonnage ProjectedPublicBinRAMTonnage { get; set; }
        public RAMTonnage? ProjectedHouseholdDrinksContainerRAMTonnage { get; set; }
    }
}
