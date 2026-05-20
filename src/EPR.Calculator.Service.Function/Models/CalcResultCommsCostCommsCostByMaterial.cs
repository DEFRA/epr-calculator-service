namespace EPR.Calculator.Service.Function.Models;

public class CalcResultCommsCostCommsCostByMaterial : CalcResultCommsCostOnePlusFourApportionment
{
    public required decimal HouseholdPackagingWasteTonnage { get; set; }
    public required decimal PublicBinTonnage { get; set; }
    public decimal? HouseholdDrinksContainersTonnage { get; set; }
    public required decimal LateReportingTonnage { get; set; }

    private decimal? totalTonnage;
    public decimal TotalTonnage =>
        totalTonnage ??=
            HouseholdPackagingWasteTonnage
            + LateReportingTonnage
            + PublicBinTonnage
            + (HouseholdDrinksContainersTonnage ?? 0);


    private decimal? pricePerTonne;
    public decimal PricePerTonne =>
        pricePerTonne ??=
            TotalTonnage != 0 ? TotalCost / TotalTonnage : 0;
}
