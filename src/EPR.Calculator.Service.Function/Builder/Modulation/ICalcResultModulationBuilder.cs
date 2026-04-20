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
            Dictionary<string, decimal> defaultParams,
            List<ProducerData> producerData
        );
    }

    public class ModulationResult
    {
        public required decimal GreenTotal { get; set; }
        public required decimal AmberTotal { get; set; }
        public required decimal RedTotal { get; set; }
        public required decimal RedFactor { get; set; }
        public required decimal GreenFactor { get; set; }
        public required List<string> MaterialNames { get; set; } // TODO storing here temporarily - not currently passed to Exporters...
        public required Dictionary<string, Dictionary<RagRating, decimal>> PricePerTonnePerMaterial { get; set; }
        public required Dictionary<string, Dictionary<RagRating, decimal>> CostPerMaterial { get; set; }
    }
}
