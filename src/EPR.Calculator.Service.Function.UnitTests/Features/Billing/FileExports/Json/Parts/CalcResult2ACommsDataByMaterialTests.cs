using EPR.Calculator.Service.Function.Features.Billing.FileExports.Json.Parts;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing.FileExports.Json.Parts;

[TestClass]
public class CalcResult2ACommsDataByMaterialTests
{
    [TestMethod]
    public void From_MapsMaterialsAndTotal()
    {
        // Arrange
        var comms = GetCalcResultCommsCostCommsCostByMaterials();

        // Act
        var result = CalcResult2ACommsDataByMaterial.From(comms);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.CalcResult2aCommsDataDetails.Any());

        var first = result.CalcResult2aCommsDataDetails.First();
        Assert.IsNotNull(first);
        Assert.AreEqual("Aluminium", first.MaterialName);
        Assert.AreEqual("£0.4200", first.CommsCostByMaterialPricePerTonne);

        Assert.IsNotNull(result.CalcResult2aCommsDataDetailsTotal);
    }

    private static List<CalcResultCommsCostCommsCostByMaterial> GetCalcResultCommsCostCommsCostByMaterials()
    {
        return new List<CalcResultCommsCostCommsCostByMaterial>
        {
            new()
            {
                CommsCostByMaterialPricePerTonne = "0.42",
                CommsCostByMaterialPricePerTonneValue = 0.42m,
                Name = "Aluminium"
            },
            new()
            {
                CommsCostByMaterialPricePerTonne = "0.3",
                CommsCostByMaterialPricePerTonneValue = 0.3m,
                Name = "Glass"
            },
            new()
            {
                CommsCostByMaterialPricePerTonne = "0.51",
                CommsCostByMaterialPricePerTonneValue = 0.51m,
                Name = "Total"
            }
        };
    }
}