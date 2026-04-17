using EPR.Calculator.Service.Function.Features.Billing.FileExports.Json.Parts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing.FileExports.Json.Parts;

[TestClass]
public class CommsCostByMaterial2AJsonTests
{
    [TestMethod]
    public void From_MapsDictionaryToMaterialBreakdown_AndSetsGlassDrinksContainers()
    {
        var comms = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>
        {
            ["AL"] = new()
            {
                HouseholdPackagingWasteTonnage = 100.25m,
                PublicBinTonnage = 50.50m,
                TotalReportedTonnage = 150.75m,
                HouseholdDrinksContainersTonnage = 0m,
                PriceperTonne = 0.42m,
                ProducerTotalCostWithoutBadDebtProvision = 10m,
                BadDebtProvision = 1m,
                ProducerTotalCostwithBadDebtProvision = 11m,
                EnglandWithBadDebtProvision = 4m,
                WalesWithBadDebtProvision = 3m,
                ScotlandWithBadDebtProvision = 2m,
                NorthernIrelandWithBadDebtProvision = 1m
            },
            ["GL"] = new()
            {
                HouseholdPackagingWasteTonnage = 200m,
                PublicBinTonnage = 80m,
                TotalReportedTonnage = 280m,
                HouseholdDrinksContainersTonnage = 12.34m,
                PriceperTonne = 0.3m,
                ProducerTotalCostWithoutBadDebtProvision = 20m,
                BadDebtProvision = 2m,
                ProducerTotalCostwithBadDebtProvision = 22m,
                EnglandWithBadDebtProvision = 8m,
                WalesWithBadDebtProvision = 7m,
                ScotlandWithBadDebtProvision = 6m,
                NorthernIrelandWithBadDebtProvision = 5m
            }
        };

        var result = CalcResultCommsCostByMaterial2AJson.From(comms, TestData.Materials);
        var list = result.MaterialBreakdown.ToList();

        var aluminium = list.Single(x => x.MaterialName == "Aluminium");
        Assert.AreEqual(100.25m, aluminium.HouseholdPackagingWasteTonnage);
        Assert.AreEqual(50.50m, aluminium.PublicBinTonnage);
        Assert.AreEqual(150.75m, aluminium.TotalTonnage);
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(0.42m, 4), aluminium.PricePerTonne);
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(10m), aluminium.ProducerTotalCostWithoutBadDebtProvision);
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(1m), aluminium.BadDebtProvision);
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(11m), aluminium.ProducerTotalCostwithBadDebtProvision);
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(4m), aluminium.EnglandWithBadDebtProvision);
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(3m), aluminium.WalesWithBadDebtProvision);
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(2m), aluminium.ScotlandWithBadDebtProvision);
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(1m), aluminium.NorthernIrelandWithBadDebtProvision);

        var glass = list.Single(x => x.MaterialName == "Glass");
        Assert.AreEqual(200m, glass.HouseholdPackagingWasteTonnage);
        Assert.AreEqual(80m, glass.PublicBinTonnage);
        Assert.AreEqual(280m, glass.TotalTonnage);
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(0.3m, 4), glass.PricePerTonne);
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(20m), glass.ProducerTotalCostWithoutBadDebtProvision);
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(2m), glass.BadDebtProvision);
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(22m), glass.ProducerTotalCostwithBadDebtProvision);
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(8m), glass.EnglandWithBadDebtProvision);
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(7m), glass.WalesWithBadDebtProvision);
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(6m), glass.ScotlandWithBadDebtProvision);
        Assert.AreEqual(CurrencyConverterUtils.ConvertToCurrency(5m), glass.NorthernIrelandWithBadDebtProvision);
        Assert.AreEqual(12.34m, glass.HouseholdDrinksContainersTonnageGlass);
    }
}