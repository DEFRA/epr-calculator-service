using EPR.Calculator.Service.Function.Data.DataModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Interface
{
    public interface IOrgAndPomWrapper
    {
        bool AnyOrganisationData();
        bool AnyPomData();
        Task<IEnumerable<OrganisationData>> GetOrganisationDataAsync();
        Task<IEnumerable<PomData>> GetPomDataAsync();
        Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken cancellationToken);
    }
}
