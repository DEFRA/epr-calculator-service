using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Migrations;
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
        private record MaterialCost
        {
            public required string Material { get; init; }
            public required decimal PricePerTonne { get; init; }
            public required decimal BaseFee { get; init; }
            public required decimal AmberCost { get; init; }
            public required decimal RedCost { get; init; }
            public required decimal Difference { get; init; }
            public required decimal RedExcessiveCost { get; init; }
            public decimal GreenCost { get; set; }
        }

        public async Task<ModulationResult> ConstructAsync(
            CalcResultLaDisposalCostData laDisposalCostData,
            Dictionary<string, decimal> defaultParams
        )
        {
            var redFactor = defaultParams["REDM-RF"];

            var materials = (await materialService.GetMaterials()).Select(m => m.Name);// TODO move everything to use Code instead

            decimal pricePerTonne(string material)
            {
                var cost = laDisposalCostData.CalcResultLaDisposalCostDetails.First(ldc => ldc.Material == material).DisposalCostPricePerTonne;
                return decimal.Parse(cost!, CultureInfo.InvariantCulture);
            }

            decimal tonnage(string material, RagRating rag)
            {
                return laDisposalCostData.NetByMaterialAndRag[material][rag];
            }

            var materialCosts =
                materials.Select(material =>
                {
                    var amberDisposalCostPerTonne = pricePerTonne(material);
                    var greenTonnage = tonnage(material, RagRating.Green) + tonnage(material, RagRating.GreenMedical);
                    var amberTonnage = tonnage(material, RagRating.Amber) + tonnage(material, RagRating.AmberMedical);
                    var redTonnage   = tonnage(material, RagRating.Red)   + tonnage(material, RagRating.RedMedical);
                    var difference   = amberDisposalCostPerTonne * (redFactor - 1);

                    return new MaterialCost
                    {
                        Material         = material,
                        PricePerTonne    = amberDisposalCostPerTonne,
                        BaseFee          = amberDisposalCostPerTonne * greenTonnage,
                        AmberCost        = amberDisposalCostPerTonne * amberTonnage,
                        RedCost          = amberDisposalCostPerTonne * redFactor * redTonnage,
                        Difference       = difference,
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

            decimal to4dp(decimal d)
            {
                return Math.Round(d, 4);
            }

            return new ModulationResult
            {
                GreenTotal = materialCosts.Sum(c => c.BaseFee),
                AmberTotal = materialCosts.Sum(c => c.AmberCost),
                RedTotal   = materialCosts.Sum(c => c.RedCost),

                GreenFactor = greenFactor,
                RedFactor   = redFactor,
                MaterialNames = materials.ToList(),
                PricePerTonnePerMaterial = materials.ToDictionary(
                    material => material,
                    material =>
                    {
                        var basePrice = materialCosts.First(c => c.Material == material).PricePerTonne;
                        return Enum.GetValues<RagRating>().Cast<RagRating>().ToDictionary(
                            rag => rag,
                            rag => rag switch
                            {
                                var r when r == RagRating.Amber || r == RagRating.AmberMedical => to4dp(basePrice),
                                var r when r == RagRating.Red   || r == RagRating.RedMedical   => to4dp(basePrice * redFactor),
                                var r when r == RagRating.Green || r == RagRating.GreenMedical => to4dp(basePrice * greenFactor),
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
