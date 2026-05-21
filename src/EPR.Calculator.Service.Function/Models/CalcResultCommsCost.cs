using System.Linq;
using EPR.Calculator.Service.Function.Models;
namespace EPR.Calculator.Service.Function.Models;

/// <summary>
/// The CommsCost report.
/// </summary>
public class CalcResultCommsCost
{
    public required ByCountryApportionment OnePlusFourApportionment { get; init; }

    public required Dictionary<string, CalcResultCommsCostCommsCostByMaterial> ByMaterial { get; init; }
        = [];

    private CalcResultCommsCostCommsCostByMaterial? total;
    public CalcResultCommsCostCommsCostByMaterial Total =>
        total ??=
            new CalcResultCommsCostCommsCostByMaterial
            {
                Cost                             = ByCountryCost.Sum(ByMaterial.Values.Select(v => v.Cost)),
                // TODO why do we sum up tonnage for different materials?
                HouseholdPackagingWasteTonnage   = ByMaterial.Values.Sum(v => v.HouseholdPackagingWasteTonnage),
                PublicBinTonnage                 = ByMaterial.Values.Sum(v => v.PublicBinTonnage),
                HouseholdDrinksContainersTonnage = ByMaterial.Values.Sum(v => v.HouseholdDrinksContainersTonnage ?? 0),
                LateReportingTonnage             = ByMaterial.Values.Sum(v => v.LateReportingTonnage)
            };

    public required ByCountryCost CommsCostUkWide { get; init; }

    public required ByCountryCost CommsCostByCountry { get; init; }
}

public class CalcResultCommsCostCommsCostByMaterial
{
    public required ByCountryCost Cost {get; set;}

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
            TotalTonnage != 0 ? Cost.Total / TotalTonnage : 0;
}
