using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Common.Utils
{
    /// <summary>
    ///     Provides extension methods for Entity Framework's <see cref="IQueryable{T}" />.
    /// </summary>
    public static class QueryableExtensions
    {
        public static async Task<ImmutableHashSet<T>> ToImmutableHashSetAsync<T>(this IQueryable<T> queryable, CancellationToken ct = default)
        {
            var builder = ImmutableHashSet.CreateBuilder<T>();

            await foreach (var dto in queryable.AsAsyncEnumerable().WithCancellation(ct))
            {
                builder.Add(dto);
            }

            return builder.ToImmutable();
        }

        public static async Task<ImmutableArray<T>> ToImmutableArrayAsync<T>(this IQueryable<T> queryable, CancellationToken ct = default)
        {
            var builder = ImmutableArray.CreateBuilder<T>();

            await foreach (var dto in queryable.AsAsyncEnumerable().WithCancellation(ct))
            {
                builder.Add(dto);
            }

            return builder.ToImmutable();
        }
    }
}