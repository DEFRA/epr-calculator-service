using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Utils;

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

    public static Task<ImmutableDictionary<TKey, T>> ToImmutableDictionaryAsync<T, TKey>(this IQueryable<T> queryable,
        Func<T, TKey> keySelector,
        CancellationToken ct = default) where TKey : notnull
    {
        return queryable.ToImmutableDictionaryAsync(keySelector, t => t, ct);
    }

    public static async Task<ImmutableDictionary<TKey, TValue>> ToImmutableDictionaryAsync<T, TKey, TValue>(this IQueryable<T> queryable,
        Func<T, TKey> keySelector,
        Func<T, TValue> valueSelector,
        CancellationToken ct = default) where TKey : notnull
    {
        var builder = ImmutableDictionary.CreateBuilder<TKey, TValue>();

        await foreach (var entity in queryable.AsAsyncEnumerable().WithCancellation(ct))
        {
            builder.Add(keySelector(entity), valueSelector(entity));
        }

        return builder.ToImmutable();
    }
}