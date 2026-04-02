using EPR.Calculator.API.Data.DataModels;

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
