using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.Service.Function
{
    /// <summary>
    ///     Common run parameters.
    /// </summary>
    public abstract record RunParams
    {
        /// <summary>
        ///     The unique identifier of this run.
        /// </summary>
        public required int Id { get; init; }

        /// <summary>
        ///     The name of this run.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        ///     The type of run, for display purposes.
        /// </summary>
        public abstract string Type { get; }
    }

    /// <summary>
    ///     Parameters for a calculator run.
    /// </summary>
    public record CalculatorRunParams : RunParams
    {
        /// <summary>
        ///     The user who initiated the calculator run.
        /// </summary>
        public required string User { get; init; }

        /// <summary>
        ///     The relative year for the calculator to run against.
        /// </summary>
        public required RelativeYear RelativeYear { get; init; }

        public override string Type => "Calculator Run";
    }

    /// <summary>
    ///     Parameters for a billing file run.
    /// </summary>
    public record BillingRunParams : RunParams
    {
        /// <summary>
        ///     The user who approved the billing file.
        /// </summary>
        public required string ApprovedBy { get; init; }

        public override string Type => "Billing Run";
    }
}