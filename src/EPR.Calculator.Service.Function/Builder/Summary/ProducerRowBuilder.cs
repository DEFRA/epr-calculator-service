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
        var materialCosts = new Dictionary<string, CalcResultSummaryProducerFeesByMaterial>();

        var l1SmcwRecord = smcw.ProducerTotals.Single(x => x.Level == 1 && x.ProducerId == producerId);

        foreach (var material in materials)
        {
            var l2MatRows = l2Rows
                .Where(r => r.ProducerFeesByMaterial.ContainsKey(material.Code))
                .Select(r => r.ProducerFeesByMaterial[material.Code])
                .ToList();

            var l1Smcw = l1SmcwRecord.SelfManagedConsumerWasteDataPerMaterials.GetValueOrDefault(material.Code)
                ?? SelfManagedConsumerWasteData.Zero;

            invoicedNetTonnageByProducerMaterial.TryGetValue((producerId, material.Id), out var prevInvoiced);

            var l1TotalReportedTonnage = l2MatRows.Sum(r => r.DisposalFees.TotalTonnage);

            var disposalFee = l1Smcw.SelfManagedConsumerWasteTonnage > l1TotalReportedTonnage
                ? new RAMTonnageGroup { Total = 0m, Red = 0m, Amber = 0m, Green = 0m }
                : CalcResultSummaryUtil.GetProducerDisposalFee(material, calcResult, l1Smcw);

            materialCosts[material.Code] = new CalcResultSummaryProducerFeesByMaterial {
                // Additive from L2 rows
                MaterialCode = material.Code,
                DisposalFees = new CalcResultSummaryProducerDisposalFeesByMaterial
                {
                    HouseholdTonnage        = l2MatRows.Sum(r => r.DisposalFees.HouseholdTonnage),
                    HouseholdRAMTonnage     = AggregateRAM(l2MatRows, r => r.DisposalFees.HouseholdRAMTonnage),
                    PublicBinTonnage        = l2MatRows.Sum(r => r.DisposalFees.PublicBinTonnage),
                    PublicBinRAMTonnage     = AggregateRAM(l2MatRows, r => r.DisposalFees.PublicBinRAMTonnage),
                    HDCTonnage              = l2MatRows.Sum(r => r.DisposalFees.HDCTonnage),
                    HDCRAMTonnage           = AggregateRAM(l2MatRows, r => r.DisposalFees.HDCRAMTonnage),
                    TotalTonnage            = l1TotalReportedTonnage,
                    TotalRAMTonnage         = AggregateRAM(l2MatRows, r => r.DisposalFees.TotalRAMTonnage),

                    // From L1 SMCW record — not derivable by summing L2 values
                    //TODO: change smcw to RAMTonnageGroup
                    SelfManagedConsumerWasteTonnage           = l1Smcw.SelfManagedConsumerWasteTonnage,
                    ActionedSelfManagedConsumerWasteTonnage   = new RAMTonnageGroup { 
                        Total = l1Smcw.ActionedSelfManagedConsumerWasteTonnage.total, 
                        Red = l1Smcw.ActionedSelfManagedConsumerWasteTonnage.red, 
                        Amber = l1Smcw.ActionedSelfManagedConsumerWasteTonnage.amber, 
                        Green = l1Smcw.ActionedSelfManagedConsumerWasteTonnage.green 
                    },
                    ResidualSelfManagedConsumerWasteTonnage   = l1Smcw.ResidualSelfManagedConsumerWasteTonnage,
                    NetReportedTonnage                        = new RAMTonnageGroup { 
                        Total = l1Smcw.NetReportedTonnage.total, 
                        Red = l1Smcw.NetReportedTonnage.red, 
                        Amber = l1Smcw.NetReportedTonnage.amber, 
                        Green = l1Smcw.NetReportedTonnage.green 
                    },

                    // Derived from L1 SMCW
                    TonnageChange                             = TonnageChangeUtil.ComputePerMaterialChange(
                                                                CommonConstants.LevelOne.ToString(),
                                                                l1Smcw.NetReportedTonnage.total,
                                                                prevInvoiced),
                    PricePerTonne                             = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult),
                    ProducerDisposalFee                       = disposalFee,
                    BadDebtProvision                          = CalcResultSummaryUtil.GetBadDebtProvision(calcResult, disposalFee.Total),
                    ProducerDisposalFeeWithBadDebtProvision   = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvision(calcResult, disposalFee.Total),
                    PreviousInvoicedTonnage                   = prevInvoiced
                },
                CommFees                                  = new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdTonnage   = l2MatRows.Sum(r => r.CommFees.HouseholdTonnage),
                    PublicBinTonnage   = l2MatRows.Sum(r => r.CommFees.PublicBinTonnage),
                    HDCTonnage         = l2MatRows.Sum(r => r.CommFees.HDCTonnage),
                    TotalTonnage       = l2MatRows.Sum(r => r.CommFees.TotalTonnage),
                    PricePerTonne      = l2MatRows.Count > 0 ? l2MatRows.First().CommFees.PricePerTonne : 0,
                    Costs              = l2MatRows.Select(r => r.CommFees.Costs).Sum(),
                }
            };
        }

        var producerForTotalRow = GetProducerDetailsForTotalRow(producerId, isOverAllTotalRow: false);
        var (tonnageChangeCount, tonnageChangeAdvice) = TonnageChangeUtil.ComputeCountAndAdvice(
            CommonConstants.LevelOne.ToString(), materialCosts.ToDictionary(k => k.Key, v => v.Value.DisposalFees));

        return new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId     = calcResult.CalcResultDetail.RunId,
            ProducerId          = producerId,
            ProducerName        = producerForTotalRow?.OrganisationName ?? string.Empty,
            SubsidiaryId        = string.Empty,
            TradingName         = producerForTotalRow?.TradingName ?? string.Empty,
            Level               = CommonConstants.LevelOne.ToString(),
            StatusCode          = producerForTotalRow?.StatusCode,
            JoinerDate          = producerForTotalRow?.JoinerDate,
            LeaverDate          = producerForTotalRow?.LeaverDate,

            ProducerFeesByMaterial = materialCosts,
            CommsCostsSection2a            = GetCommunicationCostsSectionTwoA(materialCosts.ToDictionary(k => k.Key, v => v.Value.CommFees)),

            TonnageChangeCount  = tonnageChangeCount,
            TonnageChangeAdvice = tonnageChangeAdvice,

            LADisposalCostsSection1 = GetLocalAuthorityDisposalCostsSectionOne(materialCosts.ToDictionary(k => k.Key, v => v.Value.DisposalFees)),
            CommsCostsSection2b     = l2Rows.Select(r => r.CommsCostsSection2b).Sum(),

            PercentageofProducerReportedTonnagevsAllProducers = l2Rows.Sum(r => r.PercentageofProducerReportedTonnagevsAllProducers),

            CommsCostsSection2c = l2Rows.Select(r => r.CommsCostsSection2c).Sum()
        };
    }

    /// <summary>
    /// Builds the overall-total row by summing all Level-1 rows (one per producer group).
    /// All fields — including SMCW — are additive: the overall SMCW equals the sum of the
    /// Level-1 SMCW records by construction in <see cref="SelfManagedConsumerWasteService"/>.
    /// </summary>
    public static CalcResultSummaryProducerDisposalFees GetOverallTotalRow(
        int runId,
        IReadOnlyList<CalcResultSummaryProducerDisposalFees> l1Rows,
        IReadOnlyList<MaterialDetail> materials
    )
    {
        var materialCosts = new Dictionary<string, CalcResultSummaryProducerFeesByMaterial>();

        // Accumulators for the post-loop row-level sums, folded into a single pass.
        var commsCostsSection2b = CalcResultSummaryBadDebtProvision.Empty;
        decimal percentageOfProducerTonnage = 0;
        var commsCostsSection2c = CalcResultSummaryBadDebtProvision.Empty;

        // Per-material sub-lists built in a single pass over l1Rows per material.
        var matRowsByCode   = materials.ToDictionary(m => m.Code, _ => new List<CalcResultSummaryProducerFeesByMaterial>());

        foreach (var row in l1Rows)
        {
            commsCostsSection2b             += row.CommsCostsSection2b;
            percentageOfProducerTonnage     += row.PercentageofProducerReportedTonnagevsAllProducers;
            commsCostsSection2c             += row.CommsCostsSection2c;

            foreach (var materialCode in materials.Select(material => material.Code))
            {
                if (row.ProducerFeesByMaterial.TryGetValue(materialCode, out var mat))
                    matRowsByCode[materialCode].Add(mat);
            }
        }

        foreach (var materialCode in materials.Select(material => material.Code))
        {
            var l1MatRows   = matRowsByCode[materialCode];

            materialCosts[materialCode] = new CalcResultSummaryProducerFeesByMaterial {
                MaterialCode           = materialCode,
                DisposalFees = new CalcResultSummaryProducerDisposalFeesByMaterial
                {
                    HouseholdTonnage       = l1MatRows.Sum(r => r.DisposalFees.HouseholdTonnage),
                    HouseholdRAMTonnage    = AggregateRAM(l1MatRows, r => r.DisposalFees.HouseholdRAMTonnage),
                    PublicBinTonnage       = l1MatRows.Sum(r => r.DisposalFees.PublicBinTonnage),
                    PublicBinRAMTonnage    = AggregateRAM(l1MatRows, r => r.DisposalFees.PublicBinRAMTonnage),
                    HDCTonnage             = l1MatRows.Sum(r => r.DisposalFees.HDCTonnage),
                    HDCRAMTonnage          = AggregateRAM(l1MatRows, r => r.DisposalFees.HDCRAMTonnage),
                    TotalTonnage           = l1MatRows.Sum(r => r.DisposalFees.TotalTonnage),
                    TotalRAMTonnage        = AggregateRAM(l1MatRows, r => r.DisposalFees.TotalRAMTonnage),

                    // SMCW is additive: overall SMCW = sum of Level-1 SMCW records
                    SelfManagedConsumerWasteTonnage           = l1MatRows.Sum(r => r.DisposalFees.SelfManagedConsumerWasteTonnage),
                    ActionedSelfManagedConsumerWasteTonnage   = AggregateRAMTonnageGroup(l1MatRows, r => r.DisposalFees.ActionedSelfManagedConsumerWasteTonnage),
                    ResidualSelfManagedConsumerWasteTonnage   = l1MatRows.Sum(r => r.DisposalFees.ResidualSelfManagedConsumerWasteTonnage),
                    NetReportedTonnage                        = AggregateRAMTonnageGroup(l1MatRows, r => r.DisposalFees.NetReportedTonnage),

                    TonnageChange                             = l1MatRows.Sum(r => r.DisposalFees.TonnageChange),
                    PricePerTonne                             = l1MatRows.Count > 0 ? l1MatRows[0].DisposalFees.PricePerTonne : RAMTonnageGroup.Empty,
                    ProducerDisposalFee                       = AggregateRAMTonnageGroup(l1MatRows, r => r.DisposalFees.ProducerDisposalFee),
                    BadDebtProvision                          = l1MatRows.Sum(r => r.DisposalFees.BadDebtProvision),
                    ProducerDisposalFeeWithBadDebtProvision   = ByCountryCost.Sum([.. l1MatRows.Select(r => r.DisposalFees.ProducerDisposalFeeWithBadDebtProvision)]),
                    PreviousInvoicedTonnage                   = l1MatRows.Sum(r => r.DisposalFees.PreviousInvoicedTonnage)
                },
                CommFees               = new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdTonnage   = l1MatRows.Sum(r => r.CommFees.HouseholdTonnage),
                    PublicBinTonnage   = l1MatRows.Sum(r => r.CommFees.PublicBinTonnage),
                    HDCTonnage         = l1MatRows.Sum(r => r.CommFees.HDCTonnage),
                    TotalTonnage       = l1MatRows.Sum(r => r.CommFees.TotalTonnage),
                    PricePerTonne      = l1MatRows.Count > 0 ? l1MatRows[0].CommFees.PricePerTonne : 0,
                    Costs              = l1MatRows.Select(r => r.CommFees.Costs).Sum(),
                }
            };
        }

        return new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId     = runId,
            ProducerId          = 0,
            ProducerName        = string.Empty,
            SubsidiaryId        = string.Empty,
            TradingName         = string.Empty,
            Level               = string.Empty,
            StatusCode          = null,
            JoinerDate          = null,
            LeaverDate          = CommonConstants.Totals,

            ProducerFeesByMaterial         = materialCosts,
            CommsCostsSection2a            = GetCommunicationCostsSectionTwoA(materialCosts.ToDictionary(k => k.Key, v => v.Value.CommFees)),

            LADisposalCostsSection1       = GetLocalAuthorityDisposalCostsSectionOne(materialCosts.ToDictionary(k => k.Key, v => v.Value.DisposalFees)),
            CommsCostsSection2b           = commsCostsSection2b,

            PercentageofProducerReportedTonnagevsAllProducers = percentageOfProducerTonnage,
            IsOverallTotal = true,
            CommsCostsSection2c = commsCostsSection2c
        };
    }

    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
    public CalcResultSummaryProducerDisposalFees GetProducerRow(
        RunContext runContext,
        ILookup<(int, string?), TransformProducerReportedMaterial> projectedMaterialsLookup,
        bool hasGroupTotalRow,
        IReadOnlyList<ProducerDetail> producerAndSubsidiaries,
        ProducerDetail producer,
        IReadOnlyList<MaterialDetail> materials,
        CalcResult calcResult,
        IReadOnlyList<TotalPackagingTonnagePerRun> totalPackagingTonnage,
        SelfManagedConsumerWaste smcw
    )
    {
        var materialFeeSummary = new Dictionary<string, CalcResultSummaryProducerFeesByMaterial>();
        var level = hasGroupTotalRow ? (int)CalcResultSummaryLevelIndex.Two : (int)CalcResultSummaryLevelIndex.One;

        // PERF: Use O(1) lookup instead of an O(orgs) FirstOrDefault per producer row.
        organisationsByKey.TryGetValue((producer.ProducerId, producer.SubsidiaryId), out var orgData);

        var result = new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId     = runContext.RunId,
            ProducerId          = producer.ProducerId,
            ProducerName        = producer.ProducerName ?? string.Empty,
            SubsidiaryId        = producer.SubsidiaryId ?? string.Empty,
            TradingName         = producer.TradingName ?? string.Empty,
            Level               = level.ToString(),
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

            result.LADisposalCostsSection1 +=
                new CalcResultSummaryBadDebtProvision
                {
                    FeeWithoutBadDebtProvision = producerDisposalFeesByMaterial.ProducerDisposalFee.Total ?? 0,
                    BadDebtProvision           = producerDisposalFeesByMaterial.BadDebtProvision,
                    FeeWithBadDebtProvision    = producerDisposalFeesByMaterial.ProducerDisposalFeeWithBadDebtProvision
                };

            var producerCommsFeesCostByMaterial = BuildProducerCommsFeesCostByMaterial(
                projectedMaterialsLookup,
                producer,
                material,
                calcResult
            );

            materialFeeSummary.Add(material.Code, new CalcResultSummaryProducerFeesByMaterial {
                MaterialCode = material.Code,
                DisposalFees = producerDisposalFeesByMaterial,
                CommFees = producerCommsFeesCostByMaterial
            });
            commsSection2a += producerCommsFeesCostByMaterial.Costs;
        }

        result.ProducerFeesByMaterial = materialFeeSummary;
        result.CommsCostsSection2a = commsSection2a;

        result.CommsCostsSection2b = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsCosts(calcResult, producer, totalPackagingTonnage);

        var (countStr, advice) = TonnageChangeUtil.ComputeCountAndAdvice(result.Level, materialFeeSummary.ToDictionary(k => k.Key, v => v.Value.DisposalFees));
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
        ILookup<(int, string?), TransformProducerReportedMaterial> projectedMaterialsLookup,
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

        RAMTonnage? hhRamTonnage    = null;
        RAMTonnage? pbRamTonnage    = null;
        RAMTonnage? hdcRamTonnage   = null;
        RAMTonnage? totalRamTonnage = null;

        if (runContext.RequiresModulation)
        {
            hhRamTonnage = new RAMTonnage
            {
                Red = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household, RagRating.Red),
                Amber = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household, RagRating.Amber),
                Green = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household, RagRating.Green),
                RedMedical = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household, RagRating.RedMedical),
                AmberMedical = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household, RagRating.AmberMedical),
                GreenMedical = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household, RagRating.GreenMedical),
            };

            pbRamTonnage = new RAMTonnage
            {
                Red = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin, RagRating.Red),
                Amber = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin, RagRating.Amber),
                Green = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin, RagRating.Green),
                RedMedical = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin, RagRating.RedMedical),
                AmberMedical = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin, RagRating.AmberMedical),
                GreenMedical = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin, RagRating.GreenMedical),
            };

            hdcRamTonnage = material.Code == MaterialCodes.Glass ? new RAMTonnage
            {
                Red = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers, RagRating.Red),
                Amber = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers, RagRating.Amber),
                Green = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers, RagRating.Green),
                RedMedical = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers, RagRating.RedMedical),
                AmberMedical = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers, RagRating.AmberMedical),
                GreenMedical = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers, RagRating.GreenMedical),
            } : RAMTonnage.Empty;

            totalRamTonnage = new RAMTonnage
            {
                Red = hhRamTonnage.Red + pbRamTonnage.Red + hdcRamTonnage.Red,
                Amber = hhRamTonnage.Amber + pbRamTonnage.Amber + hdcRamTonnage.Amber,
                Green = hhRamTonnage.Green + pbRamTonnage.Green + hdcRamTonnage.Green,
                RedMedical = hhRamTonnage.RedMedical + pbRamTonnage.RedMedical + hdcRamTonnage.RedMedical,
                AmberMedical = hhRamTonnage.AmberMedical + pbRamTonnage.AmberMedical + hdcRamTonnage.AmberMedical,
                GreenMedical = hhRamTonnage.GreenMedical + pbRamTonnage.GreenMedical + hdcRamTonnage.GreenMedical,
            };
        }

        var selfManagedConsumerWasteData = smcw
            .ProducerTotals
            .Find(x => x.ProducerId == producer.ProducerId && x.SubsidiaryId == producer.SubsidiaryId && x.Level == level)?
            .SelfManagedConsumerWasteDataPerMaterials[material.Code] ?? SelfManagedConsumerWasteData.Zero;

        var producerDisposalFee =
            l1SelfManagedConsumerWasteData.SelfManagedConsumerWasteTonnage > l1TotalReportedTonnage
                ? new RAMTonnageGroup { Total = 0m, Red = 0m, Amber = 0m, Green = 0m }
                : CalcResultSummaryUtil.GetProducerDisposalFee(material, calcResult, selfManagedConsumerWasteData);

        return new CalcResultSummaryProducerDisposalFeesByMaterial
        {
            HouseholdTonnage    = hhTonnage,
            HouseholdRAMTonnage = hhRamTonnage,

            PublicBinTonnage    = pbTonnage,
            PublicBinRAMTonnage = pbRamTonnage,

            HDCTonnage          = hdcTonnage,
            HDCRAMTonnage       = hdcRamTonnage,

            TotalTonnage        = hhTonnage + pbTonnage + hdcTonnage,
            TotalRAMTonnage     = totalRamTonnage,

            SelfManagedConsumerWasteTonnage         = selfManagedConsumerWasteData.SelfManagedConsumerWasteTonnage,
            ActionedSelfManagedConsumerWasteTonnage = new RAMTonnageGroup { 
                    Total = selfManagedConsumerWasteData.ActionedSelfManagedConsumerWasteTonnage.total, 
                    Red = selfManagedConsumerWasteData.ActionedSelfManagedConsumerWasteTonnage.red, 
                    Amber = selfManagedConsumerWasteData.ActionedSelfManagedConsumerWasteTonnage.amber, 
                    Green = selfManagedConsumerWasteData.ActionedSelfManagedConsumerWasteTonnage.green 
                },
            ResidualSelfManagedConsumerWasteTonnage = selfManagedConsumerWasteData.ResidualSelfManagedConsumerWasteTonnage,
            NetReportedTonnage                      =  new RAMTonnageGroup { 
                    Total = selfManagedConsumerWasteData.NetReportedTonnage.total, 
                    Red = selfManagedConsumerWasteData.NetReportedTonnage.red, 
                    Amber = selfManagedConsumerWasteData.NetReportedTonnage.amber, 
                    Green = selfManagedConsumerWasteData.NetReportedTonnage.green 
                },
            TonnageChange                           = TonnageChangeUtil.ComputePerMaterialChange(level.ToString(), selfManagedConsumerWasteData.NetReportedTonnage.total, previousInvoicedNetTonnage),
            PricePerTonne                           = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult),
            ProducerDisposalFee                     = producerDisposalFee,
            BadDebtProvision                        = CalcResultSummaryUtil.GetBadDebtProvision(calcResult, producerDisposalFee.Total),
            ProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvision(calcResult, producerDisposalFee.Total),
            PreviousInvoicedTonnage                 = previousInvoicedNetTonnage.HasValue ? previousInvoicedNetTonnage.Value : null
        };
    }

    private static CalcResultSummaryProducerCommsFeesCostByMaterial BuildProducerCommsFeesCostByMaterial(
        ILookup<(int, string?), TransformProducerReportedMaterial> projectedMaterialsLookup,
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
            HouseholdTonnage   = hhTonnage,
            PublicBinTonnage   = pbTonnage,
            HDCTonnage         = hdcTonnage,
            TotalTonnage       = totalTonnage,
            PricePerTonne      = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult),
            Costs              = CalcResultSummaryCommsCostTwoA.GetCommsFeesCosts(totalTonnage, material, calcResult)
        };
    }

    private static CalcResultSummaryBadDebtProvision GetLocalAuthorityDisposalCostsSectionOne(
        Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary
    ) =>
        materialCostSummary.Values.Select(m => new CalcResultSummaryBadDebtProvision
        {
            FeeWithoutBadDebtProvision = m.ProducerDisposalFee.Total ?? 0,
            BadDebtProvision           = m.BadDebtProvision,
            FeeWithBadDebtProvision    = m.ProducerDisposalFeeWithBadDebtProvision,
        }).Sum();

    private static CalcResultSummaryBadDebtProvision GetCommunicationCostsSectionTwoA(
        Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary
    ) =>
        commsCostSummary.Values.Select(m => m.Costs).Sum();

    private static RAMTonnage? AggregateRAM(
        IReadOnlyList<CalcResultSummaryProducerFeesByMaterial> rows,
        Func<CalcResultSummaryProducerFeesByMaterial, RAMTonnage?> selector
    )
    {
        return rows.Aggregate((RAMTonnage?)null, (acc, src) => {
            var tonnage = selector(src);

            if(tonnage == null)
                return acc;

            var currAcc = acc ?? RAMTonnage.Empty;

            return currAcc with
            {
                Red = currAcc.Red + tonnage.Red,
                Amber = currAcc.Amber + tonnage.Amber,
                Green = currAcc.Green + tonnage.Green,
                RedMedical = currAcc.RedMedical + tonnage.RedMedical,
                AmberMedical = currAcc.AmberMedical + tonnage.AmberMedical,
                GreenMedical = currAcc.GreenMedical + tonnage.GreenMedical,
            };
        });
    }

    private static RAMTonnageGroup AggregateRAMTonnageGroup(
        IReadOnlyList<CalcResultSummaryProducerFeesByMaterial> rows,
        Func<CalcResultSummaryProducerFeesByMaterial, RAMTonnageGroup> selector
    ) =>
        rows.Aggregate(new RAMTonnageGroup { Total = 0, Red = 0, Amber = 0, Green = 0 }, (acc, src) =>
        {
            var selected = selector(src);

            return acc with 
            {
                Total = acc.Total + (selected.Total ?? 0),
                Red =   acc.Red   + (selected.Red ?? 0),
                Amber = acc.Amber + (selected.Amber ?? 0),
                Green = acc.Green + (selected.Green ?? 0)
            };
        });

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

