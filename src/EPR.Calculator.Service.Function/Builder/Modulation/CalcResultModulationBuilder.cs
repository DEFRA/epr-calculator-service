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
    public class CalcResultModulationBuilder : ICalcResultModulationBuilder
    {
        public async Task<ModulationResult> ConstructAsync(
            Dictionary<string, decimal> defaultParams,
            List<MaterialDetail> materials,
            CalcResultLaDisposalCostData laDisposalCostData
        )
        {
            var redFactor = defaultParams["REDM-RF"];

            decimal pricePerTonne(string material)
            {
                // TODO the Material field is empty!? remove it or move material to the Material field...
                var cost = laDisposalCostData.CalcResultLaDisposalCostDetails.First(ldc => ldc.Name == material).DisposalCostPricePerTonne;
                return decimal.Parse(cost!.TrimStart('£'), CultureInfo.InvariantCulture);
            }

            decimal tonnage(string material, RagRating rag)
            {
                return laDisposalCostData.NetByMaterialAndRag[material][rag];
            }

            var materialCosts =
                materials.Select(material =>
                {
                    var materialName = material.Name; // TODO can we use Code instead
                    var materialDisposalCost = pricePerTonne(materialName);
                    return new
                    {
                        material = material,
                        amberMaterialDisposalCost = materialDisposalCost,
                        redMaterialDisposalCost   = materialDisposalCost * redFactor,
                        redMaterialTonnages   = tonnage(materialName, RagRating.Red)   + tonnage(materialName, RagRating.RedMedical),
                        amberMaterialTonnages = tonnage(materialName, RagRating.Amber) + tonnage(materialName, RagRating.AmberMedical),
                        greenMaterialTonnages = tonnage(materialName, RagRating.Green) + tonnage(materialName, RagRating.GreenMedical)
                    };
                });

            decimal to4dp(decimal d)
            {
                return Math.Round(d, 4);
            }

            var greenDiscount = (redFactor - 1) * materialCosts.Sum(c => c.amberMaterialDisposalCost * c.redMaterialTonnages) / materialCosts.Sum(c => c.amberMaterialDisposalCost * c.greenMaterialTonnages);
            Console.WriteLine($"# greenDiscount: {greenDiscount}");
            var greenFactor = 1 - greenDiscount;
            Console.WriteLine($"# greenFactor: {greenFactor}");
            var materialModulations =
                materials.ToDictionary(
                    material => material.Name,
                    material =>
            {
                var cost = materialCosts.First(c => c.material == material);
                var greenMaterialDisposalCost = greenFactor * cost.amberMaterialDisposalCost;

                return new MaterialModulation
                {
                    AmberMaterialDisposalCost = to4dp(cost.amberMaterialDisposalCost),
                    RedMaterialDisposalCost   = to4dp(cost.redMaterialDisposalCost),
                    GreenMaterialDisposalCost = to4dp(greenMaterialDisposalCost),
                    RedMaterialTonnages       = cost.redMaterialTonnages,
                    GreenMaterialTonnages     = cost.greenMaterialTonnages,
                    TotalRedMaterialAtAmberDisposalCost   = cost.redMaterialTonnages   * cost.amberMaterialDisposalCost,
                    TotalGreenMaterialAtAmberDisposalCost = cost.greenMaterialTonnages * cost.amberMaterialDisposalCost,
                    // TODO confirm we don't need the following?
                    //AmberCostPerMaterial = c.amberMaterialTonnages * c.amberMaterialDisposalCost,
                    //RedCostPerMaterial   = c.redMaterialTonnages   * c.amberMaterialDisposalCost * redFactor,
                    //GreenCostPerMaterial = c.greenMaterialTonnages * c.amberMaterialDisposalCost * greenFactor
                };
            });
            Console.WriteLine($">> {JsonConvert.SerializeObject(materialCosts, Formatting.Indented)}");


            return new ModulationResult
            {
                GreenFactor = greenFactor,
                RedFactor   = redFactor,
                MaterialModulation = materialModulations
            };
        }
    }
}
