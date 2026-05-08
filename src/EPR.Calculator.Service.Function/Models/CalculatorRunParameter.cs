using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.Service.Function.Models;

/// <summary>
///     CalculatorRunParameter model class.
/// </summary>
public class CalculatorRunParameter
{
    /// <summary>
    ///     Gets or sets the identifier for the calculator run.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the user who initiated the calculator run.
    /// </summary>
    public required string User { get; set; }

    /// <summary>
    ///     Gets or sets the relative year for the calculator run.
    /// </summary>
    public required RelativeYear RelativeYear { get; set; }

    /// <summary>
    ///     Gets or sets the message type for calculator.
    /// </summary>
    public required string MessageType { get; set; }
}
