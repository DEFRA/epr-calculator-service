using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{   
    public class CalcResult2ACommsDataByMaterial
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "2a Comms Costs - by Material";

        [JsonPropertyName("calcResult2aCommsDataDetails")]
        public required IEnumerable<CalcResult2ACommsDataDetails> CalcResult2aCommsDataDetails {  get; set; }

        [JsonPropertyName("calcResult2aCommsDataDetailsTotal")]
        public required CalcResult2ACommsDataDetailsTotal CalcResult2aCommsDataDetailsTotal { get; set; }
    }

    public class CalcResult2ACommsDataDetails : BaseLaDisposalcostAnd2ACommsData
    {
        [JsonPropertyName("materialName")]
        public required string MaterialName { get; init; }

        [JsonPropertyName("englandCommsCost")]
        public required string EnglandCommsCost { get; init; }

        [JsonPropertyName("walesCommsCost")]
        public required string WalesCommsCost { get; init; }

        [JsonPropertyName("scotlandCommsCost")]
        public required string ScotlandCommsCost { get; init; }

        [JsonPropertyName("northernIrelandCommsCost")]
        public required string NorthernIrelandCommsCost { get; init; }

        [JsonPropertyName("totalCommsCost")]
        public required string TotalCommsCost { get; init; }

        [JsonPropertyName("commsCostByMaterialPricePerTonne")]
        public required string CommsCostByMaterialPricePerTonne { get; init; }

    }
}
