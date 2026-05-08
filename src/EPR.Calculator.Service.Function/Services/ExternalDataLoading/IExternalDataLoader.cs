using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Features.Common;

namespace EPR.Calculator.Service.Function.Services.ExternalDataLoading;

/// <summary>
///     Loads required data from an external data source directly to the <see cref="ApplicationDBContext" />.
/// </summary>
public interface IExternalDataLoader
{
    /// <summary>
    ///     Loads POM and Organisation data for the specified calculator run.
    /// </summary>
    /// <param name="runContext">The <see cref="RunContext" /> of the calculator run.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task LoadOrgAndPomData(RunContext runContext, CancellationToken cancellationToken = default);
}
