using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Builder.Summary.Common
{
    public static class CalcResultSummaryUtil
    {
        public static decimal GetTonnage(
            ILookup<(int, string?), TransformProducerReportedMaterial> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            string packagingType,
            RagRating? ragRating = null
        )
        {
            var prms = projectedMaterialsLookup[(producer.ProducerId, producer.SubsidiaryId)]
                .Where(p => p.MaterialId == material.Id && p.PackagingType == packagingType);

            return ragRating switch
            {
                null                   => prms.Sum(p => p.PackagingTonnage),
                RagRating.Red          => prms.Sum(p => p.PackagingTonnageRed ?? 0),
                RagRating.Amber        => prms.Sum(p => p.PackagingTonnageAmber ?? 0),
                RagRating.Green        => prms.Sum(p => p.PackagingTonnageGreen ?? 0),
                RagRating.RedMedical   => prms.Sum(p => p.PackagingTonnageRedMedical ?? 0),
                RagRating.AmberMedical => prms.Sum(p => p.PackagingTonnageAmberMedical ?? 0),
                RagRating.GreenMedical => prms.Sum(p => p.PackagingTonnageGreenMedical ?? 0),
                _                      => 0m
            };
        }

        public static decimal GetReportedTonnage(
            ILookup<(int, string?), TransformProducerReportedMaterial> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            RagRating? ragRating = null
        )
        {
            var householdTonnage = GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household, ragRating);
            var publicBinTonnage = GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin, ragRating);
            var glassTonnage = material.Code == MaterialCodes.Glass
                ? GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers, ragRating)
                : 0;

            return householdTonnage + publicBinTonnage + glassTonnage;
        }

        // Single-pass equivalent of calling GetReportedTonnage seven times with each RagRating and once without.
        public static (decimal R, decimal A, decimal G, decimal Total) GetReportedTonnagesByRag(
            ILookup<(int, string?), TransformProducerReportedMaterial> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material
        )
        {
            decimal r = 0, a = 0, g = 0, total = 0;

            foreach (var item in projectedMaterialsLookup[(producer.ProducerId, producer.SubsidiaryId)]
                .Where(p => p.MaterialId == material.Id && p.PackagingType != PackagingTypes.ConsumerWaste))
            {
                r     += (item.PackagingTonnageRed   ?? 0) + (item.PackagingTonnageRedMedical   ?? 0);
                a     += (item.PackagingTonnageAmber ?? 0) + (item.PackagingTonnageAmberMedical ?? 0);
                g     += (item.PackagingTonnageGreen ?? 0) + (item.PackagingTonnageGreenMedical ?? 0);
                total += item.PackagingTonnage;
            }

            return (r, a, g, total);
        }

        public static SelfManagedConsumerWasteData SumSelfManagedConsumerWasteData(
            IReadOnlyList<ProducerDetail> producersAndSubsidiaries,
            MaterialDetail material,
            SelfManagedConsumerWaste smcw
        ) =>
            smcw.ProducerTotals
                .Where(x => x.Level == 1 && producersAndSubsidiaries.Any(y => x.ProducerId == y.ProducerId))
                .Select(x => x.SelfManagedConsumerWasteDataPerMaterials[material.Code])
                .Single();

        public static (decimal? total, decimal? red,  decimal? amber, decimal? green) GetPricePerTonne(
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var laDisposalCostDataDetail = calcResult.CalcResultLaDisposalCostData.ByMaterial.GetValueOrDefault(material.Code);

            if (laDisposalCostDataDetail == null)
            {
                return (total: null, red: null, amber: null, green: null);
            }

            var total = laDisposalCostDataDetail.DisposalCostPricePerTonne ?? 0m;

            if (calcResult.CalcResultModulation is not null) {
                return (
                    total: total,
                    red:   calcResult.CalcResultModulation.MaterialModulation[material].RedMaterialDisposalCost,
                    amber: calcResult.CalcResultModulation.MaterialModulation[material].AmberMaterialDisposalCost,
                    green: calcResult.CalcResultModulation.MaterialModulation[material].GreenMaterialDisposalCost
                );
            } else {
                return (total: total, red: null, amber: null, green: null);
            }
        }

        public static (decimal? total, decimal? red,  decimal? amber, decimal? green) GetProducerDisposalFee(
            MaterialDetail material,
            CalcResult calcResult,
            SelfManagedConsumerWasteData smcw
        )
        {
            var pricePerTonne = GetPricePerTonne(material, calcResult);

            if (calcResult.CalcResultModulation is not null) {
                var red   = smcw.NetReportedTonnage.red   * pricePerTonne.red;
                var amber = smcw.NetReportedTonnage.amber * pricePerTonne.amber;
                var green = smcw.NetReportedTonnage.green * pricePerTonne.green;
                return (
                    total: red + amber + green,
                    red:   red,
                    amber: amber,
                    green: green
                );
            } else {
                var total = (smcw.NetReportedTonnage.total ?? 0) * (pricePerTonne.total ?? 0);
                return (total: total, red: null, amber: null, green: null);
            }
        }

        public static decimal GetBadDebtProvision(
            CalcResult calcResult,
            decimal? producerDisposalFeeTotal
        ) =>
           (producerDisposalFeeTotal ?? 0) * calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;

        public static ByCountryCost GetProducerDisposalFeeWithBadDebtProvision(
            CalcResult calcResult,
            decimal? producerDisposalFeeTotal
        )
        {
            var total = (producerDisposalFeeTotal ?? 0) * (1 + calcResult.CalcResultParameterOtherCost.BadDebtValue / 100);
            var countryApportionment = calcResult.CalcResultLapcapData.CountryApportionment;
            return total * countryApportionment;
        }

        public static decimal GetCommsCostHeaderWithoutBadDebtFor2bTitle(
            CalcResult calcResult
        ) => calcResult.CalcResultCommsCostReportDetail.CommsCostUkWide.Total;
    }
}
