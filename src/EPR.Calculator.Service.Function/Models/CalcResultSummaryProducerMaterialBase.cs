using EPR.Calculator.API.Data.Enums;

namespace EPR.Calculator.Service.Function.Models;

public abstract class CalcResultSummaryProducerMaterialBase
{
    public decimal HouseholdPackagingWasteTonnage { get; set; }

    public Dictionary<RagRating, decimal> HouseholdPackagingWasteTonnageRagRating { get; set; } = new();

    public decimal PublicBinTonnage { get; set; }

    public Dictionary<RagRating, decimal> PublicBinTonnageRagRating { get; set; } = new();

    public decimal HouseholdDrinksContainersTonnage { get; set; }

    public Dictionary<RagRating, decimal> HouseholdDrinksContainersTonnageRagRating { get; set; } = new();

    public decimal TotalReportedTonnage { get; set; }

    public decimal BadDebtProvision { get; set; }

    public decimal EnglandWithBadDebtProvision { get; set; }
    public decimal WalesWithBadDebtProvision { get; set; }
    public decimal ScotlandWithBadDebtProvision { get; set; }
    public decimal NorthernIrelandWithBadDebtProvision { get; set; }
}