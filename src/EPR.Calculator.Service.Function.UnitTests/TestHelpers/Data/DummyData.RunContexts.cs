using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.Data;

public static partial class DummyData
{
    public static class RunContexts
    {
        /// <summary>
        ///     2026 (modulation) calculator run
        /// </summary>
        public static CalculatorRunContext CalculatorRun2026 =>
            new()
            {
                RunId = 20260001,
                RelativeYear = 2026,
                RunName = "2026 Calculator run (modulation)",
                ProcessingStartedAt = DateTimeOffset.Parse("2026-01-01T00:00:00Z"),
                User = "Test User"
            };

        /// <summary>
        ///     2026 (modulation) billing run
        /// </summary>
        public static BillingRunContext BillingRun2026 =>
            new()
            {
                RunId = 20260011,
                RelativeYear = 2026,
                RunName = "2026 Billing run (modulation)",
                ProcessingStartedAt = DateTimeOffset.Parse("2026-01-01T00:00:00Z"),
                User = "Test User",
                AcceptedProducerIds = [1, 2]
            };

        /// <summary>
        ///     2025 (pre-modulation) calculator run
        /// </summary>
        public static CalculatorRunContext CalculatorRun2025 =>
            new()
            {
                RunId = 20250001,
                RelativeYear = 2025,
                RunName = "2025 Calculator run (pre-modulation)",
                ProcessingStartedAt = DateTimeOffset.Parse("2025-01-01T00:00:00Z"),
                User = "Test User"
            };

        /// <summary>
        ///     2025 (pre-modulation) billing run
        /// </summary>
        public static BillingRunContext BillingRun2025 =>
            new()
            {
                RunId = 20250011,
                RelativeYear = 2025,
                RunName = "2025 Billing run (pre-modulation)",
                ProcessingStartedAt = DateTimeOffset.Parse("2025-01-01T00:00:00Z"),
                User = "Test User",
                AcceptedProducerIds = [1, 2]
            };

        /// <summary>
        ///     2024 (pre-modulation) calculator run
        /// </summary>
        public static CalculatorRunContext CalculatorRun2024 =>
            new()
            {
                RunId = 20240001,
                RelativeYear = 2024,
                RunName = "2024 Calculator run (pre-modulation)",
                ProcessingStartedAt = DateTimeOffset.Parse("2024-01-01T00:00:00Z"),
                User = "Test User"
            };

        /// <summary>
        ///     2024 (pre-modulation) billing run
        /// </summary>
        public static BillingRunContext BillingRun2024 =>
            new()
            {
                RunId = 20240011,
                RelativeYear = 2024,
                RunName = "2024 Billing run (pre-modulation)",
                ProcessingStartedAt = DateTimeOffset.Parse("2024-01-01T00:00:00Z"),
                User = "Test User",
                AcceptedProducerIds = [1, 2]
            };
    }
}
