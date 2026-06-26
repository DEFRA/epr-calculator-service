using System.Text.Json;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.JsonExporter.Model;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.UnitTests.JsonExporter.Model;

[TestClass]
public class CommsCostByMaterial2AJsonTests
{
    [TestMethod]
    public void From_MapsDictionaryToMaterialBreakdown_AndSetsGlassDrinksContainers()
    {
        // Arrange
        var materials = TestDataHelper.GetMaterialDetails();

        var comms = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>
        {
            ["AL"] = new CalcResultSummaryProducerCommsFeesCostByMaterial
            {
                HouseholdTonnage         = 100.25m,
                PublicBinTonnage         = 50.50m,
                TotalTonnage     = 150.75m,
                HDCTonnage               = 0m,
                PricePerTonne            = 0.42m,
                Costs = new CalcResultSummaryBadDebtProvision
                {
                    FeeWithoutBadDebtProvision = 10m,
                    BadDebtProvision           = 1m,
                    FeeWithBadDebtProvision    = new ByCountryCost { England = 4m, Wales = 3m, Scotland = 2m, NorthernIreland = 1m },
                },
            },
            ["GL"] = new CalcResultSummaryProducerCommsFeesCostByMaterial
            {
                HouseholdTonnage         = 200m,
                PublicBinTonnage         = 80m,
                TotalTonnage             = 280m,
                HDCTonnage               = 12.34m,
                PricePerTonne            = 0.3m,
                Costs = new CalcResultSummaryBadDebtProvision
                {
                    FeeWithoutBadDebtProvision = 20m,
                    BadDebtProvision           = 2m,
                    FeeWithBadDebtProvision    = new ByCountryCost { England = 8m, Wales = 7m, Scotland = 6m, NorthernIreland = 5m },
                },
            }
        };

        // Act
        var result = CalcResultCommsCostByMaterial2AJson.From(comms, materials);

         // Assert
        Assert.IsNotNull(result);

        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);

        var expectedJson = """
            {
            "materialBreakdown": [
              {
                "materialName": "Aluminium",
                "householdPackagingWasteTonnage": 100.25,
                "publicBinTonnage": 50.50,
                "totalTonnage": 150.75,
                "pricePerTonne": "£0.4200",
                "producerTotalCostWithoutBadDebtProvision": "£10.00",
                "badDebtProvision": "£1.00",
                "producerTotalCostWithBadDebtProvision": "£10.00",
                "englandWithBadDebtProvision": "£4.00",
                "walesWithBadDebtProvision": "£3.00",
                "scotlandWithBadDebtProvision": "£2.00",
                "northernIrelandWithBadDebtProvision": "£1.00"
              },
              {
                "materialName": "Glass",
                "householdPackagingWasteTonnage": 200,
                "publicBinTonnage": 80,
                "totalTonnage": 280,
                "householdDrinksContainersTonnageGlass": 12.34,
                "pricePerTonne": "£0.3000",
                "producerTotalCostWithoutBadDebtProvision": "£20.00",
                "badDebtProvision": "£2.00",
                "producerTotalCostWithBadDebtProvision": "£26.00",
                "englandWithBadDebtProvision": "£8.00",
                "walesWithBadDebtProvision": "£7.00",
                "scotlandWithBadDebtProvision": "£6.00",
                "northernIrelandWithBadDebtProvision": "£5.00"
              }
            ]
            }
            """;

        JsonTestUtils.AssertJson(expectedJson, json);
    }
}
