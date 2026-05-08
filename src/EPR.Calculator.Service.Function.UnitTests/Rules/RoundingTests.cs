using EPR.Calculator.Service.Function.Rules;

namespace EPR.Calculator.Service.Function.UnitTests.Rules;

[TestClass]
public class RoundingTests
{
    [DataRow(1000d, "1")]
    [DataRow(499.9d, "0.5")]
    [DataRow(499.5d, "0.5")]
    [DataRow(499.4d, "0.499")]
    [DataRow(499.0d, "0.499")]
    [DataRow(0d, "0")]
    [TestMethod]
    public void CanCallConvertKilogramToTonne(double weight, string expected)
    {
        // Act
        var result = Rounding.KilogramsToTonnes(weight);

        // Assert
        result.ShouldBe(decimal.Parse(expected));
    }
}