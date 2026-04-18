using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Builder.Modulation
{
    public interface ICalcResultModulationBuilder
    {
        Task<ModulationResult> ConstructAsync(
            IReadOnlyDictionary<string, decimal> defaultParams,
            List<MaterialDetail> materials,
            CalcResultLaDisposalCostData laDisposalCostData,
            SelfManagedConsumerWaste smcw
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
        public required IReadOnlyDictionary<MaterialDetail, MaterialModulation> MaterialModulation { get; init; }
    }
}
