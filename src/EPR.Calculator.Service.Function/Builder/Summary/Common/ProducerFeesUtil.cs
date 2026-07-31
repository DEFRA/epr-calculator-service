using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Constants;

namespace EPR.Calculator.Service.Function.Builder.Summary.Common
{
    public static class ProducerFeesUtil
    {
        public static decimal GetTonnage(
            ILookup<(int, string?), ProducerMaterialPackaging> projectedMaterialsLookup,
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
            ILookup<(int, string?), ProducerMaterialPackaging> projectedMaterialsLookup,
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
            ILookup<(int, string?), ProducerMaterialPackaging> projectedMaterialsLookup,
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
                .Select(x => x.SmcwByMaterial[material.Code])
                .Single();

        public static RamTonnageGroup GetPricePerTonne(
            MaterialDetail material,
            FeesState state
        )
        {
            var laDisposalCostDataDetail = state.DisposalCost.ByMaterial.GetValueOrDefault(material.Code);

            if (laDisposalCostDataDetail == null)
            {
                return new RamTonnageGroup();
            }

            var total = laDisposalCostDataDetail.DisposalCostPricePerTonne ?? 0m;

            if (state.Modulation is not null) {
                return new RamTonnageGroup {
                    Total = total,
                    Red = state.Modulation.ModulationByMaterial[material].RedMaterialDisposalCost,
                    Amber = state.Modulation.ModulationByMaterial[material].AmberMaterialDisposalCost,
                    Green = state.Modulation.ModulationByMaterial[material].GreenMaterialDisposalCost
                };
            } else {
                return new RamTonnageGroup { Total = total, Red = null, Amber = null, Green = null };
            }
        }

        public static RamTonnageGroup GetProducerDisposalFee(
            MaterialDetail material,
            FeesState state,
            SelfManagedConsumerWasteData smcw
        )
        {
            var pricePerTonne = GetPricePerTonne(material, state);

            if (state.Modulation is not null) {
                var red   = smcw.NetTonnage.Red   * pricePerTonne.Red;
                var amber = smcw.NetTonnage.Amber * pricePerTonne.Amber;
                var green = smcw.NetTonnage.Green * pricePerTonne.Green;

                return new RamTonnageGroup {
                    Total = red + amber + green,
                    Red = red,
                    Amber = amber,
                    Green = green
                };
            } else {
                var total = (smcw.NetTonnage.Total ?? 0) * (pricePerTonne.Total ?? 0);
                return new RamTonnageGroup { Total = total, Red = null, Amber = null, Green = null };
            }
        }

        public static decimal GetBadDebtProvision(
            FeesState state,
            decimal? producerDisposalFeeTotal
        ) =>
           (producerDisposalFeeTotal ?? 0) * state.OtherCost.BadDebtValue / 100;

        public static ByCountryCost GetProducerDisposalFeeWithBadDebtProvision(
            FeesState state,
            decimal? producerDisposalFeeTotal
        )
        {
            var total = (producerDisposalFeeTotal ?? 0) * (1 + state.OtherCost.BadDebtValue / 100);
            var countryApportionment = state.LapcapData.CountryApportionment;
            return total * countryApportionment;
        }

        public static decimal GetCommsCostHeaderWithoutBadDebtFor2bTitle(
            FeesState state
        ) => state.CommsCost.CommsCostUkWide.Total;
    }
}
