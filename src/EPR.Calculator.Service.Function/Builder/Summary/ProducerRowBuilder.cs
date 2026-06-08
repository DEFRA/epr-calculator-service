using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using static EPR.Calculator.Service.Function.Builder.Summary.Common.TonnageChangeUtil;

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
        IReadOnlyList<MaterialDetail> materials)
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
                HouseholdPackagingWasteTonnage           = l2CommsRows.Sum(r => r.HouseholdPackagingWasteTonnage),
                PublicBinTonnage                         = l2CommsRows.Sum(r => r.PublicBinTonnage),
                HouseholdDrinksContainersTonnage         = l2CommsRows.Sum(r => r.HouseholdDrinksContainersTonnage),
                TotalReportedTonnage                     = l2CommsRows.Sum(r => r.TotalReportedTonnage),
                PriceperTonne                            = l2CommsRows.Count > 0 ? l2CommsRows[0].PriceperTonne : 0,
                ProducerTotalCostWithoutBadDebtProvision = l2CommsRows.Sum(r => r.ProducerTotalCostWithoutBadDebtProvision),
                BadDebtProvision                         = l2CommsRows.Sum(r => r.BadDebtProvision),
                ProducerDisposalFeeWithBadDebtProvision  = ByCountryCost.Sum([.. l2CommsRows.Select(r => r.ProducerDisposalFeeWithBadDebtProvision)])
            };
        }

        var producerForTotalRow = GetProducerDetailsForTotalRow(producerId, isOverAllTotalRow: false);
        var (tonnageChangeCount, tonnageChangeAdvice) = TonnageChangeUtil.ComputeCountAndAdvice(
            CommonConstants.LevelOne.ToString(), materialCosts);

        return new CalcResultSummaryProducerDisposalFees
        {
            ProducerIdInt       = producerId,
            ProducerId          = producerId.ToString(),
            ProducerName        = producerForTotalRow?.OrganisationName ?? string.Empty,
            SubsidiaryId        = string.Empty,
            TradingName         = producerForTotalRow?.TradingName ?? string.Empty,
            Level               = CommonConstants.LevelOne.ToString(),
            IsProducerScaledup  = scaledupProducers.Any(p => p.ProducerId == producerId) ? CommonConstants.Yes : CommonConstants.No,
            IsPartialObligation = partialObligations.Any(p => p.ProducerId == producerId) ? CommonConstants.Yes : CommonConstants.No,
            StatusCode          = producerForTotalRow?.StatusCode,
            JoinerDate          = producerForTotalRow?.JoinerDate,
            LeaverDate          = producerForTotalRow?.LeaverDate,

            ProducerDisposalFeesByMaterial               = materialCosts,
            TotalProducerDisposalFee                     = materialCosts.Sum(m => m.Value.ProducerDisposalFee.total ?? 0),
            BadDebtProvision                             = materialCosts.Values.Sum(m => m.BadDebtProvision),
            TotalProducerDisposalFeeWithBadDebtProvision = ByCountryCost.Sum([.. materialCosts.Values.Select(m => m.ProducerDisposalFeeWithBadDebtProvision)]),

            ProducerCommsFeesByMaterial               = commsCosts,
            TotalProducerCommsFee                     = commsCosts.Values.Sum(m => m.ProducerTotalCostWithoutBadDebtProvision),
            BadDebtProvisionComms                     = commsCosts.Values.Sum(m => m.BadDebtProvision),
            TotalProducerCommsFeeWithBadDebtProvision = ByCountryCost.Sum([.. commsCosts.Values.Select(m => m.ProducerDisposalFeeWithBadDebtProvision)]),

            TonnageChangeCount  = tonnageChangeCount,
            TonnageChangeAdvice = tonnageChangeAdvice,

            LocalAuthorityDisposalCostsSectionOne = GetLocalAuthorityDisposalCostsSectionOne(materialCosts),
            CommunicationCostsSectionTwoA         = GetCommunicationCostsSectionTwoA(commsCosts),
            CommunicationCostsSectionTwoB         = SumBadDebtProvision(l2Rows, r => r.CommunicationCostsSectionTwoB),

            PercentageofProducerReportedTonnagevsAllProducers = l2Rows.Sum(r => r.PercentageofProducerReportedTonnagevsAllProducers),

            TwoCTotalProducerFeeForCommsCostsWithoutBadDebt = l2Rows.Sum(r => r.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt),
            TwoCBadDebtProvision                            = l2Rows.Sum(r => r.TwoCBadDebtProvision),
            TwoCTotalProducerFeeForCommsCostsWithBadDebt    = ByCountryCost.Sum([.. l2Rows.Select(r => r.TwoCTotalProducerFeeForCommsCostsWithBadDebt)]),

            isTotalRow        = true,
            isOverallTotalRow = false,
        };
    }

    /// <summary>
    /// Builds the overall-total row by summing all Level-1 rows (one per producer group).
    /// All fields — including SMCW — are additive: the overall SMCW equals the sum of the
    /// Level-1 SMCW records by construction in <see cref="SelfManagedConsumerWasteService"/>.
    /// </summary>
    public CalcResultSummaryProducerDisposalFees GetOverallTotalRow(
        IReadOnlyList<CalcResultSummaryProducerDisposalFees> l1Rows,
        IReadOnlyList<MaterialDetail> materials)
    {
        var materialCosts = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>();
        var commsCosts    = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>();

        foreach (var material in materials)
        {
            var l1MatRows = l1Rows
                .Where(r => r.ProducerDisposalFeesByMaterial.ContainsKey(material.Code))
                .Select(r => r.ProducerDisposalFeesByMaterial[material.Code])
                .ToList();

            var l1CommsRows = l1Rows
                .Where(r => r.ProducerCommsFeesByMaterial.ContainsKey(material.Code))
                .Select(r => r.ProducerCommsFeesByMaterial[material.Code])
                .ToList();

            materialCosts[material.Code] = new CalcResultSummaryProducerDisposalFeesByMaterial
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

            commsCosts[material.Code] = new CalcResultSummaryProducerCommsFeesCostByMaterial
            {
                HouseholdPackagingWasteTonnage           = l1CommsRows.Sum(r => r.HouseholdPackagingWasteTonnage),
                PublicBinTonnage                         = l1CommsRows.Sum(r => r.PublicBinTonnage),
                HouseholdDrinksContainersTonnage         = l1CommsRows.Sum(r => r.HouseholdDrinksContainersTonnage),
                TotalReportedTonnage                     = l1CommsRows.Sum(r => r.TotalReportedTonnage),
                PriceperTonne                            = l1CommsRows.Count > 0 ? l1CommsRows[0].PriceperTonne : 0,
                ProducerTotalCostWithoutBadDebtProvision = l1CommsRows.Sum(r => r.ProducerTotalCostWithoutBadDebtProvision),
                BadDebtProvision                         = l1CommsRows.Sum(r => r.BadDebtProvision),
                ProducerDisposalFeeWithBadDebtProvision  = ByCountryCost.Sum([.. l1CommsRows.Select(r => r.ProducerDisposalFeeWithBadDebtProvision)]),
            };
        }

        return new CalcResultSummaryProducerDisposalFees
        {
            ProducerIdInt       = 0,
            ProducerId          = string.Empty,
            ProducerName        = string.Empty,
            SubsidiaryId        = string.Empty,
            TradingName         = string.Empty,
            Level               = string.Empty,
            IsProducerScaledup  = string.Empty,
            IsPartialObligation = string.Empty,
            StatusCode          = null,
            JoinerDate          = null,
            LeaverDate          = CommonConstants.Totals,

            ProducerDisposalFeesByMaterial               = materialCosts,
            TotalProducerDisposalFee                     = materialCosts.Sum(m => m.Value.ProducerDisposalFee.total ?? 0),
            BadDebtProvision                             = materialCosts.Values.Sum(m => m.BadDebtProvision),
            TotalProducerDisposalFeeWithBadDebtProvision = ByCountryCost.Sum([.. materialCosts.Values.Select(m => m.ProducerDisposalFeeWithBadDebtProvision)]),

            ProducerCommsFeesByMaterial               = commsCosts,
            TotalProducerCommsFee                     = commsCosts.Values.Sum(m => m.ProducerTotalCostWithoutBadDebtProvision),
            BadDebtProvisionComms                     = commsCosts.Values.Sum(m => m.BadDebtProvision),
            TotalProducerCommsFeeWithBadDebtProvision = ByCountryCost.Sum([.. commsCosts.Values.Select(m => m.ProducerDisposalFeeWithBadDebtProvision)]),

            LocalAuthorityDisposalCostsSectionOne = GetLocalAuthorityDisposalCostsSectionOne(materialCosts),
            CommunicationCostsSectionTwoA         = GetCommunicationCostsSectionTwoA(commsCosts),
            CommunicationCostsSectionTwoB         = SumBadDebtProvision(l1Rows, r => r.CommunicationCostsSectionTwoB),

            PercentageofProducerReportedTonnagevsAllProducers = l1Rows.Sum(r => r.PercentageofProducerReportedTonnagevsAllProducers),

            TwoCTotalProducerFeeForCommsCostsWithoutBadDebt = l1Rows.Sum(r => r.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt),
            TwoCBadDebtProvision                            = l1Rows.Sum(r => r.TwoCBadDebtProvision),
            TwoCTotalProducerFeeForCommsCostsWithBadDebt = ByCountryCost.Sum([.. l1Rows.Select(r => r.TwoCTotalProducerFeeForCommsCostsWithBadDebt)]),

            isTotalRow        = true,
            isOverallTotalRow = true,
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
            ProducerIdInt       = producer.ProducerId,
            ProducerId          = producer.ProducerId.ToString(),
            ProducerName        = producer.ProducerName ?? string.Empty,
            SubsidiaryId        = producer.SubsidiaryId ?? string.Empty,
            TradingName         = producer.TradingName ?? string.Empty,
            Level               = level.ToString(),
            IsProducerScaledup  = scaledupProducers.Any(p => p.ProducerId == producer.ProducerId)
                ? CommonConstants.Yes
                : CommonConstants.No,
            IsPartialObligation = CalcResultSummaryUtil.IsProducerPartiallyObligated(producer, partialObligations, isTotalRow: false)
                ? CommonConstants.Yes
                : CommonConstants.No,
            StatusCode          = orgData?.StatusCode,
            JoinerDate          = orgData?.JoinerDate,
            LeaverDate          = orgData?.LeaverDate,
            TwoCTotalProducerFeeForCommsCostsWithBadDebt = ByCountryCost.Empty,
            TotalProducerDisposalFeeWithBadDebtProvision = ByCountryCost.Empty,
            TotalProducerCommsFeeWithBadDebtProvision    = ByCountryCost.Empty,
        };

        foreach (var material in materials)
        {
            // PERF: Hoist the loop invariants - both values depend only on (producerAndSubsidiaries, material)
            // and were previously recomputed once per subsidiary.
            var l1TotalReportedTonnage = producerAndSubsidiaries.Sum(p => CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, p, material));
            var l1SelfManagedConsumerWasteData = CalcResultSummaryUtil.SumSelfManagedConsumerWasteData(producerAndSubsidiaries, material, isOverAllTotalRow: false, smcw);

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
            result.TotalProducerDisposalFee                     += producerDisposalFeesByMaterial.ProducerDisposalFee.total ?? 0;
            result.BadDebtProvision                             += producerDisposalFeesByMaterial.BadDebtProvision;
            result.TotalProducerDisposalFeeWithBadDebtProvision += producerDisposalFeesByMaterial.ProducerDisposalFeeWithBadDebtProvision;

            var producerCommsFeesCostByMaterial = BuildProducerCommsFeesCostByMaterial(
                projectedMaterialsLookup,
                producer,
                material,
                calcResult
            );

            commsCostSummary.Add(material.Code, producerCommsFeesCostByMaterial);
            result.TotalProducerCommsFee                     += producerCommsFeesCostByMaterial.ProducerTotalCostWithoutBadDebtProvision;
            result.BadDebtProvisionComms                     += producerCommsFeesCostByMaterial.BadDebtProvision;
            result.TotalProducerCommsFeeWithBadDebtProvision += producerCommsFeesCostByMaterial.ProducerDisposalFeeWithBadDebtProvision;
        }

        result.ProducerDisposalFeesByMaterial = materialCostSummary;
        result.ProducerCommsFeesByMaterial = commsCostSummary;

        // Section 1
        result.LocalAuthorityDisposalCostsSectionOne = new CalcResultSummaryBadDebtProvision
        {
            // TODO result is missing CalcResultSummaryBadDebtProvision?
            FeeWithoutBadDebtProvision = result.TotalProducerDisposalFee,
            BadDebtProvision           = result.BadDebtProvision,
            FeeWithBadDebtProvision    = result.TotalProducerDisposalFeeWithBadDebtProvision
        };

        // Section 2a
        result.CommunicationCostsSectionTwoA = new CalcResultSummaryBadDebtProvision
        {
            FeeWithoutBadDebtProvision = result.TotalProducerCommsFee,
            BadDebtProvision           = result.BadDebtProvisionComms,
            FeeWithBadDebtProvision    = result.TotalProducerCommsFeeWithBadDebtProvision
        };

        // Section 2b
        result.CommunicationCostsSectionTwoB = new CalcResultSummaryBadDebtProvision
        {
            FeeWithoutBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2b(calcResult, producer, totalPackagingTonnage),
            BadDebtProvision           = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2b(calcResult, producer, totalPackagingTonnage),
            FeeWithBadDebtProvision    = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWithBadDebt(calcResult, producer, totalPackagingTonnage)
        };

        var (countStr, advice) = TonnageChangeUtil.ComputeCountAndAdvice(result.Level, materialCostSummary);
        result.TonnageChangeCount  = countStr;
        result.TonnageChangeAdvice = advice;

        // Section-3: Percentage of Producer Reported Tonnage vs All Producers
        result.PercentageofProducerReportedTonnagevsAllProducers = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(producer, totalPackagingTonnage);

        TwoCCommsCostProducer.UpdateTwoCRows(calcResult, result, producer, totalPackagingTonnage);

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
        SelfManagedConsumerWasteData l1SelfManagedConsumerWasteData)
    {
        // PERF: O(1) replacement for the original `Where(...).Select(...).FirstOrDefault()` scan.
        invoicedNetTonnageByProducerMaterial.TryGetValue((producer.ProducerId, material.Id), out var previousInvoicedNetTonnage);

        var totalReportedTonnage = CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, producer, material);

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
            HouseholdPackagingWasteTonnage = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household),
            HouseholdPackagingWasteTonnageRagRating = runContext.RequiresModulation
                ? Enum.GetValues<RagRating>().ToDictionary(
                    rag => rag,
                    rag => CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household, rag))
                : new(),

            PublicBinTonnage = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin),
            PublicBinTonnageRagRating = runContext.RequiresModulation
                ? Enum.GetValues<RagRating>().ToDictionary(
                    rag => rag,
                    rag => CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin, rag))
                : new(),

            HouseholdDrinksContainersTonnage = material.Code == MaterialCodes.Glass
                ? CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers)
                : new(),
            HouseholdDrinksContainersTonnageRagRating = runContext.RequiresModulation && material.Code == MaterialCodes.Glass
                ? Enum.GetValues<RagRating>().ToDictionary(
                    rag => rag,
                    rag => CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers, rag))
                : new(),

            TotalReportedTonnage = totalReportedTonnage,
            TotalReportedTonnageRagRating = runContext.RequiresModulation
                ? Enum.GetValues<RagRating>().ToDictionary(
                    rag => rag,
                    rag => CalcResultSummaryUtil.GetReportedTonnage(projectedMaterialsLookup, producer, material, rag))
                : new(),

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
        return new CalcResultSummaryProducerCommsFeesCostByMaterial
        {
            HouseholdPackagingWasteTonnage           = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household),
            PublicBinTonnage                         = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin),
            HouseholdDrinksContainersTonnage         = material.Code == MaterialCodes.Glass
                ? CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers)
                : new(),
            TotalReportedTonnage                     = CalcResultSummaryCommsCostTwoA.GetTotalReportedTonnage(projectedMaterialsLookup, producer, material),
            PriceperTonne                            = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult),
            ProducerTotalCostWithoutBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult),
            BadDebtProvision                         = CalcResultSummaryCommsCostTwoA.GetBadDebtProvisionForCommsCost(projectedMaterialsLookup, producer, material, calcResult).Total,
            ProducerDisposalFeeWithBadDebtProvision  = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult)
        };
    }

    private static CalcResultSummaryBadDebtProvision GetLocalAuthorityDisposalCostsSectionOne(
        Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
    {
        return new CalcResultSummaryBadDebtProvision
        {
            FeeWithoutBadDebtProvision = materialCostSummary.Sum(m => m.Value.ProducerDisposalFee.total ?? 0),
            BadDebtProvision           = materialCostSummary.Values.Sum(m => m.BadDebtProvision),
            FeeWithBadDebtProvision    = ByCountryCost.Sum([.. materialCostSummary.Values.Select(m => m.ProducerDisposalFeeWithBadDebtProvision)])
        };
    }

    private static CalcResultSummaryBadDebtProvision GetCommunicationCostsSectionTwoA(
        Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
    {
        return new CalcResultSummaryBadDebtProvision
        {
            FeeWithoutBadDebtProvision = commsCostSummary.Values.Sum(m => m.ProducerTotalCostWithoutBadDebtProvision),
            BadDebtProvision           = commsCostSummary.Values.Sum(m => m.BadDebtProvision),
            FeeWithBadDebtProvision    = ByCountryCost.Sum([.. commsCostSummary.Values.Select(m => m.ProducerDisposalFeeWithBadDebtProvision)])
        };
    }

    private static CalcResultSummaryBadDebtProvision SumBadDebtProvision(
        IReadOnlyList<CalcResultSummaryProducerDisposalFees> rows,
        Func<CalcResultSummaryProducerDisposalFees, CalcResultSummaryBadDebtProvision?> selector)
    {
        // TODO just sum the rows
        return new CalcResultSummaryBadDebtProvision
        {
            FeeWithoutBadDebtProvision = rows.Sum(r => selector(r)?.FeeWithoutBadDebtProvision ?? 0),
            BadDebtProvision           = rows.Sum(r => selector(r)?.BadDebtProvision ?? 0),
            FeeWithBadDebtProvision    = ByCountryCost.Sum([.. rows.Select(r => selector(r)?.FeeWithBadDebtProvision ?? ByCountryCost.Empty)])
        };
    }

    private static Dictionary<RagRating, decimal> AggregateRagDict(
        IReadOnlyList<CalcResultSummaryProducerDisposalFeesByMaterial> rows,
        Func<CalcResultSummaryProducerDisposalFeesByMaterial, Dictionary<RagRating, decimal>> selector)
    {
        if (rows.All(r => selector(r).Count == 0))
            return new();
        return Enum.GetValues<RagRating>().ToDictionary(
            rag => rag,
            rag => rows.Sum(r => selector(r).GetValueOrDefault(rag)));
    }

    private static (decimal? total, decimal? red, decimal? amber, decimal? green) SumTupleField(
        IReadOnlyList<CalcResultSummaryProducerDisposalFeesByMaterial> rows,
        Func<CalcResultSummaryProducerDisposalFeesByMaterial, (decimal? total, decimal? red, decimal? amber, decimal? green)> selector) =>
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
