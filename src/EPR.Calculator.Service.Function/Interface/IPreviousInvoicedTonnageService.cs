using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Interface
{
    public interface IPreviousInvoicedTonnageService
    {
        Task<IReadOnlyDictionary<(int ProducerId, int MaterialId), decimal?>>
            GetPreviousInvoicedTonnageMapByRunAsync(int runId, CancellationToken ct = default);
    }
}