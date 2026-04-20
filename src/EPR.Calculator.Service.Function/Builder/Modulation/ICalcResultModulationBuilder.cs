using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;

namespace EPR.Calculator.Service.Function.Builder.Modulation
{
    public interface ICalcResultModulationBuilder
    {
        Task<ModulationResult> ConstructAsync(
            CalcResultLaDisposalCostData laDisposalCostData,
            Dictionary<string, decimal> defaultParams
        );
    }

    public record MaterialModulation
    {
        public required decimal AmberMaterialDisposalCost { get; init; }
        public required decimal RedMaterialDisposalCost { get; init; }
        public required decimal GreenMaterialDisposalCost { get; init; }
        public required decimal RedMaterialTonnages { get; init; }
        public required decimal GreenMaterialTonnages { get; init; }
        public required decimal TotalRedMaterialAtAmberDisposalCost { get; init; }
        public required decimal TotalGreenMaterialAtAmberDisposalCost { get; init; }
    }

    public record ModulationResult
    {
        public required decimal GreenFactor { get; init; }
        public required decimal RedFactor { get; init; }
        public required Dictionary<string, MaterialModulation> MaterialModulation { get; init; }
    }
}
