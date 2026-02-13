namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    using System.Collections.Generic;
    using System.Linq;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Models.JsonExporter;
    using EPR.Calculator.Service.Function.UnitTests.Builder;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CommsCostByMaterial2AJsonTests
    {
        [TestMethod]
        public void From_MapsDictionaryToMaterialBreakdown_AndSetsGlassDrinksContainers()
        {
            var materials = TestDataHelper.GetMaterials();

            var comms = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>
            {
                ["AL"] = new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdPackagingWasteTonnage = 100.25m,
                    ReportedPublicBinTonnage = 50.50m,
                    TotalReportedTonnage = 150.75m,
                    HouseholdDrinksContainers = 0m,
                    PriceperTonne = 0.42m,
                    ProducerTotalCostWithoutBadDebtProvision = 10m,
                    BadDebtProvision = 1m,
                    ProducerTotalCostwithBadDebtProvision = 11m,
                    EnglandWithBadDebtProvision = 4m,
                    WalesWithBadDebtProvision = 3m,
                    ScotlandWithBadDebtProvision = 2m,
                    NorthernIrelandWithBadDebtProvision = 1m,
                },
                ["GL"] = new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdPackagingWasteTonnage = 200m,
                    ReportedPublicBinTonnage = 80m,
                    TotalReportedTonnage = 280m,
                    HouseholdDrinksContainers = 12.34m,
                    PriceperTonne = 0.3m,
                    ProducerTotalCostWithoutBadDebtProvision = 20m,
                    BadDebtProvision = 2m,
                    ProducerTotalCostwithBadDebtProvision = 22m,
                    EnglandWithBadDebtProvision = 8m,
                    WalesWithBadDebtProvision = 7m,
                    ScotlandWithBadDebtProvision = 6m,
                    NorthernIrelandWithBadDebtProvision = 5m,
                }
            };

            var result = CalcResultCommsCostByMaterial2AJson.From(comms, materials);
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
}
