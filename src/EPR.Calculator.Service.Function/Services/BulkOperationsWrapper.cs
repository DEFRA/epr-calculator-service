using System.Diagnostics.CodeAnalysis;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    /// <summary>
    ///     Abstraction over <see cref="EFCore.BulkExtensions"/> to allow bulk operations to be verified in unit tests.
    /// </summary>
    public interface IBulkOperations
    {
        Task BulkInsertAsync<T>(DbContext dbContext, IEnumerable<T> entities, CancellationToken cancellationToken = default)
            where T : class;
    }

    [ExcludeFromCodeCoverage]
    public class BulkOperationsWrapper
        : IBulkOperations
    {
        public async Task BulkInsertAsync<T>(DbContext dbContext, IEnumerable<T> entities, CancellationToken cancellationToken = default)
            where T : class
        {
            await dbContext.BulkInsertAsync(entities, cancellationToken: cancellationToken);
        }
    }
}