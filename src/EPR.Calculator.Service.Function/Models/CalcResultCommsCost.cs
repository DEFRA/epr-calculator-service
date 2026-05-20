using System.Linq;
namespace EPR.Calculator.Service.Function.Models
{
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
                    EnglandCost                      = ByMaterial.Values.Sum(x => x.EnglandCost),
                    WalesCost                        = ByMaterial.Values.Sum(x => x.WalesCost),
                    NorthernIrelandCost              = ByMaterial.Values.Sum(x => x.NorthernIrelandCost),
                    ScotlandCost                     = ByMaterial.Values.Sum(x => x.ScotlandCost),
                    HouseholdPackagingWasteTonnage   = ByMaterial.Values.Sum(v => v.HouseholdPackagingWasteTonnage),
                    PublicBinTonnage                 = ByMaterial.Values.Sum(v => v.PublicBinTonnage),
                    HouseholdDrinksContainersTonnage = ByMaterial.Values.Sum(v => v.HouseholdDrinksContainersTonnage ?? 0),
                    LateReportingTonnage             = ByMaterial.Values.Sum(v => v.LateReportingTonnage)
                };

        public required ByCountryCost CommsCostUkWide { get; init; }

        public required ByCountryCost CommsCostByCountry { get; init; }
    }
}
