using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Constants;

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

        public static CalcResult2ACommsDataByMaterial From(IEnumerable<CalcResultCommsCostCommsCostByMaterial> commsCostByMaterial)
        {
            IEnumerable<CalcResult2ACommsDataDetails> GetMaterialBreakdown(IEnumerable<CalcResultCommsCostCommsCostByMaterial> commsCostByMaterial)
            {
                var commsByMaterialDataDetails = new List<CalcResult2ACommsDataDetails>();

                foreach (var item in commsCostByMaterial.Where(t => t.Name != CommonConstants.Total && t.Name != CommonConstants.TwoACommsCostsbyMaterial))
                {
                    commsByMaterialDataDetails.Add(CalcResult2ACommsDataDetails.From(item));
                }

                return commsByMaterialDataDetails;
            }

            return new CalcResult2ACommsDataByMaterial
            {
                CalcResult2aCommsDataDetails = GetMaterialBreakdown(commsCostByMaterial),
                CalcResult2aCommsDataDetailsTotal = CalcResult2ACommsDataDetailsTotal.From(commsCostByMaterial.Single(t => t.Name == CommonConstants.Total)),
            };
        }
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

        public static CalcResult2ACommsDataDetails From(CalcResultCommsCostCommsCostByMaterial materialCost)
        {
            return new CalcResult2ACommsDataDetails
            {
                MaterialName = materialCost.Name,
                ProducerHouseholdPackagingWasteTonnage = Math.Round(materialCost.ProducerReportedHouseholdPackagingWasteTonnageValue, 3),
                PublicBinTonnage = Math.Round(materialCost.ReportedPublicBinTonnageValue, 3),
                TotalTonnage = Math.Round(materialCost.ProducerReportedTotalTonnage, 3),
                HouseholdDrinksContainersTonnage = Math.Round(materialCost.HouseholdDrinksContainersValue, 3),
                CommsCostByMaterialPricePerTonne = $"£{materialCost.CommsCostByMaterialPricePerTonneValue.ToString("N4", CultureInfo.CreateSpecificCulture("en-GB"))}",
                EnglandCommsCost = CurrencyConverterUtils.ConvertToCurrency(materialCost.EnglandValue),
                WalesCommsCost = CurrencyConverterUtils.ConvertToCurrency(materialCost.WalesValue),
                ScotlandCommsCost = CurrencyConverterUtils.ConvertToCurrency(materialCost.ScotlandValue),
                NorthernIrelandCommsCost = CurrencyConverterUtils.ConvertToCurrency(materialCost.NorthernIrelandValue),
                TotalCommsCost = CurrencyConverterUtils.ConvertToCurrency(materialCost.TotalValue),
                LateReportingTonnage = Math.Round(materialCost.LateReportingTonnageValue, 3)
            };
        }
    }

    
}
