using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Features.Billing.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.Services;

public interface IInvoicedProducerService
{
    public Task<ImmutableHashSet<int>> GetProducerIdsForRun(int runId, CancellationToken cancellationToken = default);
    public Task<ImmutableHashSet<int>> GetAcceptedCancelledProducerIdsForRun(int runId, CancellationToken cancellationToken = default);
    public Task<ImmutableHashSet<int>> GetInvoicedThenCancelledProducerIdsForYear(RelativeYear relativeYear, CancellationToken cancellationToken = default);
    Task<ImmutableHashSet<int>> GetInvoicedProducerIdsForYear(RelativeYear relativeYear, CancellationToken cancellationToken = default);
    public Task<ImmutableArray<InvoicedProducerRecord>> GetInvoicedProducerRecords(RelativeYear relativeYear, ImmutableHashSet<int>? producerIdFilter = null, CancellationToken cancellationToken = default);
    public Task<ImmutableArray<InvoicedProducerRecord>> GetLatestAcceptedInvoicedProducerRecords(RelativeYear relativeYear, CancellationToken cancellationToken = default);
}

public class InvoicedProducerService(
    ApplicationDBContext dbContext,
    ILogger<InvoicedProducerService> logger)
    : IInvoicedProducerService
{
    private static readonly ImmutableHashSet<RunClassification> ValidClassifications =
    [
        RunClassification.InitialRunCompleted,
        RunClassification.InterimRecalculationRunCompleted,
        RunClassification.FinalRecalculationRunCompleted,
        RunClassification.FinalRunCompleted
    ];

    public async Task<ImmutableHashSet<int>> GetProducerIdsForRun(int runId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ProducerDetail
            .Where(t => t.CalculatorRunId == runId)
            .Select(t => t.ProducerId)
            .Distinct()
            .ToImmutableHashSetAsync(cancellationToken);
    }

    public async Task<ImmutableHashSet<int>> GetAcceptedCancelledProducerIdsForRun(int runId, CancellationToken cancellationToken = default)
    {
        return await dbContext
            .ProducerResultFileSuggestedBillingInstruction
            .Where(suggestedInstruction =>
                suggestedInstruction.CalculatorRunId == runId
                && suggestedInstruction.BillingInstructionAcceptReject == BillingConstants.Action.Accepted
                && suggestedInstruction.SuggestedBillingInstruction == BillingConstants.Suggestion.Cancel)
            .Select(ctx => ctx.ProducerId)
            .Distinct()
            .ToImmutableHashSetAsync(cancellationToken);
    }

    public async Task<ImmutableHashSet<int>> GetInvoicedThenCancelledProducerIdsForYear(RelativeYear relativeYear, CancellationToken cancellationToken = default)
    {
        return await dbContext
            .CalculatorRuns
            .Join(dbContext.ProducerResultFileSuggestedBillingInstruction,
                calculatorRun => calculatorRun.Id,
                suggestedInstruction => suggestedInstruction.CalculatorRunId,
                (calculatorRun, suggestedInstruction) => new { calculatorRun, suggestedInstruction })
            .Where(ctx =>
                ValidClassifications.Contains(ctx.calculatorRun.Classification)
                && ctx.calculatorRun.RelativeYearValue == relativeYear.Value
                && ctx.suggestedInstruction.BillingInstructionAcceptReject == BillingConstants.Action.Accepted
                && ctx.suggestedInstruction.SuggestedBillingInstruction == BillingConstants.Suggestion.Cancel)
            .Select(ctx => ctx.suggestedInstruction.ProducerId)
            .Distinct()
            .ToImmutableHashSetAsync(cancellationToken);
    }

    public async Task<ImmutableHashSet<int>> GetInvoicedProducerIdsForYear(RelativeYear relativeYear, CancellationToken cancellationToken = default)
    {
        return await dbContext
            .CalculatorRuns
            .Join(dbContext.ProducerResultFileSuggestedBillingInstruction,
                calculatorRun => calculatorRun.Id,
                suggestedInstruction => suggestedInstruction.CalculatorRunId,
                (calculatorRun, suggestedInstruction) => new { calculatorRun, suggestedInstruction })
            .Where(ctx =>
                ValidClassifications.Contains(ctx.calculatorRun.Classification)
                && ctx.calculatorRun.RelativeYearValue == relativeYear.Value
                && ctx.suggestedInstruction.BillingInstructionAcceptReject == BillingConstants.Action.Accepted)
            .Select(ctx => ctx.suggestedInstruction.ProducerId)
            .Distinct()
            .ToImmutableHashSetAsync(cancellationToken);
    }

    public async Task<ImmutableArray<InvoicedProducerRecord>> GetInvoicedProducerRecords(RelativeYear relativeYear, ImmutableHashSet<int>? producerIdFilter = null, CancellationToken cancellationToken = default)
    {
        if (producerIdFilter is not null)
        {
            if (producerIdFilter.Count == 0)
                return ImmutableArray<InvoicedProducerRecord>.Empty;

            if (producerIdFilter.Count > 10000)
            {
                // Not expecting to have much more than ~5,000 entries in the filter.
                // If this warning starts triggering, the performance of this query should be revisited - it may be
                // better to chunk the filter or join on a temp table.
                logger.LogWarning("ProducerIdFilter count is high. Count:{FilterCount}", producerIdFilter.Count);
            }
        }

        var queryable = GetProjectionQuery()
            .Where(ctx =>
                ValidClassifications.Contains(ctx.CalculatorRun.Classification)
                && ctx.CalculatorRun.RelativeYearValue == relativeYear.Value
                && ctx.SuggestedInstruction.BillingInstructionAcceptReject == BillingConstants.Action.Accepted);

        if (producerIdFilter != null)
            queryable = queryable.Where(ctx => producerIdFilter.Contains(ctx.ProducerId));

        return await queryable
            .Select(ctx => new InvoicedProducerRecord
            {
                CalculatorRunId = ctx.CalculatorRun.Id,
                CalculatorName = ctx.CalculatorRun.Name,
                ProducerId = ctx.InvoicedTonnage.ProducerId,
                ProducerName = ctx.OrgDetail.OrganisationName,
                TradingName = ctx.OrgDetail.TradingName,
                MaterialId = ctx.InvoicedTonnage.MaterialId,
                InvoicedNetTonnage = ctx.InvoicedTonnage.InvoicedNetTonnage,
                BillingInstructionId = ctx.InvoiceInstruction.BillingInstructionId,
                CurrentYearInvoicedTotalAfterThisRun = ctx.InvoiceInstruction.CurrentYearInvoicedTotalAfterThisRun
            })
            .ToImmutableArrayAsync(cancellationToken);
    }


    public Task<ImmutableArray<InvoicedProducerRecord>> GetLatestAcceptedInvoicedProducerRecords(RelativeYear relativeYear, CancellationToken cancellationToken = default)
    {
        var queryable = GetProjectionQuery()
            .Where(ctx =>
                ValidClassifications.Contains(ctx.CalculatorRun.Classification)
                && ctx.CalculatorRun.RelativeYearValue == relativeYear.Value
                && ctx.SuggestedInstruction.BillingInstructionAcceptReject == BillingConstants.Action.Accepted
                && ctx.SuggestedInstruction.SuggestedBillingInstruction != BillingConstants.Suggestion.Cancel

                //not exists clause -- to exclude previous "net tonnage" and "current year invoice total to date" values if cancel bill has been accepted since.
                && !dbContext.ProducerResultFileSuggestedBillingInstruction
                    .Any(b =>
                        b.ProducerId == ctx.SuggestedInstruction.ProducerId
                        && b.BillingInstructionAcceptReject == BillingConstants.Action.Accepted
                        && b.SuggestedBillingInstruction == BillingConstants.Suggestion.Cancel
                        && b.CalculatorRunId > ctx.CalculatorRun.Id
                        && ValidClassifications.Contains(b.CalculatorRun!.Classification)
                        && b.CalculatorRun.RelativeYearValue == relativeYear.Value)
            );

        var latest = queryable
            .GroupBy(ctx => new { ctx.InvoicedTonnage.ProducerId, ctx.InvoicedTonnage.MaterialId })
            .Select(g => new
            {
                g.Key.ProducerId,
                g.Key.MaterialId,
                MaxRunId = g.Max(x => x.CalculatorRun.Id)
            });

        var query = queryable
            .Join(latest, ctx => new { ctx.InvoicedTonnage.ProducerId, ctx.InvoicedTonnage.MaterialId, RunId = ctx.CalculatorRun.Id },
                l => new { l.ProducerId, l.MaterialId, RunId = l.MaxRunId },
                (ctx, l) => new InvoicedProducerRecord
                {
                    CalculatorRunId = ctx.CalculatorRun.Id,
                    CalculatorName = ctx.CalculatorRun.Name,
                    ProducerId = ctx.InvoicedTonnage.ProducerId,
                    ProducerName = ctx.OrgDetail.OrganisationName,
                    TradingName = ctx.OrgDetail.TradingName,
                    MaterialId = ctx.InvoicedTonnage.MaterialId,
                    InvoicedNetTonnage = ctx.InvoicedTonnage.InvoicedNetTonnage,
                    BillingInstructionId = ctx.InvoiceInstruction.BillingInstructionId,
                    CurrentYearInvoicedTotalAfterThisRun = ctx.InvoiceInstruction.CurrentYearInvoicedTotalAfterThisRun
                });

        return query.ToImmutableArrayAsync(cancellationToken);
    }

    private IQueryable<Projection> GetProjectionQuery()
    {
        return dbContext
            .CalculatorRuns
            .Join(
                dbContext.ProducerResultFileSuggestedBillingInstruction,
                calculatorRun => calculatorRun.Id,
                suggestedInstruction => suggestedInstruction.CalculatorRunId,
                (calculatorRun, suggestedInstruction) => new { calculatorRun, suggestedInstruction })
            .Join(
                dbContext.ProducerInvoicedMaterialNetTonnage,
                ctx => new { ctx.suggestedInstruction.ProducerId, ctx.suggestedInstruction.CalculatorRunId },
                netTonnage => new { netTonnage.ProducerId, netTonnage.CalculatorRunId },
                (ctx, invoicedTonnage) => new { ctx.calculatorRun, ctx.suggestedInstruction, invoicedTonnage })
            .Join(
                dbContext.ProducerDesignatedRunInvoiceInstruction,
                ctx => new { ctx.suggestedInstruction.ProducerId, ctx.suggestedInstruction.CalculatorRunId },
                invoiceInstruction => new { invoiceInstruction.ProducerId, invoiceInstruction.CalculatorRunId },
                (ctx, invoiceInstruction) => new { ctx.calculatorRun, ctx.suggestedInstruction, ctx.invoicedTonnage, invoiceInstruction })
            .Join( // This basically gets the latest orgDetail for each producer
                dbContext.CalculatorRunOrganisationDataDetails
                    // Step 1: find the latest master ID per org
                    .Where(orgDetail => string.IsNullOrEmpty(orgDetail.SubsidiaryId))
                    .GroupBy(orgDetail => orgDetail.OrganisationId)
                    .Select(g => new { OrganisationId = g.Key, MaxMasterId = g.Max(d => d.CalculatorRunOrganisationDataMasterId) })

                    // Step 2: join back to get latest orgDetail ID per org
                    .Join(
                        dbContext.CalculatorRunOrganisationDataDetails
                            .Where(d => string.IsNullOrEmpty(d.SubsidiaryId))
                            .GroupBy(d => new { d.OrganisationId, d.CalculatorRunOrganisationDataMasterId })
                            .Select(g => new { g.Key.OrganisationId, g.Key.CalculatorRunOrganisationDataMasterId, MaxDetailId = g.Max(d => d.Id) }),
                        latest => new { latest.OrganisationId, CalculatorRunOrganisationDataMasterId = latest.MaxMasterId },
                        detail => new { detail.OrganisationId, detail.CalculatorRunOrganisationDataMasterId },
                        (_, detail) => detail.MaxDetailId)

                    // Step 3: join back again on the orgDetail ID to get the full record
                    .Join(
                        dbContext.CalculatorRunOrganisationDataDetails,
                        latestDetailId => latestDetailId,
                        orgDetail => orgDetail.Id,
                        (_, orgDetail) => orgDetail),
                ctx => ctx.suggestedInstruction.ProducerId,
                orgDetail => orgDetail.OrganisationId,
                (ctx, orgDetail) => new Projection
                {
                    ProducerId = ctx.suggestedInstruction.ProducerId,
                    CalculatorRun = ctx.calculatorRun,
                    SuggestedInstruction = ctx.suggestedInstruction,
                    InvoicedTonnage = ctx.invoicedTonnage,
                    InvoiceInstruction = ctx.invoiceInstruction,
                    OrgDetail = orgDetail
                });
    }

    // ⚠️ Do not use c# record constructor style, EF cannot translate it to SQL.
    private sealed record Projection
    {
        public required int ProducerId { get; init; }
        public required CalculatorRun CalculatorRun { get; init; }
        public required ProducerResultFileSuggestedBillingInstruction SuggestedInstruction { get; init; }
        public required ProducerInvoicedMaterialNetTonnage InvoicedTonnage { get; init; }
        public required ProducerDesignatedRunInvoiceInstruction InvoiceInstruction { get; init; }
        public required CalculatorRunOrganisationDataDetail OrgDetail { get; init; }
    }
}
