using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Modulation
{
    public interface ICalcResultModulationBuilder
    {
        Task<ModulationResult> ConstructAsync(
            IReadOnlyDictionary<string, decimal> defaultParams,
            List<MaterialDetail> materials,
            CalcResultLaDisposalCostData laDisposalCostData
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
        public required IReadOnlyDictionary<string, MaterialModulation> MaterialModulation { get; init; }
    }
}
