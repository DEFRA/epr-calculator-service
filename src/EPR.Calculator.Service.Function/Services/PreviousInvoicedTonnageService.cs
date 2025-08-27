using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Services
{
    public class PreviousInvoicedTonnageService : IPreviousInvoicedTonnageService
    {
        private readonly ApplicationDBContext _db;

        // Cache ids whose Status ends with "COMPLETED"
        private static volatile int[]? _completedClassIds;
        private static readonly object _gate = new();

        public PreviousInvoicedTonnageService(ApplicationDBContext db) => _db = db;

        public async Task<IReadOnlyDictionary<(int ProducerId, int MaterialId), decimal?>>
            GetPreviousInvoicedTonnageMapByRunAsync(int runId, CancellationToken ct = default)
        {
            // (1) FY id (string) for this run
            var currentFyId = await _db.CalculatorRuns
                .AsNoTracking()
                .Where(r => r.Id == runId)
                .Select(r => r.FinancialYearId)
                .SingleAsync(ct);

            // (2) Distinct producers in this run
            var producerIds = await _db.ProducerDetail
                .AsNoTracking()
                .Where(pd => pd.CalculatorRunId == runId)
                .Select(pd => pd.ProducerId)
                .Distinct()
                .ToArrayAsync(ct);

            if (producerIds.Length == 0)
                return new Dictionary<(int, int), decimal?>();

            // (3) Resolve “...COMPLETED” classification ids once
            var completedIds = _completedClassIds;
            if (completedIds is null)
            {
                var fetched = await _db.CalculatorRunClassifications
                    .AsNoTracking()
                    .Where(c => EF.Functions.Like(c.Status, "%COMPLETED")) // e.g. "Final - COMPLETED"
                    .Select(c => c.Id)
                    .ToArrayAsync(ct);

                lock (_gate)
                {
                    _completedClassIds ??= fetched;
                    completedIds = _completedClassIds;
                }
            }
            if (completedIds!.Length == 0)
                return new Dictionary<(int, int), decimal?>();

            // (4) For each producer, pick last (max run id) run in same FY that is Accepted + COMPLETED
            var lastRunPerProducer = await
            (
                from bi in _db.ProducerResultFileSuggestedBillingInstruction.AsNoTracking()
                join r in _db.CalculatorRuns.AsNoTracking() on bi.CalculatorRunId equals r.Id
                where r.FinancialYearId == currentFyId
                   && producerIds.Contains(bi.ProducerId)
                   && bi.BillingInstructionAcceptReject == "Accepted"
                   && completedIds.Contains(r.CalculatorRunClassificationId)
                group r by bi.ProducerId into g
                select new { ProducerId = g.Key, LastRunId = g.Max(x => x.Id) }
            ).ToListAsync(ct);

            if (lastRunPerProducer.Count == 0)
                return new Dictionary<(int, int), decimal?>();

            var lastRunByProducer = lastRunPerProducer.ToDictionary(x => x.ProducerId, x => x.LastRunId);
            var lastRunIds = lastRunPerProducer.Select(x => x.LastRunId).Distinct().ToArray();

            // (5) Pull invoiced tonnages for those last runs
            var rows = await _db.ProducerInvoicedMaterialNetTonnage
                .AsNoTracking()
                .Where(t => lastRunIds.Contains(t.CalculatorRunId))
                .Select(t => new { t.ProducerId, t.MaterialId, t.CalculatorRunId, t.InvoicedNetTonnage })
                .ToListAsync(ct);

            // (6) Build map (ProducerId, MaterialId) -> decimal?
            var dict = new Dictionary<(int ProducerId, int MaterialId), decimal?>(rows.Count);
            foreach (var r in rows)
            {
                if (lastRunByProducer.TryGetValue(r.ProducerId, out var last) && r.CalculatorRunId == last)
                    dict[(r.ProducerId, r.MaterialId)] = r.InvoicedNetTonnage; // may be null
            }
            return dict;
        }
    }
}
