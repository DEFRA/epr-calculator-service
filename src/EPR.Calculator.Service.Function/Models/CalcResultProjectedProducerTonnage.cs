namespace EPR.Calculator.Service.Function.Models
{
    public record RAMTonnage
    {
        public decimal Tonnage { get; init; }
        public decimal RedTonnage { get; init; }
        public decimal AmberTonnage { get; init; }
        public decimal GreenTonnage { get; init; }
        public decimal RedMedicalTonnage { get; init; }
        public decimal AmberMedicalTonnage { get; init; }
        public decimal GreenMedicalTonnage { get; init; }

        public decimal GetTotalRamTonnage()
        {
            return RedTonnage + RedMedicalTonnage + AmberTonnage + AmberMedicalTonnage + GreenTonnage + GreenMedicalTonnage;
        }
    }

    public record RAMProportions
    {
        public decimal Red { get; init; }
        public decimal Amber { get; init; }
        public decimal Green { get; init; }
        public decimal RedMedical { get; init; }
        public decimal AmberMedical { get; init; }
        public decimal GreenMedical { get; init; }
    }

    public abstract record CalcResultProjectedProducerMaterialTonnage
    {
        public required RAMTonnage HouseholdRAMTonnage { get; init; }
        public required RAMTonnage PublicBinRAMTonnage { get; init; }
        public RAMTonnage? HouseholdDrinksContainerRAMTonnage { get; init; }
        public required decimal HouseholdTonnageWithoutRAM { get; init; }
        public required decimal PublicBinTonnageWithoutRAM { get; init; }
        public decimal? HouseholdDrinksContainerTonnageWithoutRAM { get; init; }
        public required RAMTonnage ProjectedHouseholdRAMTonnage { get; init; }
        public required RAMTonnage ProjectedPublicBinRAMTonnage { get; init; }
        public RAMTonnage? ProjectedHouseholdDrinksContainerRAMTonnage { get; init; }
        public required decimal TotalTonnage { get; init; }

        private decimal GetTotalProjectedRamTonnage(Func<RAMTonnage, decimal> getTonnage)
        {
            var hdcTonnage = ProjectedHouseholdDrinksContainerRAMTonnage != null ? getTonnage(ProjectedHouseholdDrinksContainerRAMTonnage) : 0;
            return getTonnage(ProjectedHouseholdRAMTonnage) + getTonnage(ProjectedPublicBinRAMTonnage) + hdcTonnage;
        }
        public decimal GetTotalProjectedRedTonnage(){ return GetTotalProjectedRamTonnage(t => t.RedTonnage);}
        public decimal GetTotalProjectedAmberTonnage() { return GetTotalProjectedRamTonnage(t => t.AmberTonnage); }
        public decimal GetTotalProjectedGreenTonnage() { return GetTotalProjectedRamTonnage(t => t.GreenTonnage); }
        public decimal GetTotalProjectedRedMedicalTonnage() { return GetTotalProjectedRamTonnage(t => t.RedMedicalTonnage); }
        public decimal GetTotalProjectedAmberMedicalTonnage() { return GetTotalProjectedRamTonnage(t => t.AmberMedicalTonnage); }
        public decimal GetTotalProjectedGreenMedicalTonnage() { return GetTotalProjectedRamTonnage(t => t.GreenMedicalTonnage); }
    }

    public record CalcResultH2ProjectedProducerMaterialTonnage : CalcResultProjectedProducerMaterialTonnage {}

    public record CalcResultH1ProjectedProducerMaterialTonnage : CalcResultProjectedProducerMaterialTonnage
    {
        public required RAMProportions H2RamProportions { get; init; }
        public required decimal H2TotalTonnage { get; init; }
    }
}
