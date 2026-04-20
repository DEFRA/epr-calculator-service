using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;

namespace EPR.Calculator.Service.Function.Builder.Modulation
{
    public interface ICalcResultModulationBuilder
    {
        Task<ModulationResult> ConstructAsync(
            CalcResultsRequestDto resultsRequestDto,
            CalcResultLaDisposalCostData laDisposalCostData,
            Dictionary<string, decimal> defaultParams
        );
    }

    public record ModulationResult
    {
        public required decimal GreenTotal { get; init; }
        public required decimal AmberTotal { get; init; }
        public required decimal RedTotal { get; init; }
        public required decimal RedFactor { get; init; }
        public required decimal GreenFactor { get; init; }
        public required List<string> MaterialNames { get; init; } // TODO storing here temporarily - not currently passed to Exporters...
        public required Dictionary<string, Dictionary<RagRating, decimal>> PricePerTonnePerMaterial { get; init; }
        public required Dictionary<string, Dictionary<RagRating, decimal>> CostPerMaterial { get; init; }
    }
}
