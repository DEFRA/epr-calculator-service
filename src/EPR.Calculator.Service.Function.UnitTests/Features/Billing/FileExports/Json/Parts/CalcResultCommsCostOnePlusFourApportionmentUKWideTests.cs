using System.Text.Json;
using System.Text.Json.Nodes;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Features.Billing.FileExports.Json.Parts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.Service.Function.UnitTests.Utils;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing.FileExports.Json.Parts;

using static JsonNodeComparer;

[TestClass]
public class CalcResultCommsCostOnePlusFourApportionmentUKWideTests
{
    [TestMethod]
    public void CanCallFrom_WithValidData()
    {
        // Arrange
        var ukWideData = TestFixtures.Default.Create<CalcResultCommsCostOnePlusFourApportionment>();
        ukWideData.Name = CalcResultCommsCostBuilder.TwoBCommsCostUkWide;

        // Act
        var result = CalcResultCommsCostOnePlusFourApportionmentUKWide.From(ukWideData);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void From_ValuesAreValid()
    {
        // Arrange
        var ukWideData = TestFixtures.Default.Create<CalcResultCommsCostOnePlusFourApportionment>();
        ukWideData.Name = CalcResultCommsCostBuilder.TwoBCommsCostUkWide;

        // Act
        var result = CalcResultCommsCostOnePlusFourApportionmentUKWide.From(ukWideData);
        var json = JsonSerializer.Serialize(result);
        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json);

        // Assert
        Assert.IsNotNull(roundTrippedData);

        AssertAreEqual(ukWideData.Name,
            roundTrippedData["name"]);

        AssertAreEqual(ukWideData.England,
            roundTrippedData["englandCommsCostUKWide"]);
        AssertAreEqual(ukWideData.Wales,
            roundTrippedData["walesCommsCostUKWide"]);
        AssertAreEqual(ukWideData.Scotland,
            roundTrippedData["scotlandCommsCostUKWide"]);
        AssertAreEqual(ukWideData.NorthernIreland,
            roundTrippedData["northernIrelandCommsCostUKWide"]);

        AssertAreEqual(ukWideData.Total,
            roundTrippedData["totalCommsCostUKWide"]);
    }

    [TestMethod]
    public void From_WithNullData_ReturnsNull()
    {
        // Act
        var result = CalcResultCommsCostOnePlusFourApportionmentUKWide.From(null);

        // Assert
        Assert.IsNull(result);
    }
}