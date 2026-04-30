using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Utilities;

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
                .ProducerDetail.AsNoTracking()
                .Where(pd => pd.CalculatorRunId == runId)
                .Include(pd => pd.ProducerReportedMaterials)
                .ThenInclude(prm => prm.Material)
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
                .SelectMany(group =>
                    materialDetails
                        .SelectMany(material =>
                            SelfManagedConsumerWasteServiceLevels
                                .Calculate(BuildL1(group, material, scaledUpProducers, partialObligations), showModulations)
                                .Select(r => (material, result: r))
                        )
                        .GroupBy(x => (x.result.OrgId, x.result.SubsidiaryId, x.result.Level))
                        .Select(g =>
                            new ProducerSelfManagedConsumerWaste
                            {
                                ProducerId                               = g.Key.OrgId,
                                SubsidiaryId                             = g.Key.SubsidiaryId,
                                Level                                    = g.Key.Level,
                                SelfManagedConsumerWasteDataPerMaterials = g.ToDictionary(x => x.material.Code, x => MapResultToData(x.result))
                            }
                        )
                )
                .ToList();

            return new SelfManagedConsumerWaste
            {
                ProducerTotals  = producerTotals,
                OverallTotalPerMaterials = materialDetails.ToDictionary(
                    m => m.Code,
                    m => producerTotals
                        .Where(x => x.Level == 1)
                        .Select(x => x.SelfManagedConsumerWasteDataPerMaterials.GetValueOrDefault(m.Code))
                        .Sum()
                )
            };
        }

        private IL1 BuildL1(
            IGrouping<int, ProducerDetail> group,
            MaterialDetail material,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations)
        {
            if (group.Count() == 1 && group.First().SubsidiaryId == null)
            {
                var p = group.First();

                return new SingleL1(
                    OrgId: p.ProducerId,
                    R:     CalcResultSummaryUtil.GetReportedTonnage(p, material, scaledUpProducers, partialObligations, RagRating.Red) +
                           CalcResultSummaryUtil.GetReportedTonnage(p, material, scaledUpProducers, partialObligations, RagRating.RedMedical),
                    A:     CalcResultSummaryUtil.GetReportedTonnage(p, material, scaledUpProducers, partialObligations, RagRating.Amber) +
                           CalcResultSummaryUtil.GetReportedTonnage(p, material, scaledUpProducers, partialObligations, RagRating.AmberMedical),
                    G:     CalcResultSummaryUtil.GetReportedTonnage(p, material, scaledUpProducers, partialObligations, RagRating.Green) +
                           CalcResultSummaryUtil.GetReportedTonnage(p, material, scaledUpProducers, partialObligations, RagRating.GreenMedical),
                    Total: CalcResultSummaryUtil.GetReportedTonnage(p, material, scaledUpProducers, partialObligations),
                    Smcw:  CalcResultSummaryUtil.GetTonnage(p, material, PackagingTypes.ConsumerWaste, scaledUpProducers, partialObligations)
                );
            }

            var l2s = group
                .OrderBy(p => p.ProducerId)
                .ThenBy(p => p.SubsidiaryId)
                .Select(p => new L2(
                    OrgId:        p.ProducerId,
                    SubsidiaryId: p.SubsidiaryId,
                    R:            CalcResultSummaryUtil.GetReportedTonnage(p, material, scaledUpProducers, partialObligations, RagRating.Red) +
                                  CalcResultSummaryUtil.GetReportedTonnage(p, material, scaledUpProducers, partialObligations, RagRating.RedMedical),
                    A:            CalcResultSummaryUtil.GetReportedTonnage(p, material, scaledUpProducers, partialObligations, RagRating.Amber) +
                                  CalcResultSummaryUtil.GetReportedTonnage(p, material, scaledUpProducers, partialObligations, RagRating.AmberMedical),
                    G:            CalcResultSummaryUtil.GetReportedTonnage(p, material, scaledUpProducers, partialObligations, RagRating.Green) +
                                  CalcResultSummaryUtil.GetReportedTonnage(p, material, scaledUpProducers, partialObligations, RagRating.GreenMedical),
                    Total:        CalcResultSummaryUtil.GetReportedTonnage(p, material, scaledUpProducers, partialObligations),
                    Smcw:         CalcResultSummaryUtil.GetTonnage(p, material, PackagingTypes.ConsumerWaste, scaledUpProducers, partialObligations)
                )).ToList();

            return new HC(group.Key, l2s);
        }

        private static SelfManagedConsumerWasteData MapResultToData(Result r)
        {
            return new SelfManagedConsumerWasteData
            {
                SelfManagedConsumerWasteTonnage         = r.Smcw,
                ActionedSelfManagedConsumerWasteTonnage = r.ActionedSmcwR + r.ActionedSmcwA + r.ActionedSmcwG,
                ResidualSelfManagedConsumerWasteTonnage = r.Residual,
                NetReportedTonnage = (
                    total: r.NetTotal,
                    red  : r.NetR,
                    amber: r.NetA,
                    green: r.NetG
                )
            };
        }
    }

    public record SelfManagedConsumerWaste
    {
        public required List<ProducerSelfManagedConsumerWaste> ProducerTotals { get; init; }
        public required Dictionary<string, SelfManagedConsumerWasteData> OverallTotalPerMaterials { get; init; }
    }

    public record ProducerSelfManagedConsumerWaste
    {
        public int ProducerId { get; set; }
        public string? SubsidiaryId { get; set; }
        public required int Level {get; init; }
        public required Dictionary<string, SelfManagedConsumerWasteData> SelfManagedConsumerWasteDataPerMaterials { get; init; }
    }

    public record SelfManagedConsumerWasteData
    {
        public required decimal SelfManagedConsumerWasteTonnage { get; init; }
        public required decimal? ActionedSelfManagedConsumerWasteTonnage { get; init; }
        public required decimal? ResidualSelfManagedConsumerWasteTonnage { get; init; }
        public required (decimal? total, decimal? red, decimal? amber, decimal? green) NetReportedTonnage { get; init; }

        public static SelfManagedConsumerWasteData Zero => new()
        {
            SelfManagedConsumerWasteTonnage = 0,
            ActionedSelfManagedConsumerWasteTonnage = 0,
            ResidualSelfManagedConsumerWasteTonnage = 0,
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

                ResidualSelfManagedConsumerWasteTonnage =
                    (a.ResidualSelfManagedConsumerWasteTonnage ?? 0) +
                    (b.ResidualSelfManagedConsumerWasteTonnage ?? 0),

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
        public static SelfManagedConsumerWasteData Sum(this IEnumerable<SelfManagedConsumerWasteData?> source)
        {
            return source.Aggregate(
                SelfManagedConsumerWasteData.Zero,
                (acc, x) => acc + (x ?? SelfManagedConsumerWasteData.Zero)
            );
        }
    }
}