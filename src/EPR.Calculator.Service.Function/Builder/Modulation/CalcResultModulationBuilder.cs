using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EPR.Calculator.Service.Function.Builder.Modulation
{
    [ExcludeFromCodeCoverage]
    public class CalcResultModulationBuilder(IMaterialService materialService)
        : ICalcResultModulationBuilder
    {
        private class MaterialCost
        {
            public required string Material { get; set; }
            public decimal PricePerTonne { get; set; }
            public decimal BaseFee { get; set; }
            public decimal AmberCost { get; set; }
            public decimal RedCost { get; set; }
            public decimal Difference { get; set; }
            public decimal RedExcessiveCost { get; set; }
            public decimal GreenCost { get; set; }
        }

        public async Task<ModulationResult> ConstructAsync(
            CalcResultsRequestDto resultsRequestDto,
            CalcResultLaDisposalCostData laDisposalCostData,
            Dictionary<string, decimal> defaultParams,
            List<ProducerData> producerData
        )
        {
            var runId = resultsRequestDto.RunId;

            var redFactor = defaultParams["REDM-RF"];

            // -----------
            var materials = (await materialService.GetMaterials()).Select(m => m.Name);// TODO move everything to use Code instead

            decimal pricePerTonne(string material)
            {
                var cost = laDisposalCostData.CalcResultLaDisposalCostDetails.First(ldc => ldc.Material == material).DisposalCostPricePerTonne;
                return decimal.Parse(cost!, CultureInfo.InvariantCulture);
            }

            decimal tonnage(string material, RagRating rag)
            {
                // TODO has this been added to LaDisposalCostData?
                // could work with this rather than ProducerData...
                var data = producerData.First(data => data.MaterialName == material);
                return rag switch
                {
                    RagRating.Red => data.TonnageRed,
                    RagRating.RedMedical => data.TonnageRedMedical,
                    RagRating.Amber => data.TonnageAmber,
                    RagRating.AmberMedical => data.TonnageAmberMedical,
                    RagRating.Green => data.TonnageGreen,
                    RagRating.GreenMedical => data.TonnageGreenMedical,
                    _ => 0m
                };
            }

            var materialCosts =
                materials.Select(material =>
                {
                    var basePrice = pricePerTonne(material);
                    var greenTonnage = tonnage(material, RagRating.Green) + tonnage(material, RagRating.GreenMedical);
                    var amberTonnage = tonnage(material, RagRating.Amber) + tonnage(material, RagRating.AmberMedical);
                    var redTonnage = tonnage(material, RagRating.Red) + tonnage(material, RagRating.RedMedical);
                    var difference = basePrice * redFactor - basePrice;

                    return new MaterialCost
                    {
                        Material = material,
                        PricePerTonne = basePrice,
                        BaseFee = basePrice * greenTonnage,
                        AmberCost = basePrice * amberTonnage,
                        RedCost = basePrice * redFactor * redTonnage,
                        Difference = difference,
                        RedExcessiveCost = redTonnage * difference
                    };
                });
            Console.WriteLine($">> {JsonConvert.SerializeObject(materialCosts, Formatting.Indented)}");


            var greenDiscount =
                materialCosts.Sum(c => c.RedExcessiveCost) / materialCosts.Sum(c => c.BaseFee);

            var greenFactor =
                (1 - greenDiscount);

            var updated =
                materialCosts.Select(material => material.GreenCost = material.AmberCost * greenDiscount);

            return new ModulationResult
            {
                // These are only used to assert test results, could remove?
                GreenTotal = materialCosts.Sum(c => c.BaseFee),
                AmberTotal = materialCosts.Sum(c => c.AmberCost),// This is the same as laDisposalCostData.CalcResultLaDisposalCostDetails.First(ldc => ldc.Material == material).DisposalCostPricePerTonne;. - Could pull out of the Total row and not sum!
                RedTotal = materialCosts.Sum(c => c.RedCost),

                GreenFactor = greenFactor,
                RedFactor = redFactor,
                MaterialNames = materials.ToList(),
                // Since this is just redFactor and greenFactor * basePrice, we could work with laDisposalCostData
                PricePerTonnePerMaterial = materials.ToDictionary(
                    material => material,
                    material =>
                    {
                        var basePrice = materialCosts.First(c => c.Material == material).PricePerTonne;
                        return Enum.GetValues<RagRating>().Cast<RagRating>().ToDictionary(
                            rag => rag,
                            rag => rag switch
                            {
                                var r when r == RagRating.Amber || r == RagRating.AmberMedical => basePrice,
                                var r when r == RagRating.Red || r == RagRating.RedMedical => basePrice * redFactor,
                                var r when r == RagRating.Green || r == RagRating.GreenMedical => basePrice * greenFactor,
                                _ => throw new InvalidOperationException($"Unexpected rag {rag}")
                            }
                        );
                    }
                ),
                CostPerMaterial = materials.ToDictionary(
                    material => material,
                    material =>
                    {
                        var materialCost = materialCosts.First(c => c.Material == material);
                        return Enum.GetValues<RagRating>().Cast<RagRating>().ToDictionary(
                          rag => rag,
                          rag => rag switch
                          {
                              var r when r == RagRating.Amber || r == RagRating.AmberMedical => materialCost.AmberCost,
                              var r when r == RagRating.Red || r == RagRating.RedMedical => materialCost.RedCost,
                              var r when r == RagRating.Green || r == RagRating.GreenMedical => materialCost.BaseFee * (1 - greenDiscount),
                              _ => throw new InvalidOperationException($"Unexpected rag {rag}")
                          }
                       );
                    }
               )
            };
        }
    }
}
