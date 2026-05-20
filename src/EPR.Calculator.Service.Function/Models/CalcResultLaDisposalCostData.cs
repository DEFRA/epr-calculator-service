namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultLaDisposalCostData
    {
        public required Dictionary<string, CalcResultLaDisposalCostDataDetail> ByMaterial { get; init; }

        private CalcResultLaDisposalCostDataDetail? total;
        public CalcResultLaDisposalCostDataDetail Total =>
            total ??=
                new CalcResultLaDisposalCostDataDetail
                {
                    EnglandCost         = ByMaterial.Values.Sum(v => v.EnglandCost),
                    WalesCost           = ByMaterial.Values.Sum(v => v.WalesCost),
                    ScotlandCost        = ByMaterial.Values.Sum(v => v.ScotlandCost),
                    NorthernIrelandCost = ByMaterial.Values.Sum(v => v.NorthernIrelandCost),

                    // TODO why do we sum up tonnage for different materials?
                    HouseholdPackagingWasteTonnage          = ByMaterial.Values.Sum(v => v.HouseholdPackagingWasteTonnage),
                    PublicBinTonnage                        = ByMaterial.Values.Sum(v => v.PublicBinTonnage),
                    HouseholdDrinkContainersTonnage         = ByMaterial.Values.Sum(v => v.HouseholdDrinkContainersTonnage ?? 0),
                    LateReportingTonnage                    = ByMaterial.Values.Sum(v => v.LateReportingTonnage ?? 0),
                    // TODO this used to be null when not applyModulation
                    // For modulation, can we
                    ActionedSelfManagedConsumerWasteTonnage = ByMaterial.Values.Sum(v => v.ActionedSelfManagedConsumerWasteTonnage ?? 0),

                    TotalTonnage = ByMaterial.Values.Sum(v => v.TotalTonnage),
                    DisposalCostPricePerTonne = null
                };
    }
}
