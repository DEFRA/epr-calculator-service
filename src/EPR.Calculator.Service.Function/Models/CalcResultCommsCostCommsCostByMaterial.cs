namespace EPR.Calculator.Service.Function.Models;

public class CalcResultCommsCostCommsCostByMaterial : CalcResultCommsCostOnePlusFourApportionment
{
    public required decimal ProducerReportedHouseholdPackagingWasteTonnage { get; set; }
    public required decimal LateReportingTonnage { get; set; }
    public decimal? CommsCostByMaterialPricePerTonne { get; set; }
    public required decimal ReportedPublicBinTonnage { get; set; }
    public decimal? HouseholdDrinksContainers { get; set; }
    public required decimal ProducerReportedTotalTonnage { get; set; }
    public decimal? TotalReportedTonnage { get; set; }
}
