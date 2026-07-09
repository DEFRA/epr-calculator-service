using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Builder.Modulation
{
    public interface ICalcResultModulationBuilder
    {
        Task<ModulationResult> ConstructAsync(
            IReadOnlyDictionary<string, decimal> defaultParams,
            IImmutableList<MaterialDetail> materials,
            CalcResultLaDisposalCostData laDisposalCostData,
            SelfManagedConsumerWaste smcw
        );
    }

    public class CalcResultModulationBuilder : ICalcResultModulationBuilder
    {
        public Task<ModulationResult> ConstructAsync(
            IReadOnlyDictionary<string, decimal> defaultParams,
            IImmutableList<MaterialDetail> materials,
            CalcResultLaDisposalCostData laDisposalCostData,
            SelfManagedConsumerWaste smcw
        )
        {
            var redFactor = defaultParams["REDM-RF"];

            decimal pricePerTonne(MaterialDetail material)
            {
                return laDisposalCostData.ByMaterial[material.Code].DisposalCostPricePerTonne ?? 0m;
            }

            var materialCosts =
                materials.Select(material =>
                {
                    var materialDisposalCost = pricePerTonne(material);
                    var netReportedTonnage = smcw.OverallTotalByMaterial[material.Code].SMCW.NetTonnage;
                    var lateReportingTonnageR = GetLateReportingTonnage(defaultParams, material, RagRating.Red);
                    var lateReportingTonnageA = GetLateReportingTonnage(defaultParams, material, RagRating.Amber);
                    var lateReportingTonnageG = GetLateReportingTonnage(defaultParams, material, RagRating.Green);
                    var redMaterialTonnages   = lateReportingTonnageR + netReportedTonnage.Red   ?? 0m;
                    var amberMaterialTonnages = lateReportingTonnageA + netReportedTonnage.Amber ?? 0m;
                    var greenMaterialTonnages = lateReportingTonnageG + netReportedTonnage.Green ?? 0m;
                    return new
                    {
                        material = material,
                        amberMaterialDisposalCost = materialDisposalCost,
                        redMaterialTonnages   = redMaterialTonnages,
                        amberMaterialTonnages = amberMaterialTonnages,
                        greenMaterialTonnages = greenMaterialTonnages,
                        redAtAmberDisposalCost   = MathUtils.RoundAwayFromZero(redMaterialTonnages   * materialDisposalCost, 2),
                        greenAtAmberDisposalCost = MathUtils.RoundAwayFromZero(greenMaterialTonnages * materialDisposalCost, 2)
                    };
                }).ToImmutableList();

            var totalRedAtAmberDisposalCost  = materialCosts.Sum(c => c.redAtAmberDisposalCost);
            var totalGreenAtAmberDispoalCost = materialCosts.Sum(c => c.greenAtAmberDisposalCost);
            var greenDiscount =
                totalGreenAtAmberDispoalCost == 0
                    ? 0m // this is unlikely, but if happens then the green discount is moot
                    : (redFactor - 1) * totalRedAtAmberDisposalCost / totalGreenAtAmberDispoalCost;
            var greenFactor = MathUtils.RoundAwayFromZero(1 - greenDiscount, 6);
            var materialModulations =
                materials.ToDictionary(
                    material => material,
                    material =>
                {
                    var cost = materialCosts.First(c => c.material == material);
                    return new MaterialModulation
                    {
                        MaterialDetail = material,
                        ModulationDetail = new ModulationDetail
                        {
                            RedMaterialDisposalCost   = MathUtils.RoundAwayFromZero(cost.amberMaterialDisposalCost * redFactor  , 4),
                            AmberMaterialDisposalCost = MathUtils.RoundAwayFromZero(cost.amberMaterialDisposalCost              , 4),
                            GreenMaterialDisposalCost = MathUtils.RoundAwayFromZero(cost.amberMaterialDisposalCost * greenFactor, 4),
                            RedMaterialTonnages       = cost.redMaterialTonnages,
                            AmberMaterialTonnages     = cost.amberMaterialTonnages,
                            GreenMaterialTonnages     = cost.greenMaterialTonnages,
                            TotalRedMaterialAtAmberDisposalCost   = cost.redAtAmberDisposalCost,
                            TotalGreenMaterialAtAmberDisposalCost = cost.greenAtAmberDisposalCost
                        }
                    };
                });

            return Task.FromResult(new ModulationResult
            {
                CalculatorRunId = smcw.CalculatorRunId,
                GreenFactor = greenFactor,
                RedFactor   = redFactor,
                ModulationByMaterial = materialModulations
            });
        }

        private decimal GetLateReportingTonnage(IReadOnlyDictionary<string, decimal> defaultParams, MaterialDetail material, RagRating ragRating)
        {
            var rag = ragRating switch
            {
                RagRating.Red   => "-R",
                RagRating.Amber => "",
                RagRating.Green => "-G",
                _ => throw new ArgumentException("Invalid RagRating")
            };
            return defaultParams[$"LRET-{material.Code}{rag}"];
        }
    }
}
