using System.Runtime.CompilerServices;

namespace EPR.Calculator.Service.Common.Utils
{
    /// <summary>
    ///     Provides extension methods for <see cref="IAsyncEnumerable{T}" />.
    /// </summary>
    public static class AsyncEnumerableExtensions
    {
        /// <summary>
        ///     Buffers items from an async enumerable into fixed-size batches.
        ///     The final batch may contain fewer items if the source count is not
        ///     evenly divisible by <paramref name="batchSize" />.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
        /// <param name="source">The async enumerable to buffer.</param>
        /// <param name="batchSize">The maximum number of items per batch. Must be greater than zero.</param>
        /// <param name="cancellationToken">A token to cancel the async enumeration.</param>
        /// <returns>An async enumerable of read-only collections, each containing up to <paramref name="batchSize" /> items.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="batchSize" /> is less than or equal to zero.</exception>
        public static IAsyncEnumerable<IList<T>> BufferAsync<T>(this IAsyncEnumerable<T> source,
            int batchSize,
            CancellationToken cancellationToken = default)
        {
            if (batchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchSize), batchSize, "Batch size must be greater than zero.");

            return BufferAsyncIterator(source, batchSize, cancellationToken);
        }

        private static async IAsyncEnumerable<IList<T>> BufferAsyncIterator<T>(IAsyncEnumerable<T> source, int batchSize,
            [EnumeratorCancellation] CancellationToken ct)
        {
            List<T> batch = new(batchSize);

            await foreach (var item in source.WithCancellation(ct))
            {
                batch.Add(item);

                if (batch.Count == batchSize)
                {
                    yield return batch;
                    batch = new List<T>(batchSize);
                }
            }

            // return the final partial batch (if any)
            if (batch.Count > 0)
            {
                yield return batch;
            }
        }
    }
}