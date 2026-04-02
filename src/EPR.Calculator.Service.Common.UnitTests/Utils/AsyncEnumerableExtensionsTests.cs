using System.Runtime.CompilerServices;
using EPR.Calculator.Service.Common.Utils;

namespace EPR.Calculator.Service.Common.UnitTests.Utils
{
    /// <summary>
    /// Unit tests for the <see cref="AsyncEnumerableExtensions"/> class.
    /// </summary>
    [TestClass]
    public class AsyncEnumerableExtensionsTests
    {
        [TestMethod]
        public async Task BufferAsync_ExactMultiple_ReturnsFullBatches()
        {
            // Arrange
            var source = GenerateAsync(1, 2, 3, 4, 5, 6);

            // Act
            var batches = await ToListAsync(source.BufferAsync(3));

            // Assert
            Assert.AreEqual(2, batches.Count);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, batches[0].ToList());
            CollectionAssert.AreEqual(new[] { 4, 5, 6 }, batches[1].ToList());
        }

        [TestMethod]
        public async Task BufferAsync_NotExactMultiple_ReturnsFinalPartialBatch()
        {
            // Arrange
            var source = GenerateAsync(1, 2, 3, 4, 5);

            // Act
            var batches = await ToListAsync(source.BufferAsync(3));

            // Assert
            Assert.AreEqual(2, batches.Count);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, batches[0].ToList());
            CollectionAssert.AreEqual(new[] { 4, 5 }, batches[1].ToList());
        }

        [TestMethod]
        public async Task BufferAsync_EmptySource_ReturnsNoBatches()
        {
            // Arrange
            var source = GenerateAsync<int>();

            // Act
            var batches = await ToListAsync(source.BufferAsync(5));

            // Assert
            Assert.AreEqual(0, batches.Count);
        }

        [TestMethod]
        public async Task BufferAsync_BatchSizeOne_ReturnsIndividualItems()
        {
            // Arrange
            var source = GenerateAsync(10, 20, 30);

            // Act
            var batches = await ToListAsync(source.BufferAsync(1));

            // Assert
            Assert.AreEqual(3, batches.Count);
            CollectionAssert.AreEqual(new[] { 10 }, batches[0].ToList());
            CollectionAssert.AreEqual(new[] { 20 }, batches[1].ToList());
            CollectionAssert.AreEqual(new[] { 30 }, batches[2].ToList());
        }

        [TestMethod]
        public async Task BufferAsync_BatchSizeLargerThanSource_ReturnsSingleBatch()
        {
            // Arrange
            var source = GenerateAsync(1, 2);

            // Act
            var batches = await ToListAsync(source.BufferAsync(100));

            // Assert
            Assert.AreEqual(1, batches.Count);
            CollectionAssert.AreEqual(new[] { 1, 2 }, batches[0].ToList());
        }

        [TestMethod]
        public async Task BufferAsync_BatchSizeZero_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var source = GenerateAsync(1, 2, 3);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
                async () => await ToListAsync(source.BufferAsync(0)));
        }

        [TestMethod]
        public async Task BufferAsync_NegativeBatchSize_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var source = GenerateAsync(1, 2, 3);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
                async () => await ToListAsync(source.BufferAsync(-1)));
        }

        [TestMethod]
        public async Task BufferAsync_CancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                async () => await ToListAsync(GenerateCancellableAsync().BufferAsync(2, cts.Token)));
        }

        private static async IAsyncEnumerable<T> GenerateAsync<T>(params T[] items)
        {
            foreach (var item in items)
            {
                await Task.Yield();
                yield return item;
            }
        }

        private static async IAsyncEnumerable<int> GenerateCancellableAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (int i = 0; ; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
                yield return i;
            }
            // ReSharper disable once IteratorNeverReturns
        }

        private static async Task<List<IList<T>>> ToListAsync<T>(IAsyncEnumerable<IList<T>> source)
        {
            var list = new List<IList<T>>();
            await foreach (var item in source)
            {
                list.Add(item);
            }

            return list;
        }
    }
}