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
    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
    public CalcResultSummaryProducerDisposalFees GetProducerTotalRow(
        RunContext runContext,
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        IReadOnlyList<ProducerDetail> producersAndSubsidiaries,
        IReadOnlyList<MaterialDetail> materials,
        CalcResult calcResult,
        IReadOnlyList<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
        bool isOverAllTotalRow,
        IReadOnlyList<TotalPackagingTonnagePerRun> totalPackagingTonnage,
        SelfManagedConsumerWaste smcw
    )
    {
        var materialCosts = GetMaterialCosts(runContext, projectedMaterialsLookup, producersAndSubsidiaries, producerDisposalFees, materials, calcResult, isOverAllTotalRow, smcw);
        var communicationCosts = GetCommunicationCosts(projectedMaterialsLookup, producersAndSubsidiaries, materials, calcResult);

        string? tonnageChangeCount = null;
        string? tonnageChangeAdvice = null;
        string isProducerScaledUp = string.Empty;
        string isPartialObligation = string.Empty;

        if (!isOverAllTotalRow)
        {
            (tonnageChangeCount, tonnageChangeAdvice) =
                TonnageChangeUtil.ComputeCountAndAdvice(CommonConstants.LevelOne.ToString(), materialCosts);
            isProducerScaledUp = CalcResultSummaryUtil.IsProducerScaledup(producersAndSubsidiaries[0], scaledupProducers) ? CommonConstants.Yes : CommonConstants.No;
            isPartialObligation = CalcResultSummaryUtil.IsProducerPartiallyObligated(producersAndSubsidiaries[0], partialObligations, isTotalRow: true) ? CommonConstants.Yes : CommonConstants.No;
        }

        var producerForTotalRow = GetProducerDetailsForTotalRow(producersAndSubsidiaries[0].ProducerId, isOverAllTotalRow);
        const int overallTotalId = 0;

        var totalRow = new CalcResultSummaryProducerDisposalFees
        {
            ProducerIdInt       = isOverAllTotalRow ? overallTotalId : producersAndSubsidiaries[0].ProducerId,
            ProducerId          = isOverAllTotalRow ? string.Empty : producersAndSubsidiaries[0].ProducerId.ToString(),
            ProducerName        = producerForTotalRow?.OrganisationName ?? string.Empty,
            SubsidiaryId        = string.Empty,
            TradingName         = producerForTotalRow?.TradingName ?? string.Empty,
            Level               = isOverAllTotalRow ? string.Empty : CommonConstants.LevelOne.ToString(),
            IsProducerScaledup  = isProducerScaledUp,
            IsPartialObligation = isPartialObligation,
            StatusCode          = producerForTotalRow?.StatusCode,
            JoinerDate          = producerForTotalRow?.JoinerDate,
            LeaverDate          = isOverAllTotalRow ? CommonConstants.Totals : producerForTotalRow?.LeaverDate,
            ProducerDisposalFeesByMaterial = materialCosts,

            // Disposal fee summary
            TotalProducerDisposalFee = materialCosts.Sum(m => m.Value.ProducerDisposalFee.total ?? 0),
            BadDebtProvision         = materialCosts.Values.Sum(m => m.BadDebtProvision),
            TotalProducerDisposalFeeWithBadDebtProvision = materialCosts.Values.Sum(m => m.ProducerDisposalFeeWithBadDebtProvision),
            EnglandTotal             = materialCosts.Values.Sum(m => m.EnglandWithBadDebtProvision),
            WalesTotal               = materialCosts.Values.Sum(m => m.WalesWithBadDebtProvision),
            ScotlandTotal            = materialCosts.Values.Sum(m => m.ScotlandWithBadDebtProvision),
            NorthernIrelandTotal     = materialCosts.Values.Sum(m => m.NorthernIrelandWithBadDebtProvision),

            // For Comms Start
            TotalProducerCommsFee       = communicationCosts.Values.Sum(m => m.ProducerTotalCostWithoutBadDebtProvision),
            BadDebtProvisionComms       = communicationCosts.Values.Sum(m => m.BadDebtProvision),
            TotalProducerCommsFeeWithBadDebtProvision = communicationCosts.Values.Sum(m => m.ProducerTotalCostwithBadDebtProvision),
            EnglandTotalComms           = communicationCosts.Values.Sum(m => m.EnglandWithBadDebtProvision),
            WalesTotalComms             = communicationCosts.Values.Sum(m => m.WalesWithBadDebtProvision),
            ScotlandTotalComms          = communicationCosts.Values.Sum(m => m.ScotlandWithBadDebtProvision),
            NorthernIrelandTotalComms   = communicationCosts.Values.Sum(m => m.NorthernIrelandWithBadDebtProvision),
            ProducerCommsFeesByMaterial = communicationCosts,

            TonnageChangeCount  = tonnageChangeCount,
            TonnageChangeAdvice = tonnageChangeAdvice,

            // Section 1
            LocalAuthorityDisposalCostsSectionOne = GetLocalAuthorityDisposalCostsSectionOne(materialCosts),

            // Section 2a
            CommunicationCostsSectionTwoA = GetCommunicationCostsSectionTwoA(communicationCosts),

            // Section 2b
            CommunicationCostsSectionTwoB = GetCommunicationCostsSectionTwoB(calcResult, producersAndSubsidiaries, totalPackagingTonnage),

            // Section-3
            PercentageofProducerReportedTonnagevsAllProducers = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducersTotal(producersAndSubsidiaries, totalPackagingTonnage),

            isTotalRow = true,
            isOverallTotalRow = isOverAllTotalRow,
        };

        TwoCCommsCostUtil.UpdateTwoCTotals(calcResult, producerDisposalFees, isOverAllTotalRow, totalRow, producersAndSubsidiaries, totalPackagingTonnage);

        return totalRow;
    }

    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
    public CalcResultSummaryProducerDisposalFees GetProducerRow(
        RunContext runContext,
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        IReadOnlyList<CalcResultSummaryProducerDisposalFees> producerDisposalFeesLookup,
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
        var level = CalcResultSummaryUtil.GetLevelIndex(producerDisposalFeesLookup, producer);

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
            IsProducerScaledup  = CalcResultSummaryUtil.IsProducerScaledup(producer, scaledupProducers)
                ? CommonConstants.Yes
                : CommonConstants.No,
            IsPartialObligation = CalcResultSummaryUtil.IsProducerPartiallyObligated(producer, partialObligations, isTotalRow: false)
                ? CommonConstants.Yes
                : CommonConstants.No,
            StatusCode          = orgData?.StatusCode,
            JoinerDate          = orgData?.JoinerDate,
            LeaverDate          = orgData?.LeaverDate
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
            result.EnglandTotal                                 += producerDisposalFeesByMaterial.EnglandWithBadDebtProvision;
            result.WalesTotal                                   += producerDisposalFeesByMaterial.WalesWithBadDebtProvision;
            result.ScotlandTotal                                += producerDisposalFeesByMaterial.ScotlandWithBadDebtProvision;
            result.NorthernIrelandTotal                         += producerDisposalFeesByMaterial.NorthernIrelandWithBadDebtProvision;

            var producerCommsFeesCostByMaterial = BuildProducerCommsFeesCostByMaterial(
                projectedMaterialsLookup,
                producer,
                material,
                calcResult
            );

            commsCostSummary.Add(material.Code, producerCommsFeesCostByMaterial);
            result.TotalProducerCommsFee                     += producerCommsFeesCostByMaterial.ProducerTotalCostWithoutBadDebtProvision;
            result.BadDebtProvisionComms                     += producerCommsFeesCostByMaterial.BadDebtProvision;
            result.TotalProducerCommsFeeWithBadDebtProvision += producerCommsFeesCostByMaterial.ProducerTotalCostwithBadDebtProvision;
            result.EnglandTotalComms                         += producerCommsFeesCostByMaterial.EnglandWithBadDebtProvision;
            result.WalesTotalComms                           += producerCommsFeesCostByMaterial.WalesWithBadDebtProvision;
            result.ScotlandTotalComms                        += producerCommsFeesCostByMaterial.ScotlandWithBadDebtProvision;
            result.NorthernIrelandTotalComms                 += producerCommsFeesCostByMaterial.NorthernIrelandWithBadDebtProvision;
        }

        result.ProducerDisposalFeesByMaterial = materialCostSummary;
        result.ProducerCommsFeesByMaterial = commsCostSummary;

        // Section 1
        result.LocalAuthorityDisposalCostsSectionOne = new CalcResultSummaryBadDebtProvision
        {
            TotalProducerFeeWithoutBadDebtProvision  = result.TotalProducerDisposalFee,
            BadDebtProvision                         = result.BadDebtProvision,
            TotalProducerFeeWithBadDebtProvision     = result.TotalProducerDisposalFeeWithBadDebtProvision,
            EnglandTotalWithBadDebtProvision         = result.EnglandTotal,
            WalesTotalWithBadDebtProvision           = result.WalesTotal,
            ScotlandTotalWithBadDebtProvision        = result.ScotlandTotal,
            NorthernIrelandTotalWithBadDebtProvision = result.NorthernIrelandTotal
        };

        // Section 2a
        result.CommunicationCostsSectionTwoA = new CalcResultSummaryBadDebtProvision
        {
            TotalProducerFeeWithoutBadDebtProvision  = result.TotalProducerCommsFee,
            BadDebtProvision                         = result.BadDebtProvisionComms,
            TotalProducerFeeWithBadDebtProvision     = result.TotalProducerCommsFeeWithBadDebtProvision,
            EnglandTotalWithBadDebtProvision         = result.EnglandTotalComms,
            WalesTotalWithBadDebtProvision           = result.WalesTotalComms,
            ScotlandTotalWithBadDebtProvision        = result.ScotlandTotalComms,
            NorthernIrelandTotalWithBadDebtProvision = result.NorthernIrelandTotalComms
        };

        // Section 2b
        result.CommunicationCostsSectionTwoB = new CalcResultSummaryBadDebtProvision
        {
            TotalProducerFeeWithoutBadDebtProvision  = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2b(calcResult, producer, totalPackagingTonnage),
            BadDebtProvision                         = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2b(calcResult, producer, totalPackagingTonnage),
            TotalProducerFeeWithBadDebtProvision     = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2b(calcResult, producer, totalPackagingTonnage),
            EnglandTotalWithBadDebtProvision         = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebt(calcResult, producer, totalPackagingTonnage),
            WalesTotalWithBadDebtProvision           = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWalesWithBadDebt(calcResult, producer, totalPackagingTonnage),
            ScotlandTotalWithBadDebtProvision        = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsScotlandWithBadDebt(calcResult, producer, totalPackagingTonnage),
            NorthernIrelandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebt(calcResult, producer, totalPackagingTonnage)
        };

        var (countStr, advice) = TonnageChangeUtil.ComputeCountAndAdvice(result.Level, materialCostSummary);
        result.TonnageChangeCount  = countStr;
        result.TonnageChangeAdvice = advice;

        // Section-3: Percentage of Producer Reported Tonnage vs All Producers
        result.PercentageofProducerReportedTonnagevsAllProducers = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(producer, totalPackagingTonnage);

        TwoCCommsCostUtil.UpdateTwoCRows(calcResult, result, producer, totalPackagingTonnage);

        return result;
    }

    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
    [SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "This is suppressed for now and will be refactored later.")]
    private Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> GetMaterialCosts(
        RunContext runContext,
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        IReadOnlyList<ProducerDetail> producersAndSubsidiaries,
        IReadOnlyList<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
        IReadOnlyList<MaterialDetail> materials,
        CalcResult calcResult,
        bool isOverAllTotalRow,
        SelfManagedConsumerWaste smcw)
    {
        var materialCosts = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>();

        // PERF: Resolve the producer id once per call rather than re-enumerating producersAndSubsidiaries per material.
        var primaryProducerId = producersAndSubsidiaries.FirstOrDefault()?.ProducerId;

        foreach (var material in materials)
        {
            var householdPackagingWasteTonnage = CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.Household);
            var publicBinTonnage = CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.PublicBin);

            // PERF: O(1) replacement for the previous Where(...).FirstOrDefault() scan.
            decimal? previousInvoicedNetTonnage = null;
            if (primaryProducerId.HasValue)
            {
                invoicedNetTonnageByProducerMaterial.TryGetValue((primaryProducerId.Value, material.Id), out previousInvoicedNetTonnage);
            }

            var selfManagedConsumerWasteData = CalcResultSummaryUtil.SumSelfManagedConsumerWasteData(producersAndSubsidiaries, material, isOverAllTotalRow, smcw);
            var producerDisposalFee = CalcResultSummaryUtil.GetProducerDisposalFee(material, calcResult, selfManagedConsumerWasteData);

            // - Overall totals row: sum of Level-1 values
            // - Producer totals row (Level 1): per-material logic using net - previous, with null/zero handling
            decimal? tonnageChange = isOverAllTotalRow
                ? TonnageChangeUtil.GetOverallChangeTotal(producerDisposalFees, material.Code)
                : TonnageChangeUtil.ComputePerMaterialChange(
                      CommonConstants.LevelOne.ToString(),
                      selfManagedConsumerWasteData.NetReportedTonnage.total,
                      previousInvoicedNetTonnage);

            materialCosts.Add(material.Code, new CalcResultSummaryProducerDisposalFeesByMaterial
            {
                HouseholdPackagingWasteTonnage = householdPackagingWasteTonnage,
                HouseholdPackagingWasteTonnageRagRating = runContext.RequiresModulation
                    ? Enum.GetValues<RagRating>().ToDictionary(
                        rag => rag,
                        rag => CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.Household, rag))
                    : new(),

                PublicBinTonnage = publicBinTonnage,
                PublicBinTonnageRagRating = runContext.RequiresModulation
                    ? Enum.GetValues<RagRating>().ToDictionary(
                        rag => rag,
                        rag => CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.PublicBin, rag))
                    : new(),

                HouseholdDrinksContainersTonnage = material.Code == MaterialCodes.Glass
                    ? CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.HouseholdDrinksContainers)
                    : 0,
                HouseholdDrinksContainersTonnageRagRating = runContext.RequiresModulation && material.Code == MaterialCodes.Glass
                    ? Enum.GetValues<RagRating>().ToDictionary(
                        rag => rag,
                        rag => CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.HouseholdDrinksContainers, rag))
                    : new(),

                TotalReportedTonnage = CalcResultSummaryUtil.GetReportedTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material),
                TotalReportedTonnageRagRating = runContext.RequiresModulation
                    ? Enum.GetValues<RagRating>().ToDictionary(
                        rag => rag,
                        rag => CalcResultSummaryUtil.GetReportedTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, rag))
                    : new(),

                SelfManagedConsumerWasteTonnage         = selfManagedConsumerWasteData.SelfManagedConsumerWasteTonnage,
                ActionedSelfManagedConsumerWasteTonnage = selfManagedConsumerWasteData.ActionedSelfManagedConsumerWasteTonnage,
                ResidualSelfManagedConsumerWasteTonnage = selfManagedConsumerWasteData.ResidualSelfManagedConsumerWasteTonnage,
                NetReportedTonnage                      = selfManagedConsumerWasteData.NetReportedTonnage,
                PricePerTonne                           = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult),
                ProducerDisposalFee                     = producerDisposalFee,
                BadDebtProvision                        = CalcResultSummaryUtil.GetBadDebtProvision(calcResult, producerDisposalFee.total),
                ProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvision(calcResult, producerDisposalFee.total),
                EnglandWithBadDebtProvision             = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.England, producerDisposalFee.total),
                WalesWithBadDebtProvision               = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.Wales, producerDisposalFee.total),
                ScotlandWithBadDebtProvision            = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.Scotland, producerDisposalFee.total),
                NorthernIrelandWithBadDebtProvision     = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.NorthernIreland, producerDisposalFee.total),
                PreviousInvoicedTonnage                 = CalcResultSummaryUtil.GetPreviousInvoicedTonnage(producerDisposalFees, producersAndSubsidiaries, scaledupProducers, partialObligations, material, isOverAllTotalRow, previousInvoicedNetTonnage),
                TonnageChange                           = tonnageChange
            });
        }

        return materialCosts;
    }

    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
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
            EnglandWithBadDebtProvision             = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.England, producerDisposalFee.total),
            WalesWithBadDebtProvision               = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.Wales, producerDisposalFee.total),
            ScotlandWithBadDebtProvision            = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.Scotland, producerDisposalFee.total),
            NorthernIrelandWithBadDebtProvision     = CalcResultSummaryUtil.GetCountryBadDebtProvision(calcResult, Countries.NorthernIreland, producerDisposalFee.total),
            PreviousInvoicedTonnage                 = previousInvoicedNetTonnage.HasValue ? previousInvoicedNetTonnage.Value : null,
        };
    }

    private static Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> GetCommunicationCosts(
        ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
        IReadOnlyList<ProducerDetail> producersAndSubsidiaries,
        IReadOnlyList<MaterialDetail> materials,
        CalcResult calcResult
    )
    {
        var communicationCosts = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>();

        foreach (var material in materials)
        {
            communicationCosts.Add(material.Code, new CalcResultSummaryProducerCommsFeesCostByMaterial
            {
                HouseholdPackagingWasteTonnage           = CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.Household),
                PublicBinTonnage                         = CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.PublicBin),
                HouseholdDrinksContainersTonnage         = material.Code == MaterialCodes.Glass
                    ? CalcResultSummaryUtil.GetTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, PackagingTypes.HouseholdDrinksContainers)
                    : 0,
                TotalReportedTonnage                     = CalcResultSummaryCommsCostTwoA.GetTotalReportedTonnageTotal(projectedMaterialsLookup, producersAndSubsidiaries, material),
                PriceperTonne                            = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult),
                ProducerTotalCostWithoutBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvisionTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
                BadDebtProvision                         = CalcResultSummaryCommsCostTwoA.GetBadDebtProvisionForCommsCostTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
                ProducerTotalCostwithBadDebtProvision    = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvisionTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
                EnglandWithBadDebtProvision              = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForCommsTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
                WalesWithBadDebtProvision                = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForCommsTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
                ScotlandWithBadDebtProvision             = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForCommsTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
                NorthernIrelandWithBadDebtProvision      = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForCommsTotal(projectedMaterialsLookup, producersAndSubsidiaries, material, calcResult),
            });
        }

        return communicationCosts;
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
            BadDebtProvision                         = CalcResultSummaryCommsCostTwoA.GetBadDebtProvisionForCommsCost(projectedMaterialsLookup, producer, material, calcResult),
            ProducerTotalCostwithBadDebtProvision    = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult),
            EnglandWithBadDebtProvision              = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult),
            WalesWithBadDebtProvision                = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult),
            ScotlandWithBadDebtProvision             = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult),
            NorthernIrelandWithBadDebtProvision      = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult),
        };
    }

    private static CalcResultSummaryBadDebtProvision GetLocalAuthorityDisposalCostsSectionOne(
        Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
    {
        return new CalcResultSummaryBadDebtProvision
        {
            TotalProducerFeeWithoutBadDebtProvision  = materialCostSummary.Sum(m => m.Value.ProducerDisposalFee.total ?? 0),
            BadDebtProvision                         = materialCostSummary.Values.Sum(m => m.BadDebtProvision),
            TotalProducerFeeWithBadDebtProvision     = materialCostSummary.Values.Sum(m => m.ProducerDisposalFeeWithBadDebtProvision),
            EnglandTotalWithBadDebtProvision         = materialCostSummary.Values.Sum(m => m.EnglandWithBadDebtProvision),
            WalesTotalWithBadDebtProvision           = materialCostSummary.Values.Sum(m => m.WalesWithBadDebtProvision),
            ScotlandTotalWithBadDebtProvision        = materialCostSummary.Values.Sum(m => m.ScotlandWithBadDebtProvision),
            NorthernIrelandTotalWithBadDebtProvision = materialCostSummary.Values.Sum(m => m.NorthernIrelandWithBadDebtProvision)
        };
    }

    private static CalcResultSummaryBadDebtProvision GetCommunicationCostsSectionTwoA(
        Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
    {
        return new CalcResultSummaryBadDebtProvision
        {
            TotalProducerFeeWithoutBadDebtProvision  = commsCostSummary.Values.Sum(m => m.ProducerTotalCostWithoutBadDebtProvision),
            BadDebtProvision                         = commsCostSummary.Values.Sum(m => m.BadDebtProvision),
            TotalProducerFeeWithBadDebtProvision     = commsCostSummary.Values.Sum(m => m.ProducerTotalCostwithBadDebtProvision),
            EnglandTotalWithBadDebtProvision         = commsCostSummary.Values.Sum(m => m.EnglandWithBadDebtProvision),
            WalesTotalWithBadDebtProvision           = commsCostSummary.Values.Sum(m => m.WalesWithBadDebtProvision),
            ScotlandTotalWithBadDebtProvision        = commsCostSummary.Values.Sum(m => m.ScotlandWithBadDebtProvision),
            NorthernIrelandTotalWithBadDebtProvision = commsCostSummary.Values.Sum(m => m.NorthernIrelandWithBadDebtProvision),
        };
    }

    private static CalcResultSummaryBadDebtProvision GetCommunicationCostsSectionTwoB(
        CalcResult calcResult,
        IReadOnlyList<ProducerDetail> producersAndSubsidiaries,
        IReadOnlyList<TotalPackagingTonnagePerRun> totalPackagingTonnage)
    {
        return new CalcResultSummaryBadDebtProvision
        {
            TotalProducerFeeWithoutBadDebtProvision  = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2bTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
            BadDebtProvision                         = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2bTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
            TotalProducerFeeWithBadDebtProvision     = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2bTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
            EnglandTotalWithBadDebtProvision         = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
            WalesTotalWithBadDebtProvision           = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWalesWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
            ScotlandTotalWithBadDebtProvision        = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsScotlandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
            NorthernIrelandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage)
        };
    }

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
