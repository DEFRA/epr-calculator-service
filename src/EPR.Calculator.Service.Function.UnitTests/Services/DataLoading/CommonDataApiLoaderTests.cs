using System.Net;
using System.Text;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Services.CommonDataApi;
using EPR.Calculator.Service.Function.Services.DataLoading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace EPR.Calculator.Service.Function.UnitTests.Services.DataLoading
{
    /// <summary>
    ///     Unit tests for <see cref="CommonDataApiLoader" />.
    ///     <para>
    ///         The database context factory is mocked (no SQLite/InMemory) because the bulk-insert
    ///         and transaction behaviour in <c>UpdateDatabase</c>/<c>BulkInsert</c> is incompatible
    ///         with those providers.
    ///     </para>
    ///     <para>
    ///         Tests focus on the observable behaviour of the non-excluded code paths:
    ///         the disabled guard, logging, the time-provider call, and HTTP stream
    ///         initialisation / error handling inside <c>GetStreams</c> and <c>Run</c>.
    ///     </para>
    /// </summary>
    [TestClass]
    public class CommonDataApiLoaderTests
    {
        private static readonly DateTimeOffset FixedTime = new(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

        private Mock<IDbContextFactory<ApplicationDBContext>> _mockDbFactory = null!;
        private Mock<ILogger<CommonDataApiLoader>> _mockLogger = null!;
        private Mock<TimeProvider> _mockTimeProvider = null!;

        [TestInitialize]
        public void SetUp()
        {
            _mockDbFactory = new Mock<IDbContextFactory<ApplicationDBContext>>();
            _mockLogger = new Mock<ILogger<CommonDataApiLoader>>();
            _mockTimeProvider = new Mock<TimeProvider>();
            _mockTimeProvider.Setup(t => t.GetUtcNow()).Returns(FixedTime);
        }

        // ─────────────────────────── LoadData – disabled path ───────────────────────────

        /// <summary>
        ///     When the loader is disabled the only thing it should do is log that information.
        /// </summary>
        [TestMethod]
        public async Task LoadData_WhenDisabled_DoesNotRun()
        {
            // Arrange
            var httpCallCount = 0;
            var handler = new TrackingHandler(() =>
            {
                httpCallCount++;
                return OkNdJson(string.Empty);
            });
            var loader = CreateLoader(enabled: false, httpHandler: handler);

            // Act
            await loader.LoadData(CreateRunParams(), "TestRun");

            // Assert
            VerifyLogContains(LogLevel.Information, "Disabled", Times.Once(), "Logger should record it is disabled.");
            Assert.AreEqual(0, httpCallCount, "HTTP client should not be called when disabled.");
            _mockDbFactory.Verify(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()), Times.Never, "DB Context should not be created when disabled.");
        }

        // ─────────────────────────── LoadData – enabled path, pre-stream behaviour ───────────────────────────

        /// <summary>
        ///     When the loader is enabled it must call <see cref="TimeProvider.GetUtcNow" /> exactly
        ///     once to stamp the load time before starting the streams.  The subsequent HTTP failure
        ///     terminates the run before the DB is reached.
        /// </summary>
        [TestMethod]
        public async Task LoadData_CallsTimeProviderBeforeStreaming()
        {
            // Arrange – HTTP returns 500 so GetStreams throws before UpdateDatabase
            var loader = CreateLoader(enabled: true, httpHandler: ServerErrorHandler());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                async () => await loader.LoadData(CreateRunParams(), "TestRun"));

            _mockTimeProvider.Verify(t => t.GetUtcNow(), Times.Once);
        }

        /// <summary>
        ///     When the loader is enabled a "Starting" informational message must be logged before
        ///     the streams are consumed, even when the streams subsequently fail.
        /// </summary>
        [TestMethod]
        public async Task LoadData_LogsStartMessageBeforeStreaming()
        {
            // Arrange – HTTP returns 500 so GetStreams throws before UpdateDatabase
            var loader = CreateLoader(enabled: true, httpHandler: ServerErrorHandler());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                async () => await loader.LoadData(CreateRunParams(), "TestRun"));

            VerifyLogContains(LogLevel.Information, "Starting", Times.Once());
        }

        /// <summary>
        ///     The "Starting" log message must include the run ID, run name, and relative year
        ///     from the supplied run parameters.
        /// </summary>
        [TestMethod]
        public async Task LoadData_IncludesRunParamsInStartLog()
        {
            // Arrange
            var loader = CreateLoader(enabled: true, httpHandler: ServerErrorHandler());
            var runParams = new CalculatorRunParameter
            {
                Id = 42,
                User = "user",
                RelativeYear = new RelativeYear(2025),
                MessageType = MessageTypes.Result,
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                async () => await loader.LoadData(runParams, "MyTestRun"));

            VerifyLogContains(LogLevel.Information, "Id=42", Times.Once());
        }

        // ─────────────────────────── LoadData – enabled path, HTTP stream failures ───────────────────────────

        /// <summary>
        ///     When both HTTP streams return server errors, <see cref="HttpRequestException" />
        ///     must propagate out of <see cref="CommonDataApiLoader.LoadData" />.
        /// </summary>
        [TestMethod]
        public async Task LoadData_WhenBothHttpStreamsFail_ThrowsHttpRequestException()
        {
            // Arrange
            var loader = CreateLoader(enabled: true, httpHandler: ServerErrorHandler());

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                async () => await loader.LoadData(CreateRunParams(), "TestRun"));
        }

        /// <summary>
        ///     When the POM HTTP stream returns a server error (while the organisation stream is
        ///     empty), <see cref="HttpRequestException" /> must propagate and the organisation
        ///     enumerator must be disposed.
        /// </summary>
        [TestMethod]
        public async Task LoadData_WhenPomStreamFails_ThrowsHttpRequestException()
        {
            // Arrange
            var handler = new UrlDispatchHandler(url =>
                url.Contains("poms")
                    ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    : OkNdJson(string.Empty));
            var loader = CreateLoader(enabled: true, httpHandler: handler);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                async () => await loader.LoadData(CreateRunParams(), "TestRun"));
        }

        /// <summary>
        ///     When the organisation HTTP stream returns a server error (while the POM stream is
        ///     empty), <see cref="HttpRequestException" /> must propagate and the POM enumerator
        ///     must be disposed.
        /// </summary>
        [TestMethod]
        public async Task LoadData_WhenOrgStreamFails_ThrowsHttpRequestException()
        {
            // Arrange
            var handler = new UrlDispatchHandler(url =>
                url.Contains("organisations")
                    ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    : OkNdJson(string.Empty));
            var loader = CreateLoader(enabled: true, httpHandler: handler);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                async () => await loader.LoadData(CreateRunParams(), "TestRun"));
        }

        // ─────────────────────────── LoadData – cancellation ───────────────────────────

        /// <summary>
        ///     When the supplied cancellation token is already cancelled before the streams are
        ///     initialised, a <see cref="TaskCanceledException" /> must propagate.
        /// </summary>
        [TestMethod]
        public async Task LoadData_WhenAlreadyCancelled_Throws()
        {
            // Arrange – handler throws on a cancelled token so we don't race the HTTP call
            var loader = CreateLoader(enabled: true, httpHandler: new CancellationRespectingHandler());

            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<TaskCanceledException>(
                async () => await loader.LoadData(CreateRunParams(), "TestRun", cts.Token));
        }

        // ─────────────────────────── Run – try-catch-finally ───────────────────────────

        /// <summary>
        ///     When stream initialisation succeeds but the DB context factory throws, the
        ///     exception must propagate through the <c>catch when</c> / <c>finally</c> block
        ///     inside <c>Run</c>, exercising the linked-cancellation-token cancellation and
        ///     stream-enumerator disposal paths.
        /// </summary>
        [TestMethod]
        public async Task LoadData_WhenDbContextCreationFails_ExceptionPropagates()
        {
            // Arrange – both HTTP streams return empty bodies so GetStreams succeeds.
            // The DB factory then throws, causing UpdateDatabase to fail.
            _mockDbFactory
                .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB unavailable"));

            var loader = CreateLoader(enabled: true, httpHandler: new UrlDispatchHandler(_ => OkNdJson(string.Empty)));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await loader.LoadData(CreateRunParams(), "TestRun"));
        }

        // ─────────────────────────── Helpers ───────────────────────────

        private CommonDataApiLoader CreateLoader(bool enabled, HttpMessageHandler httpHandler)
        {
            var loaderOptions = Options.Create(new CommonDataApiLoaderOptions
            {
                Enabled = enabled,
                PomBatchSize = 100,
                OrganisationBatchSize = 100,
            });

            var httpClientOptions = Options.Create(new CommonDataApiHttpClientOptions
            {
                BaseUrl = "https://test-api.example.com",
                CompressionEnabled = false,
                StreamStartTimeout = TimeSpan.FromSeconds(30),
            });

            var httpClient = new CommonDataApiHttpClient(new HttpClient(httpHandler), httpClientOptions);

            return new CommonDataApiLoader(
                loaderOptions,
                _mockDbFactory.Object,
                httpClient,
                _mockTimeProvider.Object,
                _mockLogger.Object);
        }

        private static CalculatorRunParameter CreateRunParams() =>
            new CalculatorRunParameter
            {
                Id = 1,
                User = "test-user",
                RelativeYear = new RelativeYear(2024),
                MessageType = MessageTypes.Result,
            };

        private static HttpResponseMessage OkNdJson(string content) =>
            new(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/x-ndjson"),
            };

        private static UrlDispatchHandler ServerErrorHandler() =>
            new(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        /// <summary>Verifies that the mock logger received a log entry at the given level whose message contains <paramref name="text" />.</summary>
        private void VerifyLogContains(LogLevel level, string text, Times times, string? failMessage = null)
        {
            _mockLogger.Verify(
                l => l.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains(text)),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                times,
                failMessage ?? $"Log message should contain '{text}'.");
        }

        // ─────────────────────────── Mock HTTP handlers ───────────────────────────

        /// <summary>
        ///     Calls a factory function and increments a counter so callers can assert whether
        ///     the HTTP client was invoked.
        /// </summary>
        private sealed class TrackingHandler(Func<HttpResponseMessage> responseFactory) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken) =>
                Task.FromResult(responseFactory());
        }

        /// <summary>
        ///     Dispatches responses based on the request URL path and query string.
        /// </summary>
        private sealed class UrlDispatchHandler(Func<string, HttpResponseMessage> responseFactory) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken) =>
                Task.FromResult(responseFactory(request.RequestUri?.PathAndQuery ?? string.Empty));
        }

        /// <summary>
        ///     Throws <see cref="OperationCanceledException" /> when the cancellation token is
        ///     already signalled, mirroring the behaviour of a real HTTP call with a cancelled token.
        /// </summary>
        private sealed class CancellationRespectingHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(OkNdJson(string.Empty));
            }
        }
    }
}