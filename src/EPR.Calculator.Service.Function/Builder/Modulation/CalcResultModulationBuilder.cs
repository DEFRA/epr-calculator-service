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
        public async Task<ModulationResult> ConstructAsync(
            CalcResultLaDisposalCostData laDisposalCostData,
            Dictionary<string, decimal> defaultParams
        )
        {
            var redFactor = defaultParams["REDM-RF"];

            var materials = (await materialService.GetMaterials()).Select(m => m.Name);// TODO move everything to use Code instead

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
                    var materialDisposalCost = pricePerTonne(material);
                    return new
                    {
                        material = material,
                        amberMaterialDisposalCost = materialDisposalCost,
                        redMaterialDisposalCost   = materialDisposalCost * redFactor,
                        redMaterialTonnages   = tonnage(material, RagRating.Red)   + tonnage(material, RagRating.RedMedical),
                        amberMaterialTonnages = tonnage(material, RagRating.Amber) + tonnage(material, RagRating.AmberMedical),
                        greenMaterialTonnages = tonnage(material, RagRating.Green) + tonnage(material, RagRating.GreenMedical)
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
                    material => material,
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
