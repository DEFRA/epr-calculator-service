namespace EPR.Calculator.Service.Function.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.API.Data.DataModels;

    public interface IOrgAndPomWrapper
    {
        bool AnyOrganisationData();
        bool AnyPomData();
        Task<IEnumerable<OrganisationData>> GetOrganisationDataAsync();
        Task<IEnumerable<PomData>> GetPomDataAsync();
        Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken cancellationToken);
    }
}
