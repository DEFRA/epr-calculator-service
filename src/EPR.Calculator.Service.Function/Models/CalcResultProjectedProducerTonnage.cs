namespace EPR.Calculator.Service.Function.Models
{
    public record RAMTonnage
    {
        public decimal Tonnage { get; set; }
        public decimal RedTonnage { get; set; }
        public decimal AmberTonnage { get; set; }
        public decimal GreenTonnage { get; set; }
        public decimal RedMedicalTonnage { get; set; }
        public decimal AmberMedicalTonnage { get; set; }
        public decimal GreenMedicalTonnage { get; set; }

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
        public required RAMTonnage HouseholdRAMTonnage { get; set; }
        public required RAMTonnage PublicBinRAMTonnage { get; set; }
        public RAMTonnage? HouseholdDrinksContainerRAMTonnage { get; set; }
        public required decimal HouseholdTonnageWithoutRAM { get; set; }
        public required decimal PublicBinTonnageWithoutRAM { get; set; }
        public decimal? HouseholdDrinksContainerTonnageWithoutRAM { get; set; }
        public required RAMTonnage ProjectedHouseholdRAMTonnage { get; set; }
        public required RAMTonnage ProjectedPublicBinRAMTonnage { get; set; }
        public RAMTonnage? ProjectedHouseholdDrinksContainerRAMTonnage { get; set; }
        public required decimal TotalTonnage { get; set; }

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
        public required RAMProportions H2RamProportions { get; set; }
        public required decimal H2TotalTonnage { get; set; }
    }
}
