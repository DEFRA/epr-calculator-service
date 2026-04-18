using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace EPR.Calculator.Service.Function.Builder.Modulation
{
    [ExcludeFromCodeCoverage]
    public class CalcResultModulationBuilder : ICalcResultModulationBuilder
    {
        public async Task<ModulationResult> ConstructAsync(
            IReadOnlyDictionary<string, decimal> defaultParams,
            List<MaterialDetail> materials,
            CalcResultLaDisposalCostData laDisposalCostData,
            SelfManagedConsumerWaste smcw
        )
        {
            var redFactor = defaultParams["REDM-RF"];

            decimal pricePerTonne(MaterialDetail material)
            {
                var cost = laDisposalCostData.CalcResultLaDisposalCostDetails.First(ldc => ldc.Name == material.Name).DisposalCostPricePerTonne;
                return decimal.Parse(cost!.TrimStart('£'), CultureInfo.InvariantCulture);
            }

            var materialCosts =
                materials.Select(material =>
                {
                    var materialDisposalCost = pricePerTonne(material);
                    var netReportedTonnage = smcw.OverallTotalPerMaterials[material.Code].NetReportedTonnage;
                    return new
                    {
                        material = material,
                        amberMaterialDisposalCost = materialDisposalCost,
                        redMaterialDisposalCost   = materialDisposalCost * redFactor,
                        redMaterialTonnages   = netReportedTonnage.red   ?? 0m,
                        amberMaterialTonnages = netReportedTonnage.amber ?? 0m,
                        greenMaterialTonnages = netReportedTonnage.green ?? 0m
                    };
                });

            decimal to4dp(decimal d)
            {
                return Math.Round(d, 4);
            }

            var totalRedAtAmberDisposalCost  = materialCosts.Sum(c => c.amberMaterialDisposalCost * c.redMaterialTonnages);
            var totalGreenAtAmberDispoalCost = materialCosts.Sum(c => c.amberMaterialDisposalCost * c.greenMaterialTonnages);
            var greenDiscount =
                totalGreenAtAmberDispoalCost == 0
                    ? 0m // this is unlikely, but if happens then the green discount is moot
                    : (redFactor - 1) * totalRedAtAmberDisposalCost / totalGreenAtAmberDispoalCost;
            var greenFactor = Math.Round(1 - greenDiscount, 6);
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
                    };
                });

            return new ModulationResult
            {
                GreenFactor = greenFactor,
                RedFactor   = redFactor,
                MaterialModulation = materialModulations
            };
        }
    }
}
