using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{

    public interface ISelfManagedConsumerWasteService
    {
        Task<SelfManagedConsumerWaste>
         Calculate(
            CalcResultsRequestDto resultsRequestDto,
            IEnumerable<MaterialDetail> materialDetails,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations,
            bool showModulations);
    }

    public class SelfManagedConsumerWasteService: ISelfManagedConsumerWasteService
    {
        private readonly ApplicationDBContext context;

        public SelfManagedConsumerWasteService(ApplicationDBContext context)
        {
            this.context = context;
        }

        private async Task<IEnumerable<ProducerDetail>> GetProducerDetails(int runId)
        {
            return await context
                .ProducerDetail
                .Where(pd => pd.CalculatorRunId == runId)
                .OrderBy(x => x.ProducerId)
                .ThenBy(x => x.SubsidiaryId)
                .ToListAsync();
        }

        public async Task<SelfManagedConsumerWaste> Calculate(
            CalcResultsRequestDto resultsRequestDto,
            IEnumerable<MaterialDetail> materialDetails,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations,
            bool showModulations)
        {
            var producerTotals = (await GetProducerDetails(resultsRequestDto.RunId))
                .GroupBy(x => x.ProducerId)
                .SelectMany(group => group.Select((producerDetail, index) =>
                {
                    var level = index == 0 ? 1 : 2;
                    return new ProducerSelfManagedConsumerWaste
                    {
                        producerDetail = producerDetail,
                        Level = level,
                        SelfManagedConsumerWasteDataPerMaterials =
                            materialDetails.ToDictionary(
                                material => material.Code,
                                material => {
                                    var smcw         = level == 1
                                                         ? group.Sum(p => GetTonnage(p, material, PackagingTypes.ConsumerWaste, scaledUpProducers, partialObligations))
                                                         : GetTonnage(producerDetail, material, PackagingTypes.ConsumerWaste, scaledUpProducers, partialObligations);
                                    var totalTonnage = level == 1
                                                         ? group.Sum(p => GetReportedTonnage(p, material, scaledUpProducers, partialObligations))
                                                         : GetReportedTonnage(producerDetail, material, scaledUpProducers, partialObligations);

                                    return new SelfManagedConsumerWasteData
                                    {
                                        SelfManagedConsumerWasteTonnage = smcw,
                                        ActionedSelfManagedConsumerWasteTonnage = GetActionedSelfManagedConsumerWasteTonnage(totalReportedTonnage: totalTonnage, selfManagedConsumerWasteTonnage: smcw, level: level),
                                        NetReportedTonnage = GetNetReportedTonnage(group, material, scaledUpProducers, partialObligations, showModulations, level)
                                    };
                                }
                            )
                    };
                })).ToList();

            return new SelfManagedConsumerWaste
            {
                ProducerTotals = producerTotals,
                OverallTotalPerMaterials =
                    materialDetails.ToDictionary(
                        material => material.Code,
                        material => producerTotals
                                        .Where(x => x.Level == 1)
                                        .Select(x => x.SelfManagedConsumerWasteDataPerMaterials[material.Code])
                                        .Sum()
                    )
            };
        }

        public static decimal? GetScaledUpTonnage(ProducerDetail producer, MaterialDetail material, string packagingType, IEnumerable<CalcResultScaledupProducer> scaledUpProducers)
        {
            var scaledupProducerForAllSubmissionPeriods = scaledUpProducers.Where(p => p.ProducerId == producer.ProducerId
                && p.SubsidiaryId == producer.SubsidiaryId
                && !p.IsSubtotalRow
                && !p.IsTotalRow);

            if (scaledupProducerForAllSubmissionPeriods.Any())
            {
                decimal tonnage = 0;
                foreach (var scaledupProducerTonnageByMaterial in scaledupProducerForAllSubmissionPeriods.Select(x => x.ScaledupProducerTonnageByMaterial))
                {
                    switch (packagingType)
                    {
                        case PackagingTypes.Household:
                            tonnage += scaledupProducerTonnageByMaterial.GetValueOrDefault(material.Code)?.ScaledupReportedHouseholdPackagingWasteTonnage ?? 0;
                            break;
                        case PackagingTypes.PublicBin:
                            tonnage += scaledupProducerTonnageByMaterial.GetValueOrDefault(material.Code)?.ScaledupReportedPublicBinTonnage ?? 0;
                            break;
                        case PackagingTypes.ConsumerWaste:
                            tonnage += scaledupProducerTonnageByMaterial.GetValueOrDefault(material.Code)?.ScaledupReportedSelfManagedConsumerWasteTonnage ?? 0;
                            break;
                        case PackagingTypes.HouseholdDrinksContainers:
                            tonnage += scaledupProducerTonnageByMaterial.GetValueOrDefault(material.Code)?.ScaledupHouseholdDrinksContainersTonnageGlass ?? 0;
                            break;
                        default:
                            tonnage += 0;
                            break;
                    }
                }

                return tonnage;
            }
            return null;
        }

        public static decimal? GetPartialTonnage(ProducerDetail producer, MaterialDetail material, string packagingType, IEnumerable<CalcResultPartialObligation> partialObligations)
        {
            var maybePartialObligation = partialObligations.FirstOrDefault(p => p.ProducerId == producer.ProducerId && p.SubsidiaryId == producer.SubsidiaryId);

            if (maybePartialObligation != null)
            {
                switch (packagingType)
                {
                    case PackagingTypes.Household:
                        return maybePartialObligation.PartialObligationTonnageByMaterial.GetValueOrDefault(material.Code)?.PartialReportedHouseholdPackagingWasteTonnage ?? 0;
                    case PackagingTypes.PublicBin:
                        return maybePartialObligation.PartialObligationTonnageByMaterial.GetValueOrDefault(material.Code)?.PartialReportedPublicBinTonnage ?? 0;
                    case PackagingTypes.ConsumerWaste:
                        return maybePartialObligation.PartialObligationTonnageByMaterial.GetValueOrDefault(material.Code)?.PartialReportedSelfManagedConsumerWasteTonnage ?? 0;
                    case PackagingTypes.HouseholdDrinksContainers:
                        return maybePartialObligation.PartialObligationTonnageByMaterial.GetValueOrDefault(material.Code)?.PartialHouseholdDrinksContainersTonnageGlass ?? 0;
                    default:
                        return 0;
                }
            }
            return null;
        }

        private static decimal GetTonnage(
            ProducerDetail producer,
            MaterialDetail material,
            string packagingType,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations,
            RagRating? ragRating = null)
        {
            var maybePartialScaledUpTonnage = GetPartialTonnage(producer, material, packagingType, partialObligations);

            if (maybePartialScaledUpTonnage != null) {
                return (decimal)maybePartialScaledUpTonnage;
            }

            var maybeScaledUpTonnage = GetScaledUpTonnage(producer, material, packagingType, scaledUpProducers);

            if (maybeScaledUpTonnage != null) {
                return (decimal)maybeScaledUpTonnage;
            }

            var prms = producer.ProducerReportedMaterials.Where(p => p.Material?.Code == material.Code && p.PackagingType == packagingType);

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

        private static decimal GetReportedTonnage(
            ProducerDetail producer,
            MaterialDetail material,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations,
            RagRating? ragRating = null)
        {


            var householdTonnage = GetTonnage(producer, material, PackagingTypes.Household, scaledUpProducers, partialObligations, ragRating);
            var publicBinTonnage = GetTonnage(producer, material, PackagingTypes.PublicBin, scaledUpProducers, partialObligations, ragRating);
            var glassTonnage = material.Code == MaterialCodes.Glass
                ? GetTonnage(producer, material, PackagingTypes.HouseholdDrinksContainers, scaledUpProducers, partialObligations, ragRating)
                : 0;

            return householdTonnage + publicBinTonnage + glassTonnage;
        }

        internal static (decimal? total, decimal? red,  decimal? amber, decimal? green) GetNetReportedTonnage(
            IEnumerable<ProducerDetail> producerAndSubsidiaries,
            MaterialDetail material,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations,
            bool showModulations,
            int level)
        {
            var reportedTonnage = producerAndSubsidiaries.Sum(producer => GetReportedTonnage(producer, material, scaledUpProducers, partialObligations));
            var toSubtract = producerAndSubsidiaries.Sum(producer => GetTonnage(producer, material, PackagingTypes.ConsumerWaste, scaledUpProducers, partialObligations));
            var total = reportedTonnage - toSubtract;

            if (showModulations)
            {
                if (level == CommonConstants.LevelTwo)
                    return (total: null, red: null, amber: null, green: null);

                var red   = producerAndSubsidiaries.Sum(producer => GetReportedTonnage(producer, material, scaledUpProducers, partialObligations, RagRating.Red)) +
                            producerAndSubsidiaries.Sum(producer => GetReportedTonnage(producer, material, scaledUpProducers, partialObligations, RagRating.RedMedical));
                var amber = producerAndSubsidiaries.Sum(producer => GetReportedTonnage(producer, material, scaledUpProducers, partialObligations, RagRating.Amber)) +
                            producerAndSubsidiaries.Sum(producer => GetReportedTonnage(producer, material, scaledUpProducers, partialObligations, RagRating.AmberMedical));
                var green = producerAndSubsidiaries.Sum(producer => GetReportedTonnage(producer, material, scaledUpProducers, partialObligations, RagRating.Green)) +
                            producerAndSubsidiaries.Sum(producer => GetReportedTonnage(producer, material, scaledUpProducers, partialObligations, RagRating.GreenMedical));

                return (
                    total: Math.Max(total, 0),
                    red  : Math.Max(red - Math.Max(toSubtract - amber, 0), 0),
                    amber: Math.Max(amber - toSubtract, 0),
                    green: Math.Max(green - Math.Max(toSubtract - amber - red, 0), 0)
                );
            } else {
                if (level == CommonConstants.LevelTwo)
                    return (total: total, red: null, amber: null, green: null);

                return (total: Math.Max(total, 0), red: null, amber: null, green: null);
            }
        }

        private static decimal? GetActionedSelfManagedConsumerWasteTonnage(
            decimal totalReportedTonnage,
            decimal selfManagedConsumerWasteTonnage,
            int level)
        {
            return level == CommonConstants.LevelOne
                ? Math.Min(totalReportedTonnage, selfManagedConsumerWasteTonnage):
                null;
        }
    }

    public record SelfManagedConsumerWaste
    {
        public required List<ProducerSelfManagedConsumerWaste> ProducerTotals { get; init; }
        public required Dictionary<string, SelfManagedConsumerWasteData> OverallTotalPerMaterials { get; init; }
    }

    public record ProducerSelfManagedConsumerWaste
    {
        public required ProducerDetail producerDetail { get; init; }
        public required int Level {get; init; }
        public required Dictionary<string, SelfManagedConsumerWasteData> SelfManagedConsumerWasteDataPerMaterials { get; init; }
    }

    public record SelfManagedConsumerWasteData
    {
        public required decimal SelfManagedConsumerWasteTonnage { get; init; }
        public required decimal? ActionedSelfManagedConsumerWasteTonnage { get; init; }
        public required (decimal? total, decimal? red, decimal? amber, decimal? green) NetReportedTonnage { get; init; }

        public static SelfManagedConsumerWasteData Zero => new()
        {
            SelfManagedConsumerWasteTonnage = 0,
            ActionedSelfManagedConsumerWasteTonnage = 0,
            NetReportedTonnage = (0, 0, 0, 0)
        };

        public static SelfManagedConsumerWasteData operator +(
            SelfManagedConsumerWasteData a,
            SelfManagedConsumerWasteData b)
        {
            return new SelfManagedConsumerWasteData
            {
                SelfManagedConsumerWasteTonnage =
                    a.SelfManagedConsumerWasteTonnage + b.SelfManagedConsumerWasteTonnage,

                ActionedSelfManagedConsumerWasteTonnage =
                    (a.ActionedSelfManagedConsumerWasteTonnage ?? 0) +
                    (b.ActionedSelfManagedConsumerWasteTonnage ?? 0),

                NetReportedTonnage = (
                    (a.NetReportedTonnage.total ?? 0) + (b.NetReportedTonnage.total ?? 0),
                    (a.NetReportedTonnage.red   ?? 0) + (b.NetReportedTonnage.red   ?? 0),
                    (a.NetReportedTonnage.amber ?? 0) + (b.NetReportedTonnage.amber ?? 0),
                    (a.NetReportedTonnage.green ?? 0) + (b.NetReportedTonnage.green ?? 0)
                )
            };
        }
    }

    public static class SelfManagedConsumerWasteDataExtensions
    {
        public static SelfManagedConsumerWasteData Sum(
            this IEnumerable<SelfManagedConsumerWasteData?> source)
        {
            return source.Aggregate(
                SelfManagedConsumerWasteData.Zero,
                (acc, x) => acc + (x ?? SelfManagedConsumerWasteData.Zero)
            );
        }
    }
}