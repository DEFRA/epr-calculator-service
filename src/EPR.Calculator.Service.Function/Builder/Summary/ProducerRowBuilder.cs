using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Builder.Summary;

internal sealed class ProducerRowBuilder(
    ImmutableDictionary<(int ProducerId, int MaterialId), decimal?> invoicedNetTonnageByProducerMaterial,
    ImmutableList<CalcResultScaledupProducer> scaledupProducers,
    ImmutableList<CalcResultPartialObligation> partialObligations,
    ImmutableDictionary<(int OrganisationId, string? SubsidiaryId), Organisation> organisationsByKey,
    ImmutableDictionary<int, Organisation> parentOrganisationsById)
{
    /// <summary>
    /// Builds a Level-1 total row for a producer group by aggregating its already-computed L2 rows.
    /// Tonnage and cost fields are additive sums from the L2 rows; SMCW-derived fields use the
    /// independently-computed Level-1 record from <paramref name="smcw"/> (cannot be derived by
    /// summing subsidiaries, because SMCW is computed at the group level).
    /// </summary>
    public CalcResultSummaryProducerDisposalFees GetL1TotalRow(
        int producerId,
        IReadOnlyList<CalcResultSummaryProducerDisposalFees> l2Rows,
        CalcResult calcResult,
        SelfManagedConsumerWaste smcw,
        IReadOnlyList<MaterialDetail> materials
    )
    {
        var materialCosts = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>();
        var commsCosts    = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>();

        var l1SmcwRecord = smcw.ProducerTotals.Single(x => x.Level == 1 && x.ProducerId == producerId);

        foreach (var material in materials)
        {
            var l2MatRows = l2Rows
                .Where(r => r.ProducerDisposalFeesByMaterial.ContainsKey(material.Code))
                .Select(r => r.ProducerDisposalFeesByMaterial[material.Code])
                .ToList();

            var l2CommsRows = l2Rows
                .Where(r => r.ProducerCommsFeesByMaterial.ContainsKey(material.Code))
                .Select(r => r.ProducerCommsFeesByMaterial[material.Code])
                .ToList();

            var l1Smcw = l1SmcwRecord.SelfManagedConsumerWasteDataPerMaterials.GetValueOrDefault(material.Code)
                ?? SelfManagedConsumerWasteData.Zero;

            invoicedNetTonnageByProducerMaterial.TryGetValue((producerId, material.Id), out var prevInvoiced);

            var l1TotalReportedTonnage = l2MatRows.Sum(r => r.TotalReportedTonnage);

            var disposalFee = l1Smcw.SelfManagedConsumerWasteTonnage > l1TotalReportedTonnage
                ? (total: (decimal?)0m, red: (decimal?)0m, amber: (decimal?)0m, green: (decimal?)0m)
                : CalcResultSummaryUtil.GetProducerDisposalFee(material, calcResult, l1Smcw);

            materialCosts[material.Code] = new CalcResultSummaryProducerDisposalFeesByMaterial
            {
                // Additive from L2 rows
                HouseholdPackagingWasteTonnage            = l2MatRows.Sum(r => r.HouseholdPackagingWasteTonnage),
                HouseholdPackagingWasteTonnageRagRating   = AggregateRagDict(l2MatRows, r => r.HouseholdPackagingWasteTonnageRagRating),
                PublicBinTonnage                          = l2MatRows.Sum(r => r.PublicBinTonnage),
                PublicBinTonnageRagRating                 = AggregateRagDict(l2MatRows, r => r.PublicBinTonnageRagRating),
                HouseholdDrinksContainersTonnage          = l2MatRows.Sum(r => r.HouseholdDrinksContainersTonnage),
                HouseholdDrinksContainersTonnageRagRating = AggregateRagDict(l2MatRows, r => r.HouseholdDrinksContainersTonnageRagRating),
                TotalReportedTonnage                      = l1TotalReportedTonnage,
                TotalReportedTonnageRagRating             = AggregateRagDict(l2MatRows, r => r.TotalReportedTonnageRagRating),

                // From L1 SMCW record — not derivable by summing L2 values
                SelfManagedConsumerWasteTonnage           = l1Smcw.SelfManagedConsumerWasteTonnage,
                ActionedSelfManagedConsumerWasteTonnage   = l1Smcw.ActionedSelfManagedConsumerWasteTonnage,
                ResidualSelfManagedConsumerWasteTonnage   = l1Smcw.ResidualSelfManagedConsumerWasteTonnage,
                NetReportedTonnage                        = l1Smcw.NetReportedTonnage,

                // Derived from L1 SMCW
                TonnageChange                             = TonnageChangeUtil.ComputePerMaterialChange(
                                                               CommonConstants.LevelOne.ToString(),
                                                               l1Smcw.NetReportedTonnage.total,
                                                               prevInvoiced),
                PricePerTonne                             = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult),
                ProducerDisposalFee                       = disposalFee,
                BadDebtProvision                          = CalcResultSummaryUtil.GetBadDebtProvision(calcResult, disposalFee.total),
                ProducerDisposalFeeWithBadDebtProvision   = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvision(calcResult, disposalFee.total),
                PreviousInvoicedTonnage                   = prevInvoiced
            };

            commsCosts[material.Code] = new CalcResultSummaryProducerCommsFeesCostByMaterial
            {
                HouseholdPackagingWasteTonnage   = l2CommsRows.Sum(r => r.HouseholdPackagingWasteTonnage),
                PublicBinTonnage                 = l2CommsRows.Sum(r => r.PublicBinTonnage),
                HouseholdDrinksContainersTonnage = l2CommsRows.Sum(r => r.HouseholdDrinksContainersTonnage),
                TotalReportedTonnage             = l2CommsRows.Sum(r => r.TotalReportedTonnage),
                PriceperTonne                    = l2CommsRows.Count > 0 ? l2CommsRows[0].PriceperTonne : 0,
                Costs                            = l2CommsRows.Select(r => r.Costs).Sum(),
            };
        }

        var producerForTotalRow = GetProducerDetailsForTotalRow(producerId, isOverAllTotalRow: false);
        var (tonnageChangeCount, tonnageChangeAdvice) = TonnageChangeUtil.ComputeCountAndAdvice(
            CommonConstants.LevelOne.ToString(), materialCosts);

        return new CalcResultSummaryProducerDisposalFees
        {
            ProducerId          = producerId,
            ProducerName        = producerForTotalRow?.OrganisationName ?? string.Empty,
            SubsidiaryId        = string.Empty,
            TradingName         = producerForTotalRow?.TradingName ?? string.Empty,
            Level               = CommonConstants.LevelOne.ToString(),
            IsProducerScaledup  = scaledupProducers.Exists(p => p.ProducerId == producerId) ? CommonConstants.Yes : CommonConstants.No,
            IsPartialObligation = partialObligations.Exists(p => p.ProducerId == producerId) ? CommonConstants.Yes : CommonConstants.No,
            StatusCode          = producerForTotalRow?.StatusCode,
            JoinerDate          = producerForTotalRow?.JoinerDate,
            LeaverDate          = producerForTotalRow?.LeaverDate,

            ProducerDisposalFeesByMaterial = materialCosts,
            ProducerCommsFeesByMaterial    = commsCosts,
            CommsCostsSection2a            = GetCommunicationCostsSectionTwoA(commsCosts),

            TonnageChangeCount  = tonnageChangeCount,
            TonnageChangeAdvice = tonnageChangeAdvice,

            LADisposalCostsSection1 = GetLocalAuthorityDisposalCostsSectionOne(materialCosts),
            CommsCostsSection2b     = l2Rows.Select(r => r.CommsCostsSection2b).Sum(),

            PercentageofProducerReportedTonnagevsAllProducers = l2Rows.Sum(r => r.PercentageofProducerReportedTonnagevsAllProducers),

            CommsCostsSection2c = l2Rows.Select(r => r.CommsCostsSection2c).Sum(),

            IsTotalRow        = true,
            IsOverallTotalRow = false,
        };
    }

    /// <summary>
    /// Builds the overall-total row by summing all Level-1 rows (one per producer group).
    /// All fields — including SMCW — are additive: the overall SMCW equals the sum of the
    /// Level-1 SMCW records by construction in <see cref="SelfManagedConsumerWasteService"/>.
    /// </summary>
    public static CalcResultSummaryProducerDisposalFees GetOverallTotalRow(
        IReadOnlyList<CalcResultSummaryProducerDisposalFees> l1Rows,
        IReadOnlyList<MaterialDetail> materials
    )
    {
        var materialCosts = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>();
        var commsCosts    = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>();

        // Accumulators for the post-loop row-level sums, folded into a single pass.
        var commsCostsSection2b = CalcResultSummaryBadDebtProvision.Empty;
        decimal percentageOfProducerTonnage = 0;
        var commsCostsSection2c = CalcResultSummaryBadDebtProvision.Empty;

        // Per-material sub-lists built in a single pass over l1Rows per material.
        var matRowsByCode   = materials.ToDictionary(m => m.Code, _ => new List<CalcResultSummaryProducerDisposalFeesByMaterial>());
        var commsRowsByCode = materials.ToDictionary(m => m.Code, _ => new List<CalcResultSummaryProducerCommsFeesCostByMaterial>());

        foreach (var row in l1Rows)
        {
            commsCostsSection2b             += row.CommsCostsSection2b;
            percentageOfProducerTonnage     += row.PercentageofProducerReportedTonnagevsAllProducers;
            commsCostsSection2c             += row.CommsCostsSection2c;

            foreach (var materialCode in materials.Select(material => material.Code))
            {
                if (row.ProducerDisposalFeesByMaterial.TryGetValue(materialCode, out var mat))
                    matRowsByCode[materialCode].Add(mat);
                if (row.ProducerCommsFeesByMaterial.TryGetValue(materialCode, out var comms))
                    commsRowsByCode[materialCode].Add(comms);
            }
        }

        foreach (var materialCode in materials.Select(material => material.Code))
        {
            var l1MatRows   = matRowsByCode[materialCode];
            var l1CommsRows = commsRowsByCode[materialCode];

            materialCosts[materialCode] = new CalcResultSummaryProducerDisposalFeesByMaterial
            {
                HouseholdPackagingWasteTonnage            = l1MatRows.Sum(r => r.HouseholdPackagingWasteTonnage),
                HouseholdPackagingWasteTonnageRagRating   = AggregateRagDict(l1MatRows, r => r.HouseholdPackagingWasteTonnageRagRating),
                PublicBinTonnage                          = l1MatRows.Sum(r => r.PublicBinTonnage),
                PublicBinTonnageRagRating                 = AggregateRagDict(l1MatRows, r => r.PublicBinTonnageRagRating),
                HouseholdDrinksContainersTonnage          = l1MatRows.Sum(r => r.HouseholdDrinksContainersTonnage),
                HouseholdDrinksContainersTonnageRagRating = AggregateRagDict(l1MatRows, r => r.HouseholdDrinksContainersTonnageRagRating),
                TotalReportedTonnage                      = l1MatRows.Sum(r => r.TotalReportedTonnage),
                TotalReportedTonnageRagRating             = AggregateRagDict(l1MatRows, r => r.TotalReportedTonnageRagRating),

                // SMCW is additive: overall SMCW = sum of Level-1 SMCW records
                SelfManagedConsumerWasteTonnage           = l1MatRows.Sum(r => r.SelfManagedConsumerWasteTonnage),
                ActionedSelfManagedConsumerWasteTonnage   = SumTupleField(l1MatRows, r => r.ActionedSelfManagedConsumerWasteTonnage),
                ResidualSelfManagedConsumerWasteTonnage   = l1MatRows.Sum(r => r.ResidualSelfManagedConsumerWasteTonnage),
                NetReportedTonnage                        = SumTupleField(l1MatRows, r => r.NetReportedTonnage),

                TonnageChange                             = l1MatRows.Sum(r => r.TonnageChange),
                PricePerTonne                             = l1MatRows.Count > 0 ? l1MatRows[0].PricePerTonne : (null, null, null, null),
                ProducerDisposalFee                       = SumTupleField(l1MatRows, r => r.ProducerDisposalFee),
                BadDebtProvision                          = l1MatRows.Sum(r => r.BadDebtProvision),
                ProducerDisposalFeeWithBadDebtProvision   = ByCountryCost.Sum([.. l1MatRows.Select(r => r.ProducerDisposalFeeWithBadDebtProvision)]),
                PreviousInvoicedTonnage                   = l1MatRows.Sum(r => r.PreviousInvoicedTonnage),
            };

            commsCosts[materialCode] = new CalcResultSummaryProducerCommsFeesCostByMaterial
            {
                HouseholdPackagingWasteTonnage   = l1CommsRows.Sum(r => r.HouseholdPackagingWasteTonnage),
                PublicBinTonnage                 = l1CommsRows.Sum(r => r.PublicBinTonnage),
                HouseholdDrinksContainersTonnage = l1CommsRows.Sum(r => r.HouseholdDrinksContainersTonnage),
                TotalReportedTonnage             = l1CommsRows.Sum(r => r.TotalReportedTonnage),
                PriceperTonne                    = l1CommsRows.Count > 0 ? l1CommsRows[0].PriceperTonne : 0,
                Costs                            = l1CommsRows.Select(r => r.Costs).Sum(),
            };
        }

        return new CalcResultSummaryProducerDisposalFees
        {
            ProducerId          = 0,
            ProducerName        = string.Empty,
            SubsidiaryId        = string.Empty,
            TradingName         = string.Empty,
            Level               = string.Empty,
            IsProducerScaledup  = string.Empty,
            IsPartialObligation = string.Empty,
            StatusCode          = null,
            JoinerDate          = null,
            LeaverDate          = CommonConstants.Totals,

            ProducerDisposalFeesByMaterial = materialCosts,

            ProducerCommsFeesByMaterial    = commsCosts,
            CommsCostsSection2a            = GetCommunicationCostsSectionTwoA(commsCosts),

            LADisposalCostsSection1       = GetLocalAuthorityDisposalCostsSectionOne(materialCosts),
            CommsCostsSection2b           = commsCostsSection2b,

            PercentageofProducerReportedTonnagevsAllProducers = percentageOfProducerTonnage,

            CommsCostsSection2c = commsCostsSection2c,

            IsTotalRow        = true,
            IsOverallTotalRow = true,
        };
    }

    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
    public CalcResultSummaryProducerDisposalFees GetProducerRow(
        RunContext runContext,
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        bool hasGroupTotalRow,
        IReadOnlyList<ProducerDetail> producerAndSubsidiaries,
        ProducerDetail producer,
        IReadOnlyList<MaterialDetail> materials,
        CalcResult calcResult,
        IReadOnlyList<TotalPackagingTonnagePerRun> totalPackagingTonnage,
        SelfManagedConsumerWaste smcw
    )
    {
        var materialCostSummary = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>();
        var commsCostSummary = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>();
        var level = hasGroupTotalRow ? (int)CalcResultSummaryLevelIndex.Two : (int)CalcResultSummaryLevelIndex.One;

        // PERF: Use O(1) lookup instead of an O(orgs) FirstOrDefault per producer row.
        organisationsByKey.TryGetValue((producer.ProducerId, producer.SubsidiaryId), out var orgData);

        var result = new CalcResultSummaryProducerDisposalFees
        {
            ProducerId          = producer.ProducerId,
            ProducerName        = producer.ProducerName ?? string.Empty,
            SubsidiaryId        = producer.SubsidiaryId ?? string.Empty,
            TradingName         = producer.TradingName ?? string.Empty,
            Level               = level.ToString(),
            IsProducerScaledup  = scaledupProducers.Exists(p => p.ProducerId == producer.ProducerId)
                ? CommonConstants.Yes
                : CommonConstants.No,
            IsPartialObligation = partialObligations.Exists(p => p.ProducerId == producer.ProducerId && p.SubsidiaryId == producer.SubsidiaryId)
                ? CommonConstants.Yes
                : CommonConstants.No,
            StatusCode          = orgData?.StatusCode,
            JoinerDate          = orgData?.JoinerDate,
            LeaverDate          = orgData?.LeaverDate
        };

        var commsSection2a = CalcResultSummaryBadDebtProvision.Empty;

        foreach (var material in materials)
        {
            // PERF: Hoist the loop invariants - both values depend only on (producerAndSubsidiaries, material)
            // and were previously recomputed once per subsidiary.
            var l1TotalReportedTonnage = producerAndSubsidiaries.Sum(p => CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, p, material));
            var l1SelfManagedConsumerWasteData = CalcResultSummaryUtil.SumSelfManagedConsumerWasteData(producerAndSubsidiaries, material, smcw);

            var producerDisposalFeesByMaterial = BuildProducerDisposalFeesByMaterial(
                runContext,
                projectedMaterialsLookup,
                producer,
                material,
                calcResult,
                smcw,
                level,
                l1TotalReportedTonnage,
                l1SelfManagedConsumerWasteData);

            materialCostSummary.Add(material.Code, producerDisposalFeesByMaterial);
            result.LADisposalCostsSection1 +=
                new CalcResultSummaryBadDebtProvision
                {
                    FeeWithoutBadDebtProvision = producerDisposalFeesByMaterial.ProducerDisposalFee.total ?? 0,
                    BadDebtProvision           = producerDisposalFeesByMaterial.BadDebtProvision,
                    FeeWithBadDebtProvision    = producerDisposalFeesByMaterial.ProducerDisposalFeeWithBadDebtProvision
                };

            var producerCommsFeesCostByMaterial = BuildProducerCommsFeesCostByMaterial(
                projectedMaterialsLookup,
                producer,
                material,
                calcResult
            );

            commsCostSummary.Add(material.Code, producerCommsFeesCostByMaterial);
            commsSection2a += producerCommsFeesCostByMaterial.Costs;
        }

        result.ProducerDisposalFeesByMaterial = materialCostSummary;
        result.ProducerCommsFeesByMaterial = commsCostSummary;

        result.CommsCostsSection2a = commsSection2a;

        result.CommsCostsSection2b = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsCosts(calcResult, producer, totalPackagingTonnage);

        var (countStr, advice) = TonnageChangeUtil.ComputeCountAndAdvice(result.Level, materialCostSummary);
        result.TonnageChangeCount  = countStr;
        result.TonnageChangeAdvice = advice;

        // Section-3: Percentage of Producer Reported Tonnage vs All Producers
        result.PercentageofProducerReportedTonnagevsAllProducers = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(producer, totalPackagingTonnage);

        TwoCCommsCostProducer.UpdateTwoCRows(calcResult, result);

        return result;
    }

    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
    [SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "This is suppressed for now and will be refactored later.")]
    private CalcResultSummaryProducerDisposalFeesByMaterial BuildProducerDisposalFeesByMaterial(
        RunContext runContext,
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        ProducerDetail producer,
        MaterialDetail material,
        CalcResult calcResult,
        SelfManagedConsumerWaste smcw,
        int level,
        decimal l1TotalReportedTonnage,
        SelfManagedConsumerWasteData l1SelfManagedConsumerWasteData
    )
    {
        // PERF: O(1) replacement for the original `Where(...).Select(...).FirstOrDefault()` scan.
        invoicedNetTonnageByProducerMaterial.TryGetValue((producer.ProducerId, material.Id), out var previousInvoicedNetTonnage);

        var hhTonnage  = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household);
        var pbTonnage  = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin);
        var hdcTonnage = material.Code == MaterialCodes.Glass
            ? CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers)
            : 0m;

        Dictionary<RagRating, decimal> hhRagTonnage   = [];
        Dictionary<RagRating, decimal> pbRagTonnage   = [];
        Dictionary<RagRating, decimal> hdcRagTonnage  = [];
        Dictionary<RagRating, decimal> totalRagTonnage = [];

        if (runContext.RequiresModulation)
        {
            foreach (var rag in Enum.GetValues<RagRating>())
            {
                hhRagTonnage[rag]  = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household, rag);
                pbRagTonnage[rag]  = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin, rag);
                hdcRagTonnage[rag] = material.Code == MaterialCodes.Glass
                    ? CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers, rag)
                    : 0m;
                totalRagTonnage[rag] = hhRagTonnage[rag] + pbRagTonnage[rag] + hdcRagTonnage[rag];
            }
        }

        var selfManagedConsumerWasteData = smcw
            .ProducerTotals
            .Find(x => x.ProducerId == producer.ProducerId && x.SubsidiaryId == producer.SubsidiaryId && x.Level == level)?
            .SelfManagedConsumerWasteDataPerMaterials[material.Code] ?? SelfManagedConsumerWasteData.Zero;

        var producerDisposalFee =
            l1SelfManagedConsumerWasteData.SelfManagedConsumerWasteTonnage > l1TotalReportedTonnage
                ? (total: 0, red: 0, amber: 0, green: 0)
                : CalcResultSummaryUtil.GetProducerDisposalFee(material, calcResult, selfManagedConsumerWasteData);

        return new CalcResultSummaryProducerDisposalFeesByMaterial
        {
            HouseholdPackagingWasteTonnage          = hhTonnage,
            HouseholdPackagingWasteTonnageRagRating = hhRagTonnage,

            PublicBinTonnage          = pbTonnage,
            PublicBinTonnageRagRating = pbRagTonnage,

            HouseholdDrinksContainersTonnage          = hdcTonnage,
            HouseholdDrinksContainersTonnageRagRating = hdcRagTonnage,

            TotalReportedTonnage          = hhTonnage + pbTonnage + hdcTonnage,
            TotalReportedTonnageRagRating = totalRagTonnage,

            SelfManagedConsumerWasteTonnage         = selfManagedConsumerWasteData.SelfManagedConsumerWasteTonnage,
            ActionedSelfManagedConsumerWasteTonnage = selfManagedConsumerWasteData.ActionedSelfManagedConsumerWasteTonnage,
            ResidualSelfManagedConsumerWasteTonnage = selfManagedConsumerWasteData.ResidualSelfManagedConsumerWasteTonnage,
            NetReportedTonnage                      = selfManagedConsumerWasteData.NetReportedTonnage,
            TonnageChange                           = TonnageChangeUtil.ComputePerMaterialChange(level.ToString(), selfManagedConsumerWasteData.NetReportedTonnage.total, previousInvoicedNetTonnage),
            PricePerTonne                           = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult),
            ProducerDisposalFee                     = producerDisposalFee,
            BadDebtProvision                        = CalcResultSummaryUtil.GetBadDebtProvision(calcResult, producerDisposalFee.total),
            ProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvision(calcResult, producerDisposalFee.total),
            PreviousInvoicedTonnage                 = previousInvoicedNetTonnage.HasValue ? previousInvoicedNetTonnage.Value : null,
        };
    }

    private static CalcResultSummaryProducerCommsFeesCostByMaterial BuildProducerCommsFeesCostByMaterial(
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        ProducerDetail producer,
        MaterialDetail material,
        CalcResult calcResult
    )
    {
        var hhTonnage  = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household);
        var pbTonnage  = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin);
        var hdcTonnage = material.Code == MaterialCodes.Glass
            ? CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers)
            : 0m;
        var totalTonnage = hhTonnage + pbTonnage + hdcTonnage;

        return new CalcResultSummaryProducerCommsFeesCostByMaterial
        {
            HouseholdPackagingWasteTonnage   = hhTonnage,
            PublicBinTonnage                 = pbTonnage,
            HouseholdDrinksContainersTonnage = hdcTonnage,
            TotalReportedTonnage             = totalTonnage,
            PriceperTonne                    = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult),
            Costs                            = CalcResultSummaryCommsCostTwoA.GetCommsFeesCosts(totalTonnage, material, calcResult)
        };
    }

    private static CalcResultSummaryBadDebtProvision GetLocalAuthorityDisposalCostsSectionOne(
        Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary
    ) =>
        materialCostSummary.Values.Select(m => new CalcResultSummaryBadDebtProvision
        {
            FeeWithoutBadDebtProvision = m.ProducerDisposalFee.total ?? 0,
            BadDebtProvision           = m.BadDebtProvision,
            FeeWithBadDebtProvision    = m.ProducerDisposalFeeWithBadDebtProvision,
        }).Sum();

    private static CalcResultSummaryBadDebtProvision GetCommunicationCostsSectionTwoA(
        Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary
    ) =>
        commsCostSummary.Values.Select(m => m.Costs).Sum();

    private static Dictionary<RagRating, decimal> AggregateRagDict(
        IReadOnlyList<CalcResultSummaryProducerDisposalFeesByMaterial> rows,
        Func<CalcResultSummaryProducerDisposalFeesByMaterial, Dictionary<RagRating, decimal>> selector
    )
    {
        if (rows.All(r => selector(r).Count == 0))
            return new();
        return Enum.GetValues<RagRating>().ToDictionary(
            rag => rag,
            rag => rows.Sum(r => selector(r).GetValueOrDefault(rag)));
    }

    private static (decimal? total, decimal? red, decimal? amber, decimal? green) SumTupleField(
        IReadOnlyList<CalcResultSummaryProducerDisposalFeesByMaterial> rows,
        Func<CalcResultSummaryProducerDisposalFeesByMaterial, (decimal? total, decimal? red, decimal? amber, decimal? green)> selector
    ) =>
        (
            total: rows.Sum(r => selector(r).total ?? 0),
            red:   rows.Sum(r => selector(r).red   ?? 0),
            amber: rows.Sum(r => selector(r).amber ?? 0),
            green: rows.Sum(r => selector(r).green ?? 0)
        );

    private Organisation? GetProducerDetailsForTotalRow(int producerId, bool isOverAllTotalRow)
    {
        if (isOverAllTotalRow)
        {
            return null;
        }

        // PERF: O(1) replacement for the previous FirstOrDefault scan of ParentOrganisations.
        parentOrganisationsById.TryGetValue(producerId, out var parentProducer);
        return parentProducer;
    }
}
