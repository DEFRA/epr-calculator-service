namespace EPR.Calculator.Service.Function.Messaging;

public abstract record MessageBase
{
    /// <summary>
    ///     The unique ID of the calculator run to use.
    /// </summary>
    public required int CalculatorRunId { get; init; }
}